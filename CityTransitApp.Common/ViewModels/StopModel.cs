using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using TransitBase;

namespace CityTransitApp.Common.ViewModels
{
    public class StopModel
    {
        public StopGroup Stop { get; private set; }
        public bool Fill { get; private set; }
        private RouteGroup[] groups;

        public string Name { get { return Stop.Name; } }
        public int RouteCount { get { return groups.Count(); } }
        public string Routes { get { return String.Join(", ", groups.Select(r => r.Name).Distinct()); } }
        public double HighestPriority { get; private set; }

        private uint[] colors = null;
        public uint[] Colors
        {
            get
            {
                if (colors == null)
                    calculateColors(this.Fill);
                return colors;
            }
        }

        public object T
        {
            get
            {
                return new { IsRoute = false, IsStop = true };
            }
        }
        //public Visibility IsRoute { get { return Visibility.Collapsed; } }
        //public Visibility IsStop { get { return Visibility.Visible; } }

        public StopModel(StopGroup stop, bool saveRouteString = false, bool fill = false)
        {
            this.Stop = stop;
            this.Fill = fill;
            //var routes = stop.RouteGroups.Where(g => g.Routes.Any(r => r.TravelRoute.Any(e => e.Stop == stop)));
            //this.groups = routes.OrderByText(g => g.Name).OrderByWithCache(g => g.GetCustomTypePriority()).ToArray();
            this.groups = Stop.RouteGroups.OrderByText(g => g.Name).OrderByWithCache(g => g.GetCustomTypePriority()).ToArray();
            this.HighestPriority = groups.Length > 0 ? groups.Min(g => g.GetCustomTypePriority()) : double.MaxValue;
        }

        private void calculateColors(bool fill)
        {
            var colors = groups.Select(g => g.BgColor).Distinct().Take(3).ToList();
            while (fill && colors.Count < 3) colors.Add(colors.LastOrDefault());
            this.colors = colors.ToArray();
        }
    }
}
