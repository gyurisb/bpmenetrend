using NetPortableServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class SilverlightFileSystem : IFileSystemService
    {
        public IDirectoryService GetAppStorageRoot()
        {
            return new CSDirectory(ApplicationData.Current.LocalFolder.Path);
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
            return new StreamFile(App.GetResourceStream(uri).Stream);
        }
    }

    public class StreamFile : IFileService
    {
        Stream stream;
        public StreamFile(Stream stream) { this.stream = stream; }

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

        public Stream OpenForRead()
        {
            return stream;
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
