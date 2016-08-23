using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase.Interface;

namespace UserBase.LinqToSQL
{
    public class TileRegister : ITileRegister
    {
        public static TileRegister Current { get; private set; }

        private DyanamicDBContext ctx;
        public TileRegister(DyanamicDBContext ctx)
        {
            this.ctx = ctx;
            Current = this;
        }

        public int Bind(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                TileEntry entry = new TileEntry { Route = route, Stop = stop };
                ctx.TileEntries.InsertOnSubmit(entry);
                ctx.SubmitChanges();
                return entry.EntryID;
            }
        }

        public IRouteStopPair Get(int id)
        {
            lock (ctx)
            {
                return ctx.TileEntries.FirstOrDefault(t => t.EntryID == id);
            }
        }

        public int Get(Route route, StopGroup stop)
        {
            lock (ctx)
            {
                TileEntry entry = ctx.TileEntries.FirstOrDefault(e => e.RouteID == route.ID && e.StopID == stop.ID);
                if (entry == null) return -1;
                else return entry.EntryID;
            }
        }

        public void Update()
        {
            lock (ctx)
            {
                try
                {
                    List<int> ids = new List<int>();

                    foreach (var tile in ShellTile.ActiveTiles)
                    {
                        if (tile.NavigationUri.OriginalString.StartsWith("/MainPage.xaml?tile="))
                        {
                            if (tile.NavigationUri.OriginalString.StartsWith("/MainPage.xaml?tile=stoproute")) ;
                            else
                            {
                                int id = Int32.Parse(tile.NavigationUri.OriginalString.Replace("/MainPage.xaml?tile=", ""));
                                ids.Add(id);
                            }
                        }
                    }

                    ctx.TileEntries.DeleteAllOnSubmit(ctx.TileEntries.ToList().Where(e => !ids.Contains(e.EntryID)));
                    ctx.SubmitChanges();
                }
                catch (Exception e)
                {
                }
            }
        }

        public IEnumerator<IRouteStopPair> GetEnumerator()
        {
            lock (ctx)
            {
                return ctx.TileEntries.ToList().GetEnumerator();
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    
}
