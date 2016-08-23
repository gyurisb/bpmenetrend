using FastDatabaseSaver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace TransitBaseTransformation
{
    class PostDbRepair
    {
        public static void DoRepair(string cityname, StaticDBCreater db, Action<int, string> log)
        {
            switch (cityname)
            {
                case "dc": doWashingtonRepair(db, log); break;
                case "la": doLosAngelesRepair(db, log); break;
            }
        }

        private static void doWashingtonRepair(StaticDBCreater db, Action<int, string> log)
        {
            var rexRoute = db.GetTable<RouteGroup>().FirstOrDefault(r => r.Name == "Richmond Highway Express Bus");
            if (rexRoute != null)
            {
                rexRoute.Name = "REX";
                rexRoute.Description = "Richmond Highway Express Bus";
            }
        }

        private static void doLosAngelesRepair(StaticDBCreater db, Action<int, string> log)
        {
            foreach (var route in db.GetTable<RouteGroup>())
            {
                if (isInRange(route.Name, 1, 399) || isInRange(route.Name, 600, 699))
                {
                    if (route.BgColor == 0xffffff) route.BgColor = 0xe67d43;
                }
                else if (isInRange(route.Name, 400, 599))
                {
                    if (route.BgColor == 0xffffff)
                    {
                        route.BgColor = 0x103178;
                        route.FontColor = 0xffffff;
                    }
                }
                else if (isInRange(route.Name, 700, 799))
                {
                    if (route.BgColor == 0x000000)
                    {
                        route.BgColor = 0xdf2a4d;
                        route.FontColor = 0x000000;
                    }
                }
            }
            foreach (var route in db.GetTable<RouteGroup>())
            {
                //var words = route.Name.Split(' ').Where(str => str != "").ToArray();
                var words = route.Name.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 4 && words[0] == "Metro" && words[2] == "Line")
                    route.Name = words[3].Substring(1, words[3].Length - 2);

                if (route.Name.EndsWith("Dodger Stadium Express"))
                {
                    route.Description = route.Name;
                    route.Name = new string(route.Name.Where(ch => char.IsUpper(ch)).ToArray());
                }
            }
            foreach (var trip in db.GetTable<TripType>())
            {
                var splittedName = trip.Name.Split(new string[] { " - " }, StringSplitOptions.None);
                trip.Name = String.Join(" - ", splittedName.Where(name => !isLineNameLabel(name, trip.Route.RouteGroup.Name)));
            }
            foreach (var route in db.GetTable<Route>())
            {
                var splittedName = route.Name.Split(new string[] { " - " }, StringSplitOptions.None);
                route.Name = String.Join(" - ", splittedName.Where(name => !isLineNameLabel(name, route.RouteGroup.Name)));
            }

            foreach (var entry in db.GetTable<TripType>().SelectMany(tt => tt.HeadsignEntries))
            {
                var splittedName = entry.Headsign.Split(new string[] { " - " }, StringSplitOptions.None);
                entry.Headsign = String.Join(" - ", splittedName.Where(name => !isLineNameLabel(name, entry.TripType.Route.RouteGroup.Name)));
            }
            var changingRoutes = db.GetTable<TripType>().Where(tt => tt.HeadsignEntries.Select(e => e.Headsign).Distinct().Count() > 1).ToList();
            int changingCount = changingRoutes.Count();
            int totalCount = db.GetTable<TripType>().Count();
            int changingTripCount = changingRoutes.Sum(r => r.Trips.Count());
            int totalTripCount = db.GetTable<Trip>().Count();
            log(0, String.Format("LA changing triptype count: {0}/{1} ({2:P}%)", changingCount, totalCount, changingCount / (double)totalCount));
            log(0, String.Format("LA changing trip count: {0}/{1} ({2:P}%)", changingTripCount, totalTripCount, changingTripCount / (double)totalTripCount));
        }
        private static bool isLineNameLabel(string text, string lineName)
        {
            return text == lineName || text.StartsWith("Change to Route") || lineName.Contains(text) || text == "Silver Line" || text == "Orange Line";
        }
        private static bool isInRange(string text, int start, int end)
        {
            return text.All(ch => char.IsDigit(ch) || ch == '/') && text.Split('/').Any(nr => start <= int.Parse(nr) && int.Parse(nr) <= end);
        }
    }
}
