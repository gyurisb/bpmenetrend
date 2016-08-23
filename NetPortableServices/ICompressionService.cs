using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPortableServices
{
    public interface ICompressionService
    {
        Task UnzipAsync(Stream compressedDataStream, IDirectoryService targetDirectory);
        Task UnzipAsync(IFileService compressedFile, IDirectoryService targetDirectory);
    }
}
