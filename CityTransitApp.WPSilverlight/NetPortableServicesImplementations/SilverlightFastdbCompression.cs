using NetPortableServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using Windows.Storage;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class SilverlightFastdbCompression : ICompressionService
    {
        public async Task UnzipAsync(Stream compressedDataStream, IDirectoryService targetDirectory)
        {
            string targetDirPath = targetDirectory.Path;
            StreamResourceInfo info = new StreamResourceInfo(compressedDataStream, "");
            await Task.Run(() =>
            {
                decompressFile(targetDirPath, info, "meta.txt");
                decompressFile(targetDirPath, info, "strings.txt");
                foreach (string file in getFiles(Path.Combine(targetDirPath, "meta.txt")))
                    decompressFile(targetDirPath, info, file);
            });
        }

        public async Task UnzipAsync(IFileService compressedFile, IDirectoryService targetDirectory)
        {
            using (Stream stream = compressedFile.OpenForRead())
            {
                await UnzipAsync(stream, targetDirectory);
            }
        }

        private IEnumerable<string> getFiles(string metaFile)
        {
            using (StreamReader reader = new StreamReader(metaFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    yield return line.Split(';')[1] + ".dat";
            }
        }

        private void decompressFile(string targetDir, StreamResourceInfo info, string fileName)
        {
            StreamResourceInfo file = App.GetResourceStream(info, new Uri(fileName, UriKind.Relative));

            String targetFilePath = Path.Combine(targetDir, fileName);

            using (FileStream stream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
            using (Stream zipStream = file.Stream)
            {
                zipStream.CopyTo(stream);
            }
        }
    }
}
