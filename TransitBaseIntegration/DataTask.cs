using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TransitBaseIntegration.Gtfs;
using TransitBaseTransformation;

namespace TransitBaseIntegration
{
    public class DataTask
    {
        public delegate void LogHandler(int progress, string message);
        public event LogHandler Log;
        private string linksFile;
        private bool download;

        private AppSourceConfig config;

        public DataTask(string linksFile, bool download = true)
        {
            this.linksFile = linksFile;
            this.download = download;
            this.config = AppSourceConfig.ReadFrom(linksFile);
        }

        public async Task Execute()
        {
            DeletePrevs();
            if (download)
                DownloadAndDecompress();
            await CreateDatabase();

            ReportProgress(0, "DataTask done!");
            ReportProgress(100);
        }

        private void DeletePrevs()
        {
            ReportProgress(0, "Deleting previous files!");
            TryClearDirectory(config.TargetPath);
            if (download)
                TryClearDirectory(config.TempFilesPath);
        }
        private void TryClearDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }

        private void DownloadAndDecompress()
        {
            Directory.CreateDirectory("temp");
            int downloadProgress = 0;
            Parallel.For(0, config.Size, i =>
            {
                string link = config.Sources[i];
                string zipFile = Path.Combine(config.TempFilesPath, i + ".zip");
                string decompressedDir = Path.Combine(config.TempFilesPath, "gtfs" + i);

                ReportProgress(0, "Downloading: " + link);
                downloadFile(link, zipFile);
                Directory.CreateDirectory(decompressedDir);
                decompressFile(zipFile, decompressedDir);
                ReportProgress(0, "Done: " + link);
                ReportProgress(++downloadProgress * 100 / config.Size);
            });
            ReportProgress(0, "Downloading complete!");

            ReportProgress(0, "Repairing database!");
            for (int i = 0; i < config.Size; i++)
            {
                if (!File.Exists(Path.Combine(config.TempFilesPath, "gtfs" + i, "calendar.txt")))
                    createCalendar(Path.Combine(config.TempFilesPath, "gtfs" + i));
                checkAgencies(Path.Combine(config.TempFilesPath, "gtfs" + i));
                checkRoutes(Path.Combine(config.TempFilesPath, "gtfs" + i));
                if (config.StopTimesSortRequired)
                    orderStopTimes(Path.Combine(config.TempFilesPath, "gtfs" + i));
            }
        }

        private async Task CreateDatabase()
        {
            ReportProgress(0, "Creating database!");

            string outputPath = Path.Combine(config.TargetPath, "decompressed");
            var sources = Enumerable.Range(0, config.Size).Select(i => Path.Combine(config.TempFilesPath, "gtfs" + i));

            Directory.CreateDirectory(outputPath);
            DbCreateTask dbCreateTask = new DbCreateTask(new GtfsDistributedDatabase(sources), outputPath, config);

            dbCreateTask.Log += ReportProgress;
            bool success = dbCreateTask.Execute();
            if (!success)
            {
                string transfersHistoryPath = Path.Combine(config.KnowledgePath, "history");
                await StreetPathData.StreetPathDataProgram.Execute(transfersHistoryPath, outputPath, config.LatitudeUnit, config.LongitudeUnit, x => ReportProgress(0, x));
                string newTransfersFile = Directory.EnumerateFiles(transfersHistoryPath).OrderByDescending(x => int.Parse(Path.GetFileNameWithoutExtension(x))).First();
                File.Delete(Path.Combine(config.KnowledgePath, "transfers.xml"));
                File.Copy(newTransfersFile, Path.Combine(config.KnowledgePath, "transfers.xml"));

                dbCreateTask = new DbCreateTask(new GtfsDistributedDatabase(sources), outputPath, config);
                dbCreateTask.Log += ReportProgress;
                success = dbCreateTask.Execute();
                if (!success)
                    throw new Exception("Final creation not successfull.");
            }

            ReportProgress(0, "Compressing!");
            var resultFile = new ZipFile(Path.Combine(config.TargetPath, "database.zip"));
            resultFile.AddFiles(Directory.GetFiles(outputPath), false, "");
            resultFile.Save();
        }

