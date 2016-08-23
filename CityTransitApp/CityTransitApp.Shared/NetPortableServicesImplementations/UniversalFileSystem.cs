using System;
using System.Collections.Generic;
using System.Text;
using NetPortableServices;
using Windows.Storage;
using System.Linq;
using System.IO;

namespace CityTransitApp.NetPortableServicesImplementations
{
    class UniversalFileSystem : IFileSystemService
    {
        public IDirectoryService GetAppStorageRoot()
        {
            return new UniversalDirectory(ApplicationData.Current.LocalFolder);
        }

        public IDirectoryService GetSystemRoot()
        {
            throw new NotImplementedException();
        }

        public IDirectoryService GetVolumeRoot(string volumeId)
        {
            throw new NotImplementedException();
        }

        public IDirectoryService GetDirectoryAtPath(string path)
        {
            throw new NotImplementedException();
        }

        public IFileService GetFileAtPath(string path)
        {
            throw new NotImplementedException();
        }

        public IFileService GetFileFromApplicationUri(Uri uri)
        {
            StorageFile storageFile = Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().Result;
            return new UniversalFile(storageFile);
        }
    }

    class UniversalDirectory : IDirectoryService
    {
        internal StorageFolder Directory { get; private set; }
        internal UniversalDirectory(StorageFolder dir) { this.Directory = dir; }

        public string Name { get { return Directory.Name; } }

        public string Path { get { return Directory.Path; } }

        public IEnumerable<IDirectoryService> GetDirectories()
        {
            return Directory.GetFoldersAsync().GetResults().Select(dir => new UniversalDirectory(dir));
        }

        public IEnumerable<IFileService> GetFiles()
        {
            return Directory.GetFilesAsync().GetResults().Select(f => new UniversalFile(f));
        }

        public IFileService GetFile(string name)
        {
            try
            {
                var file = Directory.GetFileAsync(name).GetResults();
                if (file != null)
                    return new UniversalFile(file);
            }
            catch (Exception) { }
            return null;
        }

        public bool CreateFile(string name)
        {
            throw new NotImplementedException();
        }

        public bool CreateDirectory(string name)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            try
            {
                Directory.GetFileAsync(path).AsTask().Wait();
                return true;
            }
            catch { return false; }
        }

        public bool DirectoryExists(string name)
        {
            try
            {
                Directory.GetFolderAsync(name).AsTask().Wait();
                return true;
            }
            catch { return false; }
        }

        public Stream OpenFile(string path)
        {
            var task = Directory.OpenStreamForReadAsync(path);
            //task.Wait();
            return task.Result;
        }
    }

    class UniversalFile : IFileService
    {
        StorageFile file;
        internal UniversalFile(StorageFile file) { this.file = file; }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string NameWithoutExtension
        {
            get { throw new NotImplementedException(); }
        }

        public string Extension
        {
            get { throw new NotImplementedException(); }
        }

        public string Path
        {
            get { throw new NotImplementedException(); }
        }

        public System.IO.Stream OpenForRead()
        {
            return file.OpenStreamForReadAsync().Result;
        }

        public void Delete()
        {
            try
            {
                file.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask().Wait();
            }
            catch { }
        }
    }
}
