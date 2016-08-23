using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.BusinessLogic;
using TransitBase.Entities;

namespace CityTransitServices.Tools
{
    public class RouteGroupNames
    {
        protected RouteGroup outer;
        public RouteGroupNames(RouteGroup outer)
        {
            this.outer = outer;
        }
        //public string NameLabel
        //{
        //    get
        //    {
        //        if (this.Name.EndsWith("-SBS"))
        //            return this.Name.Replace('-', '\n');
        //        return this.Name;
        //    }
        //}
        //public int NameLength
        //{
        //    get
        //    {
        //        if (this.Name.EndsWith("-SBS"))
        //            return this.Name.Length - "-SBS".Length;
        //        return this.Name.Length;
        //    }
        //}

        public string ShortName
        {
            get
            {
                return IsLong(outer.Name) ? "" : outer.Name;
            }
        }

        public string LongName
        {
            get
            {
                return IsLong(outer.Name) ? outer.Name + " " : "";
            }
        }
        public bool IsLongNameVisible { get { return IsLong(outer.Name); } }
        public bool IsOutOfServiceVisible { get { return outer.Routes.Count == 0; } }
        public bool HasAnyRoute { get { return outer.Routes.Count != 0; } }

        #region Main Name
        public string ShortMainName
        {
            get
            {
                string name = Config.Current.NameTextPred(outer);
                return IsLong(name) ? "" : name;
            }
        }
        public string Label
        {
            get
            {
                return Config.Current.LabelTextPred(outer);
            }
        }
        public string LongMainName
        {
            get
            {
                string name = Config.Current.NameTextPred(outer);
                return IsLong(name) ? outer.Name + " " : "";
            }
        }
        public string MainName
        {
            get
            {
                return Config.Current.NameTextPred(outer);
            }
        }
        public bool IsLabelVisible { get { return Config.Current.HasLabelPred(outer); } }
        public bool IsVeryLongNameVisible { get { return IsLong(Config.Current.NameTextPred(outer)); } }
        #endregion

        public static bool IsLong(string name)
        {
            if (Config.Current.LongNameExceptions != null)
                return name.Length > 4 || Config.Current.LongNameExceptions.Contains(name);
            return name.Length > 4;
        }

        public string Icon
        {
            get
            {
                int routeType = outer.Type;
                if (routeType == RouteType.Bus)
                    return "" + (char)0xf207;
                else if (routeType == RouteType.Metro)
                    return "" + (char)0xf239;
                else if (routeType == RouteType.RailRoad)
                    return "" + (char)0xf239;
                if (routeType == RouteType.Tram)
                    return "" + (char)0xf238;
                else if (routeType == RouteType.CableCar)
                    return "" + (char)0xf207;
                else if (routeType == RouteType.Funicular)
                    return "";
                else if (routeType == RouteType.Gondola)
                    return "";
                else if (routeType == RouteType.Ferry)
                    return "" + (char)0xf21a;
                return null;
            }
        }
        public bool IsSubway
        {
            get
            {
                return outer.Type == RouteType.Metro;
            }
        }
    }
}
