using CityTransitServices.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using TransitBase;
using TransitBase.Entities;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CityTransitApp
{
    public static class Extensions
    {
        public static GeoCoordinate AsCoord(this Geocoordinate coord) { return new GeoCoordinate { Latitude = coord.Latitude, Longitude = coord.Longitude }; }

        public static Geopoint ToGeopoint(this GeoCoordinate coord) { return new Geopoint(new BasicGeoposition { Latitude = coord.Latitude, Longitude = coord.Longitude }); }
        public static BasicGeoposition ToBasicGeoposition(this GeoCoordinate coord) { return new BasicGeoposition { Latitude = coord.Latitude, Longitude = coord.Longitude }; }


        public static void PutMany(this ResourceDictionary dict, ResourceDictionary source)
        {
            foreach (var entry in source.AsEnumerable())
            {
                if (dict.ContainsKey(entry.Key))
                    dict.Remove(entry.Key);
                dict.Add(entry.Key, entry.Value);
            }
        }

        public static RouteGroupColors GetColors(this RouteGroup routeGroup)
        {
            return new RouteGroupColors(routeGroup);
            //return (RouteGroupColors)routeGroup.T;
        }

        public static async Task TryDeleteFileAsync(this StorageFolder folder, string fileName)
        {
            try
            {
                var file = await folder.GetFileAsync(fileName);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch { }
        }

        public static async Task<bool> ContainsFileAsync(this StorageFolder folder, string fileName)
        {
            try
            {
                await folder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeepRemove(this IList<DependencyObject> UIElementLayer, UIElement uiElement)
        {
            ContentPresenter contentPresenter = VisualTreeHelper.GetParent(uiElement) as ContentPresenter;
            contentPresenter.Content = null;
            Canvas canvas = VisualTreeHelper.GetParent(contentPresenter) as Canvas;
            canvas.Children.Remove(contentPresenter);
            UIElementLayer.Remove(uiElement);
        }
    }
}
