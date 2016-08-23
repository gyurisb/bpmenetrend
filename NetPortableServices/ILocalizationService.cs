using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPortableServices
{
    public interface IResourcesService
    {
        string LocalizedStringOf(string key);
        object ColorOf(string key);
        object IconOf(string key);
        object ValueOf(string key);
    }
}
