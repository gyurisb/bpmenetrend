using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TransitBase;
using TransitBaseTransformation.Tools;

namespace StreetPathData
{
    public class StreetPathDataProgram
    {
        static string knowledgeRoot;
        static string databaseRoot;
        static bool taskDone = false;
        static PathFinder pathFinder;
        static Action<string> reportProgress;

        #region separate data collaction legacy

        //static void Main(string[] args)
        //{
        //    MainTask().Wait();
        //    if (Debugger.IsAttached)
        //    {
        //        Console.WriteLine("Main process done, press any key to escape.");
        //        Console.ReadKey();
        //    }
        //}

        //static void LoadBudapest()
        //{
        //    BkvMenetrend.Config.Current.LatitudeDegreeDistance = 111180.5537835114;
        //    BkvMenetrend.Config.Current.LongitudeDegreeDistance = 75343.5388426138;
        //    knowledgeRoot = "knowledge";
        //    databaseRoot = "database-bp";
        //}
        //static void LoadNewYork()
        //{
        //    BkvMenetrend.Config.Current.LatitudeDegreeDistance = 111049.0506675487;
        //    BkvMenetrend.Config.Current.LongitudeDegreeDistance = 84452.25134526618;
        //    knowledgeRoot = "knowledge-ny";
        //    databaseRoot = "database-ny";
        //}
        //static void LoadChicago()
        //{
        //    BkvMenetrend.Config.Current.LatitudeDegreeDistance = 111070.8886574498;
        //    BkvMenetrend.Config.Current.LongitudeDegreeDistance = 83008.61752467098;
        //    knowledgeRoot = "knowledge-chi";
        //    databaseRoot = "database-chi";
        //}
        //static void LoadWashington()
        //{
        //    BkvMenetrend.Config.Current.LatitudeDegreeDistance = 111013.68585541553;
        //    BkvMenetrend.Config.Current.LongitudeDegreeDistance = 86739.3448851524;
        //    knowledgeRoot = "knowledge-dc";
        //    databaseRoot = "database-dc";
        //}
        //static void LoadLosAngeles()
        //{
        //    BkvMenetrend.Config.Current.LatitudeDegreeDistance = 110923.30880197878;
        //    BkvMenetrend.Config.Current.LongitudeDegreeDistance = 92328.1433370971;
        //    knowledgeRoot = "knowledge-la";
        //    databaseRoot = "database-la";
        //}

        //static async Task MainTask()
        //{
        //    BkvMenetrend.Config.Current = new BkvMenetrend.Config();
        //    LoadBudapest();
        //    //LoadNewYork();
        //    //LoadChicago();
        //    //LoadWashington();
        //    //LoadLosAngeles();
        //    await Execute(knowledgeRoot, databaseRoot, BkvMenetrend.Config.Current.LatitudeDegreeDistance, BkvMenetrend.Config.Current.LongitudeDegreeDistance, x => Console.WriteLine(x));
        //}

        #endregion

        public static async Task Execute(string knowledgeRoot, string databaseRoot, double latitudeDegreeDistance, double longitudeDegreeDistance, Action<string> reportProgress)
        {
            
            StreetPathDataProgram.knowledgeRoot = knowledgeRoot;
            StreetPathDataProgram.databaseRoot = databaseRoot;
            StreetPathDataProgram.reportProgress = reportProgress;

            using (var tb = new TransitBaseComponent(
                root: new CSDirectory(databaseRoot),
                latitudeDegreeDistance: latitudeDegreeDistance,
                longitudeDegreeDistance: longitudeDegreeDistance
                ))
            using (var errorStream = File.Open("error-log.txt", FileMode.Append))
            {
                pathFinder = new PathFinder(tb, reportProgress);
                string lastFile = Directory.EnumerateFiles(knowledgeRoot).MaxBy(f => int.Parse(Path.GetFileNameWithoutExtension(f)));
                string newFile = Path.GetDirectoryName(lastFile) + "\\" + (int.Parse(Path.GetFileNameWithoutExtension(lastFile)) + 1) + ".xml";
                pathFinder.LoadData(lastFile);
                pathFinder.FindRemainingPaths();
                new Thread(progressThreadProgram).Start();
                await pathFinder.CalculateTransfers(errorStream, new HereApi());
                //await pathFinder.CalculateTransfers(errorStream, new GoogleApi());
                pathFinder.Save(newFile);
                taskDone = true;
            }
        }

        static void progressThreadProgram()
        {
            while (!taskDone)
            {
                Thread.Sleep(10000);
                if (pathFinder.totalRequests > 0)
                    reportProgress(String.Format("{0}/{1} requests: {2}%", pathFinder.completeRequests, pathFinder.totalRequests, pathFinder.completeRequests * 100 / pathFinder.totalRequests));
            }
        }
    }
}
