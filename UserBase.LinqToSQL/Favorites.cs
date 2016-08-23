using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase.Interface;
using TransitBase;

namespace UserBase.LinqToSQL
{
    public class Favorites : IFavorites
    {
        private DyanamicDBContext ctx;
        public Favorites(DyanamicDBContext ctx) { this.ctx = ctx; }

        public void Add(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                if (Contains(route, stop)) throw new DuplicateKeyException(null);

                int position = 1;
                if (ctx.FavoriteEntries.FirstOrDefault() != null)
                    position = ctx.FavoriteEntries.Max(fav => fav.Position ?? 0) + 1;
                FavoriteEntry entry = new FavoriteEntry
                {
                    Route = route,
                    Stop = stop,
                    Position = position
                };
                ctx.FavoriteEntries.InsertOnSubmit(entry);
                ctx.SubmitChanges();
            }
        }
        public void Remove(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                ctx.FavoriteEntries.DeleteOnSubmit(
                    ctx.FavoriteEntries.First(e => e.RouteID == route.ID && e.StopID == stop.ID));
                ctx.SubmitChanges();
            }
        }

        public void PushForward(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                var originalEntry = ctx.FavoriteEntries.Single(e => e.RouteID == route.ID && e.StopID == stop.ID);
                var nextEntry = ctx.FavoriteEntries.Where(e => e.Position > originalEntry.Position).MinBy(e => e.Position.Value);
                if (nextEntry != null)
                {
                    int? nextPos = nextEntry.Position;

                    nextEntry.Position = originalEntry.Position;
                    originalEntry.Position = nextPos;
                    ctx.SubmitChanges();
                }
            }
        }
        public void PushBack(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                var originalEntry = ctx.FavoriteEntries.Single(e => e.RouteID == route.ID && e.StopID == stop.ID);
                var prevEntry = ctx.FavoriteEntries.Where(e => e.Position < originalEntry.Position).MaxBy(e => e.Position.Value);
                if (prevEntry != null)
                {
                    int? prevPos = prevEntry.Position;

                    prevEntry.Position = originalEntry.Position;
                    originalEntry.Position = prevPos;
                    ctx.SubmitChanges();
                }
            }
        }

        public bool Contains(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                return ctx.FavoriteEntries.Any(e => e.RouteID == route.ID && e.StopID == stop.ID);
            }
        }

        public IEnumerator<IFavoriteEntry> GetEnumerator()
        {
            lock (ctx)
            {
                return ctx.FavoriteEntries.ToList().GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
