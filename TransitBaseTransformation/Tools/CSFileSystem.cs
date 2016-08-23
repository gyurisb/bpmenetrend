using NetPortableServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBaseTransformation.Tools
{
    public class CSDirectory : IDirectoryService
    {
        string directoryPath;
        public CSDirectory(string directoryPath) { this.directoryPath = directoryPath; }

        public string Name { get { throw new NotImplementedException(); } }

        public string Path { get { return directoryPath; } }

        public IEnumerable<IDirectoryService> GetDirectories()
        {
            return Directory.GetDirectories(directoryPath).Select(filePath => new CSDirectory(filePath));
        }

        public IEnumerable<IFileService> GetFiles()
        {
            return Directory.GetFiles(directoryPath).Select(filePath => new CSFile(filePath));
        }

        public IFileService GetFile(string name)
        {
            string filePath = System.IO.Path.Combine(directoryPath, name);
            if (File.Exists(filePath))
                return new CSFile(filePath);
            else return null;
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
            string filePath = System.IO.Path.Combine(directoryPath, path);
            return File.Exists(filePath);
        }

        public bool DirectoryExists(string path)
        {
            string filePath = System.IO.Path.Combine(directoryPath, path);
            return Directory.Exists(filePath);
        }

        public System.IO.Stream OpenFile(string path)
        {
            return File.OpenRead(System.IO.Path.Combine(directoryPath, path));
        }
    }

    public class CSFile : IFileService
    {
        private string filePath;
        public CSFile(string filePath)
        {
            this.filePath = filePath;
        }

        public string Name { get { return System.IO.Path.GetFileName(filePath); } }

        public string NameWithoutExtension { get { return System.IO.Path.GetFileNameWithoutExtension(filePath); } }

        public string Extension { get { return System.IO.Path.GetExtension(filePath); } }

        public string Path { get { return filePath; } }

        public System.IO.Stream OpenForRead()
        {
            return File.OpenRead(filePath);
        }

        public void Delete()
        {
            File.Delete(filePath);
        }
    }
}
