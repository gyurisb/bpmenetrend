using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.Common.Processes
{
    //class DatabaseInstaller : Process<DatabaseInstaller, string, bool>
    //{
    //    protected override bool Start(params object[] parameters)
    //    {
    //        Stream resultStream = (Stream)parameters[0];
    //        decompressDatabase(resultStream);
    //        completeDatabase();
    //        return true;
    //    }

    //    private void decompressDatabase(Stream resultStream)
    //    {
    //        PerformProgressChanged(Services.Localization.StringOf("DownloadDecompress"));
    //        StreamResourceInfo info = new StreamResourceInfo(resultStream, "");
    //        decompressFile(info, "meta.txt");
    //        decompressFile(info, "strings.txt");
    //        foreach (string file in getFiles())
    //            decompressFile(info, file);
    //    }

    //    private IEnumerable<string> getFiles()
    //    {
    //        string metaFile = Path.Combine(ApplicationData.Current.LocalFolder.Path, "meta.txt");
    //        using (StreamReader reader = new StreamReader(metaFile))
    //        {
    //            string line;
    //            while ((line = reader.ReadLine()) != null)
    //                yield return line.Split(';')[1] + ".dat";
    //        }
    //    }

    //    private void decompressFile(StreamResourceInfo info, string fileName)
    //    {
    //        StreamResourceInfo file = App.GetResourceStream(info, new Uri(fileName, UriKind.Relative));

    //        String targetFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);

    //        using (FileStream stream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
    //        using (Stream zipStream = file.Stream)
    //        {
    //            zipStream.CopyTo(stream);
    //        }
    //    }

    //    private void completeDatabase()
    //    {
    //        PerformProgressChanged(Services.Localization.StringOf("DownloadLoadDB"));

    //        if (App.DDB != null)
    //        {
    //            DbBackup backup = DbBackup.Import(App.DDB);
    //            App.SDBLoad();
    //            backup.ExportToContext();
    //        }
    //        else
    //        {
    //            App.SDBLoad();
    //        }

    //        PerformProgressChanged(Services.Localization.StringOf("DownloadReady"));

    //        AppFields.ForceUpdate = false;
    //    }
    //}
}
