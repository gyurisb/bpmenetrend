using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase.Entities;
using UserBase.Interface;
using Windows.UI.StartScreen;

namespace UserBase.BusinessLogic
{
    //public class TileRegister : ITileRegister
    //{
    //    public static TileRegister Current { get; private set; }

    //    private UserBaseContext ctx;
    //    public TileRegister(UserBaseContext ctx)
    //    {
    //        if (Current != null)
    //            throw new InvalidOperationException("SingletonException: Only a single TileRegister object can be present.");
    //        Current = this;
    //        this.ctx = ctx;
    //    }

    //    public int Bind(Route route, StopGroup stop)
    //    {
    //        lock (ctx)
    //        {
    //            TileEntry entry = new TileEntry { Route = route, Stop = stop };
    //            ctx.Insert(entry);
    //            return entry.EntryID;
    //        }
    //    }

    //    public IRouteStopPair Get(int id)
    //    {
    //        lock (ctx)
    //        {
    //            return ctx.TileEntries.Where(t => t.EntryID == id).FirstOrDefault();
    //        }
    //    }

    //    public int Get(Route route, StopGroup stop)
    //    {
    //        lock (ctx)
    //        {
    //            TileEntry entry = ctx.TileEntries.Where(e => e.RouteID == route.ID && e.StopID == stop.ID).FirstOrDefault();
    //            if (entry == null) return -1;
    //            else return entry.EntryID;
    //        }
    //    }

    //    public async void Update()
    //    {
    //        var tiles = await SecondaryTile.FindAllAsync();
    //        var ids = new HashSet<int>(tiles.Select(x => int.Parse(x.TileId)));
    //        lock (ctx)
    //        {
    //            foreach (var entry in ctx.TileEntries)
    //            {
    //                if (!ids.Contains(entry.EntryID))
    //                    ctx.Delete(entry);
    //            }
    //        }
    //    }

    //    public IEnumerator<IRouteStopPair> GetEnumerator()
    //    {
    //        lock (ctx)
    //        {
    //            return ctx.TileEntries.ToList().GetEnumerator();
    //        }
    //    }
    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return this.GetEnumerator();
    //    }
    //}
}
