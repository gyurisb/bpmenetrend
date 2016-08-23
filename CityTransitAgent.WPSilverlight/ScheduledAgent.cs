using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using CityTransitServices.Tools;
using CityTransitApp.Common;
using CityTransitApp.WPSilverlight.Tools;
using System;
using CityTransitApp.WPSilverlight.NetPortableServicesImplementations;

namespace CityTransitAgent.WPSilverlight
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected async override void OnInvoke(ScheduledTask task)
        {
            try
            {
                Config.Initialize();
                var services = new WPSilverlightCoreServices();
                var tb = new TransitBase.TransitBaseComponent(
                    root: services.FileSystem.GetAppStorageRoot(),
                    directionsService: null,
                    preLoad: false,
                    bigTableLimit: Config.Current.BigTableLimit,
                    checkSameRoutes: Config.Current.CheckSameRoutes,
                    latitudeDegreeDistance: Config.Current.LatitudeDegreeDistance,
                    longitudeDegreeDistance: Config.Current.LongitudeDegreeDistance
                    );
                var ub = new UserBase.UserBaseLinqToSQLComponent("Data Source=isostore:ddb.sdf", Config.Current.UBVersion, forbidMigration: true);
                var common = new CommonComponent(services, tb, ub);

                foreach (var tile in ShellTile.ActiveTiles)
                {
                    if (tile.NavigationUri.OriginalString.StartsWith("/MainPage.xaml?tile="))
                    {
                        if (tile.NavigationUri.OriginalString.StartsWith("/MainPage.xaml?tile=stoproute"))
                        {
                            updateTileUnknown(tile);
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Tiles.UpdateTile(tile);
                                //bool ret = Tiles.UpdateTile(tile);
                                //if (ret == false)
                                //    updateTileUnknown(tile);
                                tb.Flush();
                            });
                        }
                    }
                }
                if (NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && NetworkInterface.GetIsNetworkAvailable() && !AppFields.UpdateAvailable)
                {
                    var checkResult = await UpdateMonitor.CheckUpdate();
                    if (checkResult == UpdateMonitor.Result.Found)
                    {
                        ShellToast toast = new ShellToast();
                        toast.Title = "Update";
                        toast.Content = "A database update is required.";
                        toast.Show();
                    }
                }
            }
            catch (Exception) { }


            NotifyComplete();
        }

        private void updateTileUnknown(ShellTile tile)
        {
            StandardTileData tileData = new StandardTileData
            {
                Title = "Törölt csempe",
                BackgroundImage = new Uri("Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative)
            };
            tile.Update(tileData);
        }
    }
}