using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPortableServices
{
    public interface IFileSystemService
    {
        IDirectoryService GetAppStorageRoot();
        IDirectoryService GetSystemRoot();
        IDirectoryService GetVolumeRoot(string volumeId);
        IDirectoryService GetDirectoryAtPath(string path);
        IFileService GetFileAtPath(string path);
        IFileService GetFileFromApplicationUri(Uri uri);
    }

    public interface IDirectoryService
    {
        string Name { get; }
        string Path { get; }
        IEnumerable<IDirectoryService> GetDirectories();
        IEnumerable<IFileService> GetFiles();
        IFileService GetFile(string name);
        bool CreateFile(string name);
        bool CreateDirectory(string name);
        bool FileExists(string path);
        bool DirectoryExists(string path);
        Stream OpenFile(string path);
    }

    public interface IFileService
    {
        string Name { get; }
        string NameWithoutExtension { get; }
        string Extension { get; }
        string Path { get; }
        Stream OpenForRead();
        void Delete();
    }
}
