using CityTransitApp.WPSilverlight.Resources;
using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Media;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class SilverlightResources : IResourcesService
    {
        public string LocalizedStringOf(string key)
        {
            return AppResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
        }

        public object ColorOf(string key)
        {
            return (Brush)App.Current.Resources[key];
        }

        public object IconOf(string key)
        {
            switch (key)
            {
                case "Favorite": return new Uri("Assets/AppBar/favs.addto.png", UriKind.Relative);
                case "UnFavorite": return new Uri("Assets/AppBar/favs.removefrom.png", UriKind.Relative);
                default: throw new KeyNotFoundException("No icon found with the given key.");
            }
        }

        public object ValueOf(string key)
        {
            switch (key)
            {
                case "HistoryRowCount":
                    return (int)Math.Floor(App.RootFrame.ActualWidth / 85.0);
                default: throw new KeyNotFoundException("No resource found with the given key.");
            }
        }
    }
}
