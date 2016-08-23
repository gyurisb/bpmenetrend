using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media;
using UserBase.Interface;
using TransitBase.Entities;
using TransitBase;
using CityTransitApp.WPSilverlight.PageElements;

namespace CityTransitApp.WPSilverlight.Tools
{
    public static partial class Tiles
    {
        public static ShellTile Get(int tileId)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.ToString().Contains("tile=" + tileId));
        }

        public static void UpdateTile(ShellTile tile)
        {
            int id = Int32.Parse(tile.NavigationUri.OriginalString.Replace("/MainPage.xaml?tile=", ""));

            IRouteStopPair pair = App.UB.TileRegister.Get(id);
            if (pair == null) return;

            StopGroup stop = pair.Stop;
            Route route = pair.Route;

            DateTime now = DateTime.Now;
            var timeTable = App.Model.GetTimetable(route, stop, now);

            var control = CreateTileBackgroundControl(stop, route, timeTable, now);
            Uri imageUri = GenerateTileImage(control, id + ".png");

            StandardTileData tileData = new StandardTileData
            {
                Title = "",
                BackgroundImage = imageUri
            };
            tile.Update(tileData);

            //return true;
        }

        public static RouteStopTile CreateTileBackgroundControl(StopGroup stop, Route route, Tuple<DateTime, Trip>[] timeTable, DateTime now)
        {
            var tileControl = new RouteStopTile
            {
                RouteShortName = route.RouteGroup.Name,
                RouteDirName = route.Name,
                StopName = stop.Name,
                //Hour1 = int.Parse(now.HourString()),
                //Hour2 = int.Parse((now + TimeSpan.FromHours(1)).HourString()),
                //TimeSource1 = timeTable.Where(x => x.Item1.Hour == now.Hour).Select(x1 => x1.Item1.Minute.ToString()).ToArray(),
                //TimeSource2 = timeTable.Where(x => x.Item1.Hour == now.Hour + 1).Select(x1 => x1.Item1.Minute.ToString()).ToArray(),
            };
            DateTime hourNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var hourGroups = timeTable.Where(x => x.Item1 >= hourNow).GroupBy(x => (int)(x.Item1 - hourNow).TotalHours).ToList();
            int[] hours = hourGroups.Select(x => int.Parse(x.First().Item1.HourString())).ToArray();
            int[][] minutes = hourGroups.Select(x => x.Select(y => y.Item1.Minute).ToArray()).ToArray();
            tileControl.SetItemSource(hours, minutes);
            tileControl.FrameRoot.Background = new SolidColorBrush(Colors.Transparent);

            return tileControl;
        }

        //forrás: http://www.visuallylocated.com/post/2014/04/19/Creating-live-tiles-with-WriteableBitmap-and-transparent-backgrounds.aspx
        public static Uri GenerateTileImage(RouteStopTile tileBackground, string fileName)
        {
            fileName = Path.Combine("Shared", "ShellContent", fileName);
            double width = tileBackground.FrameRoot.Width;
            double height = tileBackground.FrameRoot.Height;

            using (var stream = IsolatedStorageFile.GetUserStoreForApplication().CreateFile(fileName))
            {
                tileBackground.Measure(new Size(width, height));
                tileBackground.Arrange(new Rect(0, 0, width, height));
                var bitmap = new WriteableBitmap(tileBackground, null);
                //bitmap.SaveJpeg(stream, (int)tileBackground.Width, (int)tileBackground.Height, 0, 100);
                bitmap.WritePNG(stream);
            }
            return new Uri("isostore:/" + fileName, UriKind.Absolute);
        }

        public static void UpdateTile(StopGroup stopGroup, Route route)
        {
            int id = App.UB.TileRegister.Get(route, stopGroup);
            UpdateTile(Get(id));
        }
    }
}
