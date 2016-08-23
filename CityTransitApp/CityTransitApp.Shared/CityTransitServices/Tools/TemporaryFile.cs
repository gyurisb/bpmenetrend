using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CityTransitServices.Tools
{
    public class TemporaryFile : IDisposable
    {
        private Task<StorageFile> tempFile;
        private string fileName;

        public TemporaryFile(string fileName, bool isShellContent = false)
        {
            if (isShellContent)
                fileName = Path.Combine("Shared", "ShellContent", fileName);
            this.fileName = fileName;
            this.tempFile = ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting).AsTask();
        }
        //private TemporaryFile(StorageFile file, string fileName)
        //{
        //    this.tempFile = Task.Run(() => file);
        //    this.fileName = fileName;
        //}

        //public async Task<TemporaryFile> CreateAsync(string fileName)
        //{
        //    var tempFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
        //    return new TemporaryFile(tempFile, fileName);
        //}
        //public async Task<TemporaryFile> CreateShellContentAsync(string fileName)
        //{
        //    return await CreateAsync(Path.Combine("Shared", "ShellContent", fileName));
        //}

        public async Task<IRandomAccessStream> OpenRandomAccessStreamForWriteAsync()
        {
            return await (await tempFile).OpenAsync(FileAccessMode.ReadWrite);
        }

        public Uri Uri
        {
            get { return new Uri("ms-appdata:///local/" + fileName); }
        }

        public void Dispose()
        {
            tempFile.Wait();
            var file = tempFile.Result;
            file.DeleteAsync().AsTask().Wait();
        }

        public async Task<StorageFile> FileAsync()
        {
            return await tempFile;
        }

        public string Name { get { return fileName; } }
    }
}