        private void ReportProgress(int progress, string message=null)
        {
            if (Log != null)
                Log(progress, message);
        }

        #region database repairement

        private void createCalendar(string path)
        {
            using (GtfsTable table = new GtfsTable(path + "\\calendar_dates.txt"))
            using (StreamWriter outFile = new StreamWriter(new FileStream(path + "\\calendar.txt", FileMode.Create, FileAccess.Write)))
            {
                HashSet<string> ids = new HashSet<string>();

                outFile.WriteLine("service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date");
                foreach (var record in table.Records)
                {
                    if (!ids.Contains(record["service_id"]))
                    {
                        ids.Add(record["service_id"]);
                        outFile.WriteLine(record["service_id"] + ",0,0,0,0,0,0,0,20120101,20120101");
                    }
                }
            }
        }

        private void checkAgencies(string path)
        {
            bool isWrong = false;
            using (StreamReader reader = new StreamReader(path + "\\agency.txt"))
            {
                var firstLine = reader.ReadLine();
                if (!firstLine.Contains("agency_id"))
                    isWrong = true;
            }
            if (isWrong)
            {
                var lines = AppSourceConfig.ReadLines(path + "\\agency.txt").ToList();
                if (lines.First().Split(',').First() != "agency_name")
                    throw new FormatException("Invalid agency.txt header order.");
                string fullName = lines.ElementAt(1).Split(',').First();
                string shortName = new string(fullName.Where(ch => char.IsUpper(ch)).ToArray());
                File.Delete(path + "\\agency.txt");
                bool firstLine = true;
                using (StreamWriter writer = new StreamWriter(path + "\\agency.txt"))
                {
                    foreach (var line in lines)
                    {
                        if (firstLine)
                        {
                            writer.Write("agency_id,");
                            firstLine = false;
                        }
                        else
                            writer.Write(shortName + ",");
                        writer.WriteLine(line);
                    }
                }
            }
        }

        private void checkRoutes(string path)
        {
            bool isWrong = false;
            using (StreamReader reader = new StreamReader(path + "\\routes.txt"))
            {
                var firstLine = reader.ReadLine();
                if (!firstLine.Contains("agency_id"))
                    isWrong = true;
            }
            if (isWrong)
            {
                var lines = AppSourceConfig.ReadLines(path + "\\routes.txt").ToList();
                File.Delete(path + "\\routes.txt");
                string agencyId = AppSourceConfig.ReadLines(path + "\\agency.txt").ElementAt(1).Split(',').First();
                bool firstLine = true;
                using (StreamWriter writer = new StreamWriter(path + "\\routes.txt"))
                {
                    foreach (var line in lines)
                    {
                        writer.Write(line);
                        if (firstLine)
                        {
                            writer.WriteLine(",agency_id");
                            firstLine = false;
                        }
                        else
                            writer.WriteLine("," + agencyId);
                    }
                }
            }
        }

        private void orderStopTimes(string path)
        {
            var header = AppSourceConfig.ReadLines(Path.Combine(path, "stop_times.txt")).First();
            var lines = AppSourceConfig.ReadLines(Path.Combine(path, "stop_times.txt")).Skip(1).ToList();
            lines.Sort();
            File.Delete(Path.Combine(path, "stop_times.txt"));
            using (StreamWriter writer = new StreamWriter(Path.Combine(path, "stop_times.txt")))
            {
                writer.WriteLine(header);
                foreach (var line in lines)
                    writer.WriteLine(line);
            }
        }

        #endregion

        private void downloadFile(string url, string path)
        {
            bool error = true;
            while (error)
            {
                try
                {
                    using (BigWebClient client = new BigWebClient())
                    {
                        client.DownloadFile(url, path);
                    }
                    error = false;
                }
                catch (IOException e)
                {
                    ReportProgress(0, "Error in [" + url + "], retrying!");
                }
                catch (WebException ex)
                {
                    ReportProgress(0, "Error in [" + url + "], retrying!");
                }
            }
        }
        private class BigWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = int.MaxValue;
                return w;
            }
        }

        private void decompressFile(string zipFile, string outputDir)
        {
            new ZipFile(zipFile).ExtractAll(outputDir);
        }
    }
}
