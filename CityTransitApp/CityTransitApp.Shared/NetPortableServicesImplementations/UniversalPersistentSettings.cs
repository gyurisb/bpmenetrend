using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Storage;

namespace CityTransitApp.NetPortableServicesImplementations
{
    class UniversalPersistentSettings : ISettingsService
    {
        public object Get(string key)
        {
            return this[key];
        }

        public void Set(string key, object value)
        {
            this[key] = value;
        }

        public object this[string key]
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values[key];
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }

        public bool ContainsKey(string key)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
        }

        public void Remove(string key)
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
        }

        public void SetDefault(string key, object value)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public void SaveChanges()
        {
        }
    }
}
