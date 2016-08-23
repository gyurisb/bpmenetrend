using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace CityTransitServices.Tools
{
    public class StringFactory
    {
        public static String XMinutes(int nr)
        {
            if (nr < 0)
                throw new ArgumentException();
            if (nr > 1)
                return nr + " " + CommonComponent.Current.Services.Resources.LocalizedStringOf("StringMinutePl");
            else
                return nr + " " + CommonComponent.Current.Services.Resources.LocalizedStringOf("StringMinute");
        }

        public static string Format(string input, bool plural, params object[] args)
        {
            string inputPl = Regex.Replace(input, @"\/.*\|.*\/", match => match.Value.Replace("/", "").Split('|')[plural ? 1 : 0]);
            int i = 0;
            return Regex.Replace(inputPl, @"\%", match => args[i++].ToString());
        }
        public static string Format(string input, int arg)
        {
            return Format(input, arg > 1, arg);
        }

        public static string CardinalToString(Cardinal c)
        {
            switch (c)
            {
                case Cardinal.East: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalEast");
                case Cardinal.North: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalNorth");
                case Cardinal.NorthEast: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalNorthEast");
                case Cardinal.NorthWest: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalNorthWest");
                case Cardinal.South: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalSouth");
                case Cardinal.SouthEast: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalSouthEast");
                case Cardinal.SouthWest: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalSouthWest");
                case Cardinal.West: return CommonComponent.Current.Services.Resources.LocalizedStringOf("CardinalWest");
                default: throw new InvalidOperationException();
            }
        }

        public static int LocalizeDistance(int distanceInMeters)
        {
            if (!System.Globalization.RegionInfo.CurrentRegion.IsMetric)
                return Round(distanceInMeters * 3.28084);
            return Round(distanceInMeters);
        }
        public static int LocalizeDistance(double distanceInMeters)
        {
            if (!System.Globalization.RegionInfo.CurrentRegion.IsMetric)
                return Round(distanceInMeters * 3.28084);
            return Round(distanceInMeters);
        }

        public static string LocalizeDistanceWithUnit(double distanceInMeters)
        {
            if (!System.Globalization.RegionInfo.CurrentRegion.IsMetric)
                return Round(distanceInMeters * 3.28084) + " ft";
            return Round(distanceInMeters) + " m";
        }

        private static int Round(double number)
        {
            int rounded = (int)Math.Round(number);
            if (rounded > 1000)
                rounded = rounded / 100 * 100;
            else if (rounded > 500)
                rounded = rounded / 50 * 50;
            else if (rounded > 20)
                rounded = rounded / 10 * 10;
            return rounded;
        }
    }
}
