using CityTransitApp.Common;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TransitBase;
using TransitBase.Entities;

namespace CityTransitApp
{
    public static class Extensions
    {
        public static char First(this string text) { return text[0]; }
        public static char Last(this string text) { return text[text.Length - 1]; }

        //public static GeoCoordinate ToGeocoordinate(this string text)
        //{
        //    string[] fields = text.Split('/');
        //    if (fields.Length == 2)
        //        return new GeoCoordinate(Double.Parse(fields[0]), Double.Parse(fields[1]));
        //    else throw new ArgumentException();
        //}
        //public static string ToUriString(this GeoCoordinate coord)
        //{
        //    return coord.Latitude + "/" + coord.Longitude;
        //}
        //public static string ToUriString(this Geocoordinate coord)
        //{
        //    return coord.Latitude + "/" + coord.Longitude;
        //}

        public static string ToRelativeDateString(this DateTime dateTime, CultureInfo cultureInfo = null)
        {
            DateTime today = DateTime.Today;
            cultureInfo = cultureInfo ?? CultureInfo.CurrentUICulture;
            if (dateTime.Date == today)
                return CommonComponent.Current.Services.Resources.LocalizedStringOf("DateTimeToday");
            else if (dateTime.Date == today + TimeSpan.FromDays(1))
                return CommonComponent.Current.Services.Resources.LocalizedStringOf("DateTimeTomorrow");
            else if (dateTime > today && dateTime.DayOfWeek > today.DayOfWeek && (dateTime - today) < TimeSpan.FromDays(7))
                return dateTime.ToString("dddd");
            else
                return dateTime.ToString("d", cultureInfo);
        }

        public static string ToRelativeString(this DateTime dateTime, CultureInfo cultureInfo = null)
        {
            cultureInfo = cultureInfo ?? CultureInfo.CurrentUICulture;
            if (dateTime.Date == DateTime.Today)
                return dateTime.ToString("t", cultureInfo);
            else if (dateTime.Date == DateTime.Today + TimeSpan.FromDays(1))
                return CommonComponent.Current.Services.Resources.LocalizedStringOf("DateTimeTomorrow") + " " + dateTime.ToString("t", cultureInfo);
            else
                return dateTime.ToString("g", cultureInfo);
        }

        public static string ShortTimeString(this DateTime dateTime, CultureInfo cultureInfo = null)
        {
            cultureInfo = cultureInfo ?? CultureInfo.CurrentUICulture;
            return Regex.Replace(dateTime.ToString("t", cultureInfo), "[^0-9:]", str => "");
        }
        public static bool IsPM(this DateTime dateTime, CultureInfo cultureInfo = null)
        {
            cultureInfo = cultureInfo ?? CultureInfo.CurrentUICulture;
            return dateTime.ToString("t", cultureInfo).Contains("PM");
        }

        public static TimeSpan GetTimeOfDay(this TimeSpan time)
        {
            return new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        }

        public static double StraightLineDistanceTo(this Stop stop, GeoCoordinate coordinate)
        {
            return stop.StraightLineDistanceTo(coordinate.Latitude, coordinate.Longitude);
        }

        public static double GetCustomTypePriority(this RouteGroup routeGroup)
        {
            if (CommonComponent.Current.Config.CustomTypePriority != null)
                return CommonComponent.Current.Config.CustomTypePriority(routeGroup);
            return routeGroup.TypePriority;
        }

        public static RouteGroupNames GetNames(this RouteGroup r) { return new RouteGroupNames(r); }
    }
}
