using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TransitBase.Entities;
using TransitBase;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using System.Runtime.InteropServices.WindowsRuntime;
using CityTransitCommon.Elements;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace CityTransitServices.Tools
{
    public class AppTileUpdater
    {
        public static async Task UpdateTile(Route route, StopGroup stop, Grid containerGrid = null, SecondaryTile tile = null, bool doFlush = false)
        {
            DateTime now = DateTime.Now;
            int routeId = route.ID, stopId = stop.ID;
            var timeTable = TransitBaseComponent.Current.Logic.GetTimetable(route, stop, now);
            var control = await CreateStaticTileBackgroundControl(stop, route, timeTable, now);
            if (doFlush)
            {
                route = null;
                stop = null;
                timeTable = null;
                TransitBaseComponent.Current.Flush();
            }

            string imageFileName = "stoproute" + stopId + "-" + routeId + ".png";
            StorageFile imageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(imageFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            if (containerGrid != null)
                containerGrid.Children.Insert(0, control);
            await RenderTileImage(control, imageFile, control.Width, control.Height);
            if (containerGrid != null)
                containerGrid.Children.Remove(control);

            if (tile == null)
            {
                var tiles = await SecondaryTile.FindAllAsync();
                string tileArg = String.Format("{0}-{1}", routeId, stopId);
                tile = tiles.First(t => t.Arguments == tileArg);
            }

            tile.VisualElements.Square150x150Logo = new Uri("ms-appdata:///local/" + imageFileName);
            await tile.UpdateAsync();
        }

        public static RouteStopTile CreateTileBackgroundControl(StopGroup stop, Route route, Tuple<DateTime, Trip>[] timeTable, DateTime now)
        {
            var tileControl = new RouteStopTile
            {
                RouteShortName = route.RouteGroup.Name,
                RouteDirName = route.Name,
                StopName = stop.Name
            };
            DateTime hourNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var hourGroups = timeTable.Where(x => x.Item1 >= hourNow).GroupBy(x => (int)(x.Item1 - hourNow).TotalHours).ToList();
            int[] hours = hourGroups.Select(x => int.Parse(x.First().Item1.HourString())).ToArray();
            int[][] minutes = hourGroups.Select(x => x.Select(y => y.Item1.Minute).ToArray()).ToArray();
            tileControl.SetItemSource(hours, minutes);
            tileControl.FrameRoot.Background = new SolidColorBrush(Colors.Transparent);

            return tileControl;
        }

        public static async Task<FrameworkElement> CreateStaticTileBackgroundControl(StopGroup stop, Route route, Tuple<DateTime, Trip>[] timeTable, DateTime now)
        {
            DateTime hourNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var hourGroups = timeTable.Where(x => x.Item1 >= hourNow).GroupBy(x => (int)(x.Item1 - hourNow).TotalHours).ToList();
            int[] hours = hourGroups.Select(x => int.Parse(x.First().Item1.HourString())).ToArray();
            int[][] minutes = hourGroups.Select(x => x.Select(y => y.Item1.Minute).ToArray()).ToArray();

            return await RouteStopTile.CreateRouteStopTile(route.RouteGroup.Name, route.Name, stop.Name, hours, minutes);
        }

        public static async Task RenderTileImage(UIElement tileBackground, StorageFile file, double width, double height)
        {
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                //tileBackground.Measure(new Size(width, height));
                //tileBackground.UpdateLayout();
                //tileBackground.Arrange(new Rect(0, 0, width, height));

                var renderBmp = new RenderTargetBitmap();
                await renderBmp.RenderAsync(tileBackground, (int)width, (int)height);
                var buffer = await renderBmp.GetPixelsAsync();

                DataReader dataReader = DataReader.FromBuffer(buffer);
                byte[] data = new byte[buffer.Length];
	            dataReader.ReadBytes(data);

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)renderBmp.PixelWidth, (uint)renderBmp.PixelHeight, 96, 96, buffer.ToArray());
		        await encoder.FlushAsync();
            }
        }
    }
}
