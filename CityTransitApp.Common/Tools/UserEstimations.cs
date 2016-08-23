using CityTransitApp;
using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase.Interface;

namespace CityTransitServices.Tools
{
    public class UserEstimations
    {
        public static readonly double RouteHistoryCountLimit = 6.0;
        public static readonly double StopHistoryCountLimit = 6.0;

        public static async Task<Route> BestRouteAsync(RouteGroup selectedRouteGroup)
        {
            return await Task.Run(() => BestRoute(selectedRouteGroup));
        }
        public static Route BestRoute(RouteGroup selectedRouteGroup)
        {
            var recentEntry = CommonComponent.Current.UB.History.GetRecentRoute(selectedRouteGroup);
            if (recentEntry != null)
            {
                return recentEntry.Route;
            }
            else
            {
                var route = CommonComponent.Current.UB.History.TimetableEntries
                    .Where(h => h.Route.RouteGroup == selectedRouteGroup)
                    .GroupBy(x => x.Route)
                    .Select(x => new { Route = x.Key, Rating1 = x.Min(y => HistoryHelpers.DayPartDistance(y)), Rating2 = x.Sum(y => y.CurrentCount) })
                    .OrderByDescending(x => x.Rating2).OrderBy(x => x.Rating1)
                    .FirstOrDefault();
                if (route != null && route.Rating2 > RouteHistoryCountLimit)
                    return route.Route;
                else
                    return selectedRouteGroup.Routes.First();
            }
        }

        public static async Task<StopGroup> BestStopAsync(Route selectedRoute)
        {
            return await Task.Run(() => BestStop(selectedRoute));
        }
        public static StopGroup BestStop(Route selectedRoute)
        {
            var lastStop = selectedRoute.TravelRoute.Last().Stop;
            var recentStop = CommonComponent.Current.UB.History.GetRecentStop(selectedRoute);
            if (recentStop != null && recentStop != lastStop)
            {
                return recentStop;
            }
            else
            {
                var stop = CommonComponent.Current.UB.History.TimetableEntries
                    //.Where(h => h.RouteID == selectedRoute.ID && h.StopID != lastStop.ID)
                    .Where(h => h.Route == selectedRoute && h.Stop != lastStop)
                    .GroupBy(x => x.Stop)
                    .Select(x => new { Stop = x.Key, Rating1 = x.Min(y => HistoryHelpers.DayPartDistance(y)), Rating2 = x.Sum(y => y.CurrentCount) })
                    .OrderByDescending(x => x.Rating2).OrderBy(x => x.Rating1)
                    .FirstOrDefault();
                if (stop != null && stop.Rating2 > StopHistoryCountLimit)
                    return stop.Stop;
                else
                    return selectedRoute.TravelRoute.First().Stop;
            }
        }

        public static IEnumerable<StopGroup> GetPlanningHistory(bool isSource)
        {
            return CommonComponent.Current.UB.History.SourceTargetEntries.Where(x => x.IsSource == isSource)
                    .GroupBy(x => x.Stop)
                    .Select(x => new { Stop = x.Key, Dist = x.Min(p => HistoryHelpers.DayPartDistance(p)), Count = x.Sum(p => p.RawCount) })
                    .OrderByDescending(x => x.Count)
                    .OrderBy(x => x.Dist)
                    .Select(x => x.Stop)
                    .Take(6);
        }
    }
}
