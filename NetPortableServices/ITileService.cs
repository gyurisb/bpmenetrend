using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPortableServices
{
    public interface ITileService
    {
        Task<bool> CreateFromControlAsync(string id, object control);
        Task<bool> CreateFromImageAsync(string id, object image);

        Task<bool> UpdateFromControlAsync(string id, object control);
        Task<bool> UpdateFromImageAsync(string id, object control);

        Task<string> GetTilesAsync();
    }
}
