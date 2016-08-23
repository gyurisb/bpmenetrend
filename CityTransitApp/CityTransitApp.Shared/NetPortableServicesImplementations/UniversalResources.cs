using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CityTransitApp.NetPortableServicesImplementations
{
    class UniversalResources : IResourcesService
    {
        public string LocalizedStringOf(string key)
        {
            try
            {
                ResourceCandidate resource = ResourceManager.Current.MainResourceMap.GetValue("Resources/" + key + "/text", ResourceContext.GetForCurrentView());
                return resource.ValueAsString;
            }
            catch (Exception) { return "[" + key + "]"; }
        }


        public object ColorOf(string key)
        {
            return (Brush)App.Current.Resources[key];
        }

        public object IconOf(string key)
        {
            switch (key)
            {
                case "Favorite": return new SymbolIcon(Symbol.Favorite);
                case "UnFavorite": return new SymbolIcon(Symbol.UnFavorite);
                default: throw new KeyNotFoundException("No icon found with the given key.");
            }
        }

        public object ValueOf(string key)
        {
            switch (key)
            {
                case "HistoryRowCount":
#if WINDOWS_PHONE_APP
                    return (int)Math.Floor(App.GetAppInfo().GetScreenWidth() / 85.0);
#else
                    return (int)Math.Floor((App.GetAppInfo().GetScreenHeight() - 165) / 95.0);
#endif
                default: throw new KeyNotFoundException("No resource found with the given key.");
            }
        }

    }
}
