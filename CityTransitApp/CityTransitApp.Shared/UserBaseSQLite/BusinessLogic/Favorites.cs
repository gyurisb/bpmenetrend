using System;
using System.Collections;
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
    public class Favorites : IFavorites
    {
        private UserBaseSQLiteContext ctx;
        public Favorites(UserBaseSQLiteContext ctx) { this.ctx = ctx; }

        public void Add(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                if (Contains(route, stop))
                    throw new InvalidOperationException("Duplicate key at Favorites.Add");

                var table = ctx.FavoriteEntries;
                int position = 1;
                if (table.FirstOrDefault() != null)
                    position = table.Max(fav => fav.Position ?? 0) + 1;
                FavoriteEntry entry = new FavoriteEntry
                {
                    Route = route,
                    Stop = stop,
                    Position = position
                };
                ctx.Insert(entry);
            }
        }
        public void Remove(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                var entry = ctx.FavoriteEntries.Where(e => e.RouteID == route.ID && e.StopID == stop.ID).Single();
                ctx.Delete(entry);
            }
        }

        public void PushForward(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                var originalEntry = ctx.FavoriteEntries.Where(e => e.RouteID == route.ID && e.StopID == stop.ID).Single();
                var nextEntry = ctx.FavoriteEntries.Where(e => e.Position > originalEntry.Position).MinBy(e => e.Position.Value);
                if (nextEntry != null)
                {
                    int? nextPos = nextEntry.Position;

                    nextEntry.Position = originalEntry.Position;
                    originalEntry.Position = nextPos;
                    ctx.Update(nextEntry);
                    ctx.Update(originalEntry);
                }
            }
        }
        public void PushBack(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                var originalEntry = ctx.FavoriteEntries.Where(e => e.RouteID == route.ID && e.StopID == stop.ID).Single();
                var prevEntry = ctx.FavoriteEntries.Where(e => e.Position < originalEntry.Position).MaxBy(e => e.Position.Value);
                if (prevEntry != null)
                {
                    int? prevPos = prevEntry.Position;

                    prevEntry.Position = originalEntry.Position;
                    originalEntry.Position = prevPos;
                    ctx.Update(prevEntry);
                    ctx.Update(originalEntry);
                }
            }
        }

        public bool Contains(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                return ctx.FavoriteEntries.Where(e => e.RouteID == route.ID && e.StopID == stop.ID).Count() > 0;
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
