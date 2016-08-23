using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using TransitBase;
using UserBase.Interface;

namespace UserBase.LinqToSQL
{
    public class History : IHistory
    {
        private DyanamicDBContext ctx;
        public History(DyanamicDBContext ctx) { this.ctx = ctx; }

        public event EventHandler HistoryCleared;

        public static readonly int FloatingBits = 8;

        public void AddTimetableHistory(Route route, StopGroup stop, int weight = 1)
        {
            lock (ctx)
            {
                DateTime now = DateTime.Now;
                var q = from e in ctx.HistoryEntries
                        where e.RouteID == route.ID && e.StopID == stop.ID
                        select e;
                HistoryEntry entry = q.ToList().SingleOrDefault(x => x.IsActive(now));
                if (entry != null)
                {
                    entry.RawCount = entry.RawCount + (weight << FloatingBits);
                }
                else
                {
                    entry = new HistoryEntry(now) { Route = route, Stop = stop, RawCount = weight << FloatingBits };
                    ctx.HistoryEntries.InsertOnSubmit(entry);
                }
                ctx.SubmitChanges();
            }
        }

        public void AddStopHistory(StopGroup stop)
        {
            lock (ctx)
            {
                DateTime now = DateTime.Now;
                var q = from e in ctx.StopHistoryEntries
                        where e.StopID == stop.ID
                        select e;
                StopHistoryEntry entry = q.ToList().SingleOrDefault(e => e.IsActive(now));
                if (entry != null)
                {
                    entry.RawCount = entry.RawCount + (1 << FloatingBits);
                }
                else
                {
                    entry = new StopHistoryEntry(now) { Stop = stop, RawCount = 1 << FloatingBits };
                    ctx.StopHistoryEntries.InsertOnSubmit(entry);
                }
                ctx.SubmitChanges();
            }
        }

        public void AddPlanningHistory(StopGroup stop, bool isSource)
        {
            lock (ctx)
            {
                DateTime now = DateTime.Now;
                var q = from e in ctx.PlanningHistoryEntries
                        where e.StopID == stop.ID && e.IsSource == isSource
                        select e;
                PlanningHistoryEntry entry = q.ToList().SingleOrDefault(e => e.IsActive(now));
                if (entry != null)
                {
                    entry.RawCount = entry.RawCount + (1 << FloatingBits);
                }
                else
                {
                    entry = new PlanningHistoryEntry(now) { Stop = stop, RawCount = 1 << FloatingBits, IsSource = isSource };
                    ctx.PlanningHistoryEntries.InsertOnSubmit(entry);
                }
                ctx.SubmitChanges();
            }
        }

        public void SetRecentStopAtRoute(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                var recentEntry = ctx.RecentEntries.SingleOrDefault(e => e.RouteID == route.ID);
                if (recentEntry != null)
                    ctx.RecentEntries.DeleteOnSubmit(recentEntry);

                var entry = new RecentEntry { Route = route, Stop = stop };
                ctx.RecentEntries.InsertOnSubmit(entry);

                ctx.SubmitChanges();
            }
        }
        public StopGroup GetRecentStop(Route route)
        {
            lock (ctx)
            {
                var recentEntry = ctx.RecentEntries.SingleOrDefault(e => e.RouteID == route.ID);
                if (recentEntry == null || DateTime.Now - recentEntry.CreationTime > TimeSpan.FromHours(12)) return null;
                return recentEntry.Stop;
            }
        }
        public IRecentEntry GetRecentRoute(RouteGroup routeGroup)
        {
            lock (ctx)
            {
                DateTime now = DateTime.Now; TimeSpan twelveHours = TimeSpan.FromHours(12);
                return ctx.RecentEntries.ToList()
                    .Where(e => e.Route.RouteGroup == routeGroup && now - e.CreationTime < twelveHours)
                    .MaxBy(e => e.CreationTime);
            }
        }
        public IEnumerable<IRecentEntry> GetRecents()
        {
            lock (ctx)
            {
                DateTime now = DateTime.Now; TimeSpan twelveHours = TimeSpan.FromHours(12);
                return ctx.RecentEntries
                    .Where(e => now - e.CreationTime < twelveHours)
                    .OrderByDescending(e => e.CreationTime)
                    .ToList();
            }
        }

        public void Clear()
        {
            lock (ctx)
            {
                ctx.HistoryEntries.DeleteAllOnSubmit(ctx.HistoryEntries);
                ctx.StopHistoryEntries.DeleteAllOnSubmit(ctx.StopHistoryEntries);
                ctx.PlanningHistoryEntries.DeleteAllOnSubmit(ctx.PlanningHistoryEntries);
                ctx.RecentEntries.DeleteAllOnSubmit(ctx.RecentEntries);
                ctx.SubmitChanges();
            }
            if (HistoryCleared != null)
                HistoryCleared(this, new EventArgs());
        }

        public void OldenHistory()
        {
            lock (ctx)
            {
                foreach (var entry in ctx.HistoryEntries)
                {
                    entry.RawCount = 9 * entry.RawCount / 10;
                }
                foreach (var entry in ctx.StopHistoryEntries)
                {
                    entry.RawCount = 9 * entry.RawCount / 10;
                }
                foreach (var entry in ctx.PlanningHistoryEntries)
                {
                    entry.RawCount = 9 * entry.RawCount / 10;
                }
                DateTime now = DateTime.Now; TimeSpan twelveHours = TimeSpan.FromHours(12);
                var removable = ctx.RecentEntries.Where(e => now - e.CreationTime > twelveHours);
                ctx.RecentEntries.DeleteAllOnSubmit(removable);

                ctx.SubmitChanges();
            }
        }

        public IEnumerable<ITimetableHistoryEntry> TimetableEntries
        {
            get
            {
                lock (ctx)
                {
                    return ctx.HistoryEntries.ToList();
                }
            }
        }
        public IEnumerable<IStopHistoryEntry> StopEntries
        {
            get
            {
                lock (ctx)
                {
                    return ctx.StopHistoryEntries.ToList();
                }
            }
        }
        public IEnumerable<IPlanningHistoryEntry> SourceTargetEntries
        {
            get
            {
                lock (ctx)
                {
                    return ctx.PlanningHistoryEntries.ToList();
                }
            }
        }

        public static int DayPartDistance(CountableHistoryEntry entry)
        {
            int ret = Math.Abs(DateTime.Now.Hour / 3 - entry.DayPart);
            if (ret > 4) return 8 - ret;
            else return ret;
        }
    }
}
