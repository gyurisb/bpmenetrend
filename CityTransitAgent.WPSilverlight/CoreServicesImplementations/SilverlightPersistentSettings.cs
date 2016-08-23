using NetPortableServices;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    public class SilverlightPersistentSettings : ISettingsService
    {
        public object Get(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings[key];
        }

        public void Set(string key, object value)
        {
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public object this[string key]
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings[key];
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[key] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        public bool ContainsKey(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
        }

        public void Remove(string key)
        {
            IsolatedStorageSettings.ApplicationSettings.Remove(key);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public void SetDefault(string key, object value)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings[key] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

        }

        public void SaveChanges()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
