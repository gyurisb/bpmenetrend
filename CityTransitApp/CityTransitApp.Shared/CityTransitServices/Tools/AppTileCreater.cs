using CityTransitApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TransitBase.Entities;
using UserBase.BusinessLogic;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CityTransitServices.Tools
{
    public class AppTileCreater
    {
        public static async Task<bool> CreateTile(StopGroup stop, Route route, Grid temporaryContainer = null)
        {
            string tileName = "stoproute" + stop.ID + "-" + route.ID;
            string tileArg = String.Format("{0}-{1}", route.ID, stop.ID);

            var tiles = await SecondaryTile.FindAllAsync();
            if (tiles.Any(t => t.Arguments == tileArg)) return false;
            int tileId = 0;
            if (tiles.Any())
                tileId = tiles.Max(tile => int.Parse(tile.TileId)) + 1;

            DateTime now = DateTime.Now;
            var timeTable = App.TB.Logic.GetTimetable(route, stop, now);

            var control = AppTileUpdater.CreateTileBackgroundControl(stop, route, timeTable, now);
            temporaryContainer.Children.Insert(0, control);

            StorageFile imageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(tileName + ".png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await AppTileUpdater.RenderTileImage(control, imageFile, control.FrameRoot.Width, control.FrameRoot.Height);
            temporaryContainer.Children.Remove(control);

            var secondaryTile = new SecondaryTile(
                    tileId: tileId.ToString(),
                    displayName: " ",
                    arguments: tileArg,
                    square150x150Logo: new Uri("ms-appdata:///local/" + tileName + ".png"),
                    desiredSize: TileSize.Square150x150
                    );

            bool isPinned = await secondaryTile.RequestCreateAsync();
            return isPinned;
        }

        public static async void CheckBackgroundAgent()
        {
#if WINDOWS_PHONE_APP
            var taskRegistered = false;
            var exampleTaskName = "TransitAppBackgroundTask";

            foreach (var task in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = typeof(CityTransitBgTaskCS.AppBackgroundTask).FullName;
                //builder.AddCondition(new SystemCondition(SystemConditionType.BackgroundWorkCostNotHigh));
                //builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
                builder.SetTrigger(new TimeTrigger(30, false));
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
                await BackgroundExecutionManager.RequestAccessAsync();
                BackgroundTaskRegistration task = builder.Register();
            }
#endif
        }

        public static void RemoveBackgroundAgent()
        {
#if WINDOWS_PHONE_APP
            BackgroundExecutionManager.RemoveAccess();
#endif
        }
    }
}
