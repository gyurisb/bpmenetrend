using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase.Entities;
using TransitBase;
using UserBase.Interface;

namespace UserBase.BusinessLogic
{
    public class History : IHistory
    {
        private UserBaseSQLiteContext ctx;
        public History(UserBaseSQLiteContext ctx) { this.ctx = ctx; }

        public static readonly int FloatingBits = 8;

        public event EventHandler HistoryCleared;

        public void AddTimetableHistory(Route route, StopGroup stop, int weight = 1)
        {
            lock (ctx)
            {
                DateTime now = DateTime.Now;
                var q = from e in ctx.HistoryEntries
                        where e.RouteID == route.ID && e.StopID == stop.ID
                        select e;
                HistoryEntry entry = q.SingleOrDefault(x => x.IsActive(now));
                if (entry != null)
                {
                    entry.RawCount = entry.RawCount + (weight << FloatingBits);
                    ctx.Update(entry);
                }
                else
                {
                    entry = new HistoryEntry(now) { Route = route, Stop = stop, RawCount = weight << FloatingBits };
                    ctx.Insert(entry);
                }
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
                StopHistoryEntry entry = q.SingleOrDefault(e => e.IsActive(now));
                if (entry != null)
                {
                    entry.RawCount = entry.RawCount + (1 << FloatingBits);
                    ctx.Update(entry);
                }
                else
                {
                    entry = new StopHistoryEntry(now) { Stop = stop, RawCount = 1 << FloatingBits };
                    ctx.Insert(entry);
                }
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
                PlanningHistoryEntry entry = q.SingleOrDefault(e => e.IsActive(now));
                if (entry != null)
                {
                    entry.RawCount = entry.RawCount + (1 << FloatingBits);
                    ctx.Update(entry);
                }
                else
                {
                    entry = new PlanningHistoryEntry(now) { Stop = stop, RawCount = 1 << FloatingBits, IsSource = isSource };
                    ctx.Insert(entry);
                }
            }
        }

        public void SetRecentStopAtRoute(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                var recentEntry = ctx.RecentEntries.Where(e => e.RouteID == route.ID).SingleOrDefault();
                if (recentEntry != null)
                    ctx.Delete(recentEntry);

                var entry = new RecentEntry { Route = route, Stop = stop };
                ctx.Insert(entry);
            }
        }
        public StopGroup GetRecentStop(Route route)
        {
            lock (ctx)
            {
                var recentEntry = ctx.RecentEntries.Where(e => e.RouteID == route.ID).SingleOrDefault();
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
                DateTime twelveHoursEarlier = DateTime.Now - TimeSpan.FromHours(12);
                return ctx.RecentEntries
                    .Where(e => e.CreationTime > twelveHoursEarlier)
                    .OrderByDescending(e => e.CreationTime)
                    .ToList();
            }
        }

        public void Clear()
        {
            lock (ctx)
            {
                ctx.DeleteAll<HistoryEntry>();
                ctx.DeleteAll<StopHistoryEntry>();
                ctx.DeleteAll<PlanningHistoryEntry>();
                ctx.DeleteAll<RecentEntry>();
                if (HistoryCleared != null)
                    HistoryCleared(this, new EventArgs());
            }
        }

        public void OldenHistory()
        {
            lock (ctx)
            {
                foreach (var entry in ctx.HistoryEntries)
                {
                    entry.RawCount = 9 * entry.RawCount / 10;
                    ctx.Update(entry);
                }
                foreach (var entry in ctx.StopHistoryEntries)
                {
                    entry.RawCount = 9 * entry.RawCount / 10;
                    ctx.Update(entry);
                }
                foreach (var entry in ctx.PlanningHistoryEntries)
                {
                    entry.RawCount = 9 * entry.RawCount / 10;
                    ctx.Update(entry);
                }

                DateTime twelveHoursAgo = DateTime.Now - TimeSpan.FromHours(12);
                foreach (var entry in ctx.RecentEntries.Where(e => e.CreationTime < twelveHoursAgo))
                    ctx.Delete(entry);
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
    }
}
