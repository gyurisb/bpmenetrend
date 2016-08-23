using CityTransitApp.WPSilverlight.Tools;
using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace CityTransitApp
{
    static class Extensions
    {
        public static IList ItemsSource(this LongListSelector list) { return (IList)list.ItemsSource; }
        public static IList<T> ItemsSource<T>(this LongListSelector list) { return (IList<T>)list.ItemsSource; }


        public static RouteGroupColors GetColors(this RouteGroup routeGroup)
        {
            return new RouteGroupColors(routeGroup);
            //return (RouteGroupColors)routeGroup.T;
        }
    }
}
