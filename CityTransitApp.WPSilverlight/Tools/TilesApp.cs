using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TransitBase.Entities;

namespace CityTransitApp.WPSilverlight.Tools
{
    class TilesApp
    {
        private static string taskName = "BkvCsempeFrissito";

        public static bool CreateTile(StopGroup stop, Route route)
        {
            string tileName = "stoproute" + stop.ID + "-" + route.ID;

            int tileId = App.UB.TileRegister.Get(route, stop);
            if (tileId != -1) return false;

            DateTime now = DateTime.Now;
            var timeTable = App.Model.GetTimetable(route, stop, now);

            var control = Tiles.CreateTileBackgroundControl(stop, route, timeTable, now);
            Uri imageUri = Tiles.GenerateTileImage(control, tileName + ".png");

            StandardTileData tileData = new StandardTileData
            {
                Title = "",
                BackgroundImage = imageUri
            };

            int id = App.UB.TileRegister.Bind(route, stop);
            Uri tileUri = new Uri("/MainPage.xaml?tile=" + id, UriKind.Relative);
            ShellTile.Create(tileUri, tileData);

            return true;
        }

        public static void CheckBackgroundAgent()
        {
            PeriodicTask tileUpdaterPeriodicTask;

            tileUpdaterPeriodicTask = ScheduledActionService.Find(taskName) as PeriodicTask;

            if (tileUpdaterPeriodicTask == null)
            {
                tileUpdaterPeriodicTask = new PeriodicTask(taskName);
                tileUpdaterPeriodicTask.Description = "Élőcsempék menetrend adatainak frissítését végzi az alkalmazás a háttérben.";

                try
                {
                    ScheduledActionService.Add(tileUpdaterPeriodicTask);
                }
                catch (InvalidOperationException exception)
                {
                    if (exception.Message.Contains("BNS Error: The action is disabled"))
                    {
                        MessageBox.Show("Background agents for this application have been disabled by the user.");
                    }
                    else MessageBox.Show(exception.Message);
                }
                catch (Exception e) { }
            }

            if (Debugger.IsAttached)
            {
                try
                {
                    ScheduledActionService.LaunchForTest(
                        taskName,
                        TimeSpan.FromSeconds(10));
                }
                catch (Exception) { }
            }
        }

        public static void RemoveBackgroundAgent()
        {
            if (ScheduledActionService.Find(taskName) != null)
                ScheduledActionService.Remove(taskName);
        }
    }
}
