using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CityTransitServices.Migration
{
    public class TransitBaseMigrator
    {
        public static async Task Run(StorageFolder oldSource, StorageFolder newSource)
        {
            List<string> lines = new List<string>();
            using (var stream = await oldSource.OpenStreamForReadAsync("meta.txt"))
            using (var reader = new StreamReader(stream))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }
            var metaFile = await newSource.CreateFileAsync("meta.txt");
            using (var stream = await metaFile.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream))
            {
                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    string typeName = fields[1].Split(',').First().Split('.').Last();
                    fields[1] = "TransitBase.Entities." + typeName + ", TransitBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                    writer.WriteLine(String.Join(";", fields));
                }
            }
            await (await oldSource.GetFileAsync("strings.txt")).CopyAsync(newSource);
            foreach (var file in (await oldSource.GetFilesAsync()).ToList())
                if (file.FileType == ".dat")
                    await file.CopyAsync(newSource);
            foreach (var file in (await newSource.GetFilesAsync()).ToList())
                if (file.FileType == ".dat")
                {
                    string typeName = file.DisplayName.Split(',').First().Split('.').Last();
                    await file.RenameAsync("TransitBase.Entities." + typeName + ", TransitBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null.dat");
                }
        }

        public static async Task Run()
        {
            var oldFolder = ApplicationData.Current.LocalFolder;
            var newFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("target");
            await Run(oldFolder, newFolder);
            foreach (var file in (await oldFolder.GetFilesAsync()).ToList())
                if (file.FileType == ".dat" || file.FileType == ".txt")
                    await file.DeleteAsync();
            foreach (var file in (await newFolder.GetFilesAsync()).ToList())
                await file.CopyAsync(oldFolder);
            await newFolder.DeleteAsync();
        }
    }
}
