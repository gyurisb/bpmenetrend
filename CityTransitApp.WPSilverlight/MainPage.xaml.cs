using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.WPSilverlight.Resources;
using CityTransitServices.Tools;
using System.Device.Location;
using System.Diagnostics;
using System.Threading.Tasks;
using CityTransitApp.Common.Processes;
using TransitBase.Entities;
using CityTransitApp.Common;
using System.Windows.Media;
using Microsoft.Advertising.Mobile.UI;
using UserBase.Interface;
using System.IO;
using CityTransitApp.WPSilverlight.Dialogs;
using CityTransitApp.Common.ViewModels.Settings;
using TransitBase.BusinessLogic;
using Windows.ApplicationModel.Store;

namespace CityTransitApp.WPSilverlight
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static MainPage Current;

        private ApplicationBarIconButton filterMenuIcon;
        private ApplicationBarIconButton searchMenuIcon;
        private ApplicationBarIconButton nearMenuIcon;
        private ApplicationBarIconButton mapMenuIcon;
        private ApplicationBarIconButton updateMenuIcon;

        private PeriodicTask contentSetterTask;

        public static int PLANNING_INDEX = 1;
        public static int FAVORITES_INDEX = 0;

        private AdControl AdControl;

        public MainPage()
        {
            Current = this;
            InitializeComponent();
            DataContext = this;
            searchMenuIcon = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            nearMenuIcon = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            mapMenuIcon = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            filterMenuIcon = ApplicationBar.Buttons[3] as ApplicationBarIconButton;
            updateMenuIcon = new ApplicationBarIconButton { IconUri = new Uri("/Assets/AppBar/download.png", UriKind.Relative), IsEnabled = false };
            updateMenuIcon.Click += UpdateButton_Click;
            BuildLocalizedApplicationBar();
            BuildApplicationBar();

            ApplicationBar.Buttons.Remove(filterMenuIcon);

            if (App.Config.Advertising)
            {
                AdControl.Visibility = Visibility.Visible;
                AdControl.ApplicationId = App.Config.ApplicationID;
                AdControl.AdUnitId = App.Config.MainAdUnitID;
            }
        }

        private Task<Tuple<double, TimeSpan, TimeSpan>> calculateRoute(GeoCoordinate start, GeoCoordinate end)
        {
            Tuple<double, TimeSpan, TimeSpan> result = null;
            var task = new Task<Tuple<double, TimeSpan, TimeSpan>>(() => result);

            var query = new Microsoft.Phone.Maps.Services.RouteQuery
            {
                TravelMode = Microsoft.Phone.Maps.Services.TravelMode.Walking,
                Waypoints = new GeoCoordinate[] { start, end }
            };
            var sw = Stopwatch.StartNew();
            query.QueryCompleted += delegate(object sender, Microsoft.Phone.Maps.Services.QueryCompletedEventArgs<Microsoft.Phone.Maps.Services.Route> e)
            {
                sw.Stop();
                if (e.Error == null)
                {
                    var resultRoute = e.Result;
                    double length = resultRoute.LengthInMeters;
                    TimeSpan time = resultRoute.EstimatedDuration;
                    query.Dispose();

                    result = Tuple.Create(length, time, sw.Elapsed);
                }
                task.Start();
            };
            query.QueryAsync();
            return task;
        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (!App.AppEnabled)
                {
                    MessageBox.Show("Your trial period has expired. You cannot use the app until you buy the full version.");
                    App.Current.Terminate();
                }

                //InitializerProcess.SendStatisctics();

                if (!App.DatabaseExists() || AppFields.ForceUpdate)
                {
                    var downloadResult = await DownloadDBDialog.Show();
                    DownloadDone(downloadResult);
                }
                if (InitializerProcess.FirstRun)
                {
                    await ShowLocationConsentBox();
                }
                FavoritesTab.SetContent();
                contentSetterTask = new PeriodicTask(updateContent);
                contentSetterTask.RunEveryMinute();

                string tile;
                if (NavigationContext.QueryString.TryGetValue("tile", out tile) && !tile.StartsWith("stoproute"))
                {
                    IRouteStopPair pair = App.UB.TileRegister.Get(Int32.Parse(tile));
                    if (pair != null)
                        NavigateToRouteStopPage(this, pair.Route, pair.Stop);
                }

                var checkUpdateResult = await UpdateMonitor.CheckUpdate();
                if (checkUpdateResult == UpdateMonitor.Result.Found)
                    IndicateUpdateAvailable();
            }
            else
            {
                FavoritesTab.SetContent();
                if (contentSetterTask != null)
                    contentSetterTask.Resume();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (contentSetterTask != null)
                contentSetterTask.Cancel();
        }

        private async void updateContent()
        {
            FavoritesTab.UpdateContent();
            if (SettingsModel.AutomaticNearSearch)
            {
                var nearestStop = await StopTransfers.GetNearestStop(await CurrentLocation.Get());
                if (nearestStop != null)
                {
                    FavoritesTab.NearStop = nearestStop.Stop.Group;
                    PlanningTab.SetNearStop(nearestStop.Stop.Group, nearestStop.EstimatedDuration);
                }
            }
        }

        private void IndicateUpdateAvailable()
        {
            ProgressIndicator prog = new ProgressIndicator { Text = AppResources.MainTopOutOfDate, IsVisible = true };
            SystemTray.SetProgressIndicator(this, prog);

            updateMenuIcon.IsEnabled = true;
            if (!ApplicationBar.Buttons.Contains(updateMenuIcon))
                ApplicationBar.Buttons.Add(updateMenuIcon);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Pivot.SelectedIndex == 0)
            {
                ApplicationBar.Buttons.Clear();
                if (!FavoritesTab.FavoriteEmpty)
                    BeginnerTips.CheckTileTip();
                ApplicationBar.Buttons.Add(searchMenuIcon);
                ApplicationBar.Buttons.Add(nearMenuIcon);
                ApplicationBar.Buttons.Add(mapMenuIcon);
                if (updateMenuIcon.IsEnabled)
                    ApplicationBar.Buttons.Add(updateMenuIcon);
            }
            else
            {
                ApplicationBar.Buttons.Clear();
                ApplicationBar.Buttons.Add(filterMenuIcon);
            }
        }

        private void Search_Clicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.Relative));
        }

        private void Near_Clicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NearPage.xaml", UriKind.Relative));
        }

        private void Map_Clicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MapPage.xaml?stopGroupID=0&location=near", UriKind.Relative));
        }

        private async void Update_Clicked(object sender, EventArgs e)
        {
            var checkResult = await UpdateMonitor.CheckUpdate(force: true);
            switch (checkResult)
            {
                case UpdateMonitor.Result.Found:
                    IndicateUpdateAvailable();
                    break;
                case UpdateMonitor.Result.NotFound:
                    ProgressIndicator prog = new ProgressIndicator { Text = AppResources.MainProgressNoUpdate, IsVisible = true };
                    SystemTray.SetProgressIndicator(this, prog);
                    await Task.Delay(3000);
                    SystemTray.SetProgressIndicator(this, null);
                    break;
                case UpdateMonitor.Result.NoAccess:
                    ProgressIndicator prog1 = new ProgressIndicator { Text = AppResources.MainProgressNoInternet, IsVisible = true };
                    SystemTray.SetProgressIndicator(this, prog1);
                    await Task.Delay(3000);
                    SystemTray.SetProgressIndicator(this, null);
                    break;
            }
        }

        private void Settings_Clicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings/SettingsPage.xaml", UriKind.Relative));
        }

        private async void Buy_Clicked(object sender, EventArgs e)
        {
            var res = await App.Services.Http.GetAsync("http://users.hszk.bme.hu/~gb1120/allow_purchuse.txt");
            if (res == null)
            {
                MessageBox.Show("A vásárláshoz internetkapcsolat szükséges.");
                return;
            }
            StreamReader reader = new StreamReader(res);
            string message = reader.ReadLine();
            if (message != "OK")
            {
                MessageBox.Show(message);
                return;
            }
            try
            {
                // Kick off purchase; don't ask for a receipt when it returns
                await CurrentApp.RequestProductPurchaseAsync("OfflinePlanning", false);

                // Now that purchase is done, give the user the goods they paid for
                AppFields.OfflinePlanningPurchused = true;
                AppFields.AcquisitionType = 10;
                MessageBox.Show("Köszönjük a vásárlást.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("A vásárlás sikertelen volt: " + ex.Message);
                // When the user does not complete the purchase (e.g. cancels or navigates back from the Purchase Page), an exception with an HRESULT of E_FAIL is expected.
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            var downloadResult = await DownloadDBDialog.Show();
            DownloadDone(downloadResult);
        }
        private void DownloadDone(DatabaseDownloadResult result)
        {
            if (result == DatabaseDownloadResult.Success)
            {
                MessageBox.Show(AppResources.MainDownloadDone);
                App.Current.Terminate();
                //if (updateButton.IsEnabled)
                //{
                //    updateButton.IsEnabled = false;
                //    ApplicationBar.Buttons.Remove(updateButton);
                //    SystemTray.SetProgressIndicator(this, null);
                //}
            }
            else if (result == DatabaseDownloadResult.NoAccess)
            {
                MessageBox.Show(AppResources.DownloadNoAccess);
            }
            this.SetValue(SystemTray.BackgroundColorProperty, Colors.White);
        }

        private Task ShowLocationConsentBox()
        {
            Task task = new Task(() => { });
            CustomMessageBox box = new CustomMessageBox
            {
                Caption = AppResources.AppLocationConsentTitle,
                Content = new TextBlock { Text = AppResources.AppLocationConsent, TextWrapping = System.Windows.TextWrapping.Wrap, FontSize = 22, Margin = new Thickness(5, 20, 10, 10) },
                LeftButtonContent = AppResources.AppLocationConsentEnable,
                RightButtonContent = AppResources.AppLocationConsentDisable
            };
            box.Dismissed += (sender, args) =>
            {
                if (args.Result == CustomMessageBoxResult.LeftButton)
                    SettingsModel.LocationServices = true;
                this.SetValue(SystemTray.BackgroundColorProperty, Colors.White);
                task.Start();
            };
            box.Show();
            return task;
        }

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
#if DEBUG
            MessageBox.Show(e.ErrorCode.ToString() + ": " + e.Error.Message);
#endif
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings/PlanSettingsPage.xaml", UriKind.Relative));
        }

        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            filterMenuIcon.Text = AppResources.MainMenuFilter;
            searchMenuIcon.Text = AppResources.MainMenuSearch;
            nearMenuIcon.Text = AppResources.MainMenuNear;
            updateMenuIcon.Text = AppResources.MainMenuUpdate;
            mapMenuIcon.Text = AppResources.MainMenuMap;

            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppResources.MainMenuCheckUpdate;
            (ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).Text = AppResources.MainMenuSettings;
            (ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Text = AppResources.MainMenuOfflinePlanning;
        }

        private void BuildApplicationBar()
        {
            if (!App.Config.IAPOfflinePlanning)
            {
                ApplicationBar.MenuItems.RemoveAt(2);
            }
        }

        public static void NavigateToRouteStopPage(PhoneApplicationPage sourcePage, Route route, StopGroup stop)
        {
            if (App.Config.DefaultRouteStopPage == DefaultPageType.TimetablePage)
            {
                sourcePage.NavigationService.Navigate(new Uri("/TimetablePage.xaml?stopID=" + stop.ID + "&routeID=" + route.ID, UriKind.Relative));
            }
            else if (App.Config.DefaultRouteStopPage == DefaultPageType.TripPage)
            {
                var trip = App.Model.GetCurrentTrips(DateTime.Now, route, stop, 0, 1).SingleOrDefault();
                if (trip != null)
                {
                    sourcePage.NavigationService.Navigate(new Uri("/TripPage.xaml?nexttrips=true&stopID=" + stop.ID + "&routeID=" + route.ID + "&tripID=" + trip.Item2.ID, UriKind.Relative));
                }
                else
                {
                    sourcePage.NavigationService.Navigate(new Uri("/TimetablePage.xaml?stopID=" + stop.ID + "&routeID=" + route.ID, UriKind.Relative));
                }
            }
        }
    }
}