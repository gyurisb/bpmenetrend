using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace UserBase.Interface
{
    public interface IHistory
    {
        event EventHandler HistoryCleared;
        void AddTimetableHistory(Route route, StopGroup stop, int weight = 1);
        void AddStopHistory(StopGroup stop);
        void AddPlanningHistory(StopGroup stop, bool isSource);
        void SetRecentStopAtRoute(Route route, StopGroup stop);
        StopGroup GetRecentStop(Route route);
        IRecentEntry GetRecentRoute(RouteGroup routeGroup);
        IEnumerable<IRecentEntry> GetRecents();
        void Clear();
        void OldenHistory();
        IEnumerable<ITimetableHistoryEntry> TimetableEntries { get; }
        IEnumerable<IStopHistoryEntry> StopEntries { get; }
        IEnumerable<IPlanningHistoryEntry> SourceTargetEntries { get; }
    }

    public static class HistoryHelpers
    {
        public static int DayPartDistance(ICountableHistoryEntry entry)
        {
            int ret = Math.Abs(DateTime.Now.Hour / 3 - entry.DayPart);
            if (ret > 4) return 8 - ret;
            else return ret;
        }
    }
}
