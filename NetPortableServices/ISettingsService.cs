using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPortableServices
{
    public interface ISettingsService
    {
        object Get(string key);
        void Set(string key, object value);
        object this[string key] { get; set; }
        bool ContainsKey(string key);
        void Remove(string key);
        void SetDefault(string key, object value);
        void SaveChanges();

    }
}
