
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPortableServices
{
    public interface IMessageBoxService
    {
        Task Show(string message);
    }
}
