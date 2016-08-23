using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase;
using UserBase.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TransitBase;
using Windows.Devices.Geolocation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Input;
using CityTransitApp.CityTransitElements.BaseElements;
using Windows.Phone.UI.Input;
using CityTransitApp.Pages.Dialogs;
using Windows.UI.Xaml.Controls.Maps;
using CityTransitServices;
using CityTransitApp.CityTransitElements.Dialogs;
using CityTransitApp.Common.Processes;
using Windows.UI;
using CityTransitServices.Tools;
using CityTransitApp.CityTransitElements.Pages;
using TransitBase.BusinessLogic;
using Windows.ApplicationModel.Store;
using Windows.UI.StartScreen;
using CityTransitApp.Pages;
using CityTransitApp.Common.ViewModels;
using Windows.UI.ViewManagement;
using CityTransitApp.CityTransitServices.Tools;
using CityTransitApp.CityTransitElements.PageParts;
using CityTransitApp.Common.ViewModels.Settings;
using CityTransitApp.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CityTransitApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public PageStateManager stateManager;
        public MainPage()
        {
            this.InitializeComponent();
            PlanningPart.SetDateTimePickerDialog(new DateTimePhoneDialog());
            BuildApplicationBar();
            stateManager = new PageStateManager(this);
            stateManager.InitializeState += InitializePageState;

            //if (App.Config.Advertising)
            //{
            //    AdControl.Visibility = Visibility.Visible;
            //    AdControl.ApplicationId = App.Config.ApplicationID;
            //    AdControl.AdUnitId = App.Config.MainAdUnitID;
            //}
        }

        public async void InitializePageState(object parameter)
        {
            if (!App.GetAppInfo().IsEnabled())
            {
                await new MessageDialog("Your trial period has expired. You cannot use the app until you buy the full version.").ShowAsync();
                App.Current.Exit();
            }

            InitializerProcess.SendStatisctics();
            Logging.Upload();

            if (!App.TB.DatabaseExists || AppFields.ForceUpdate)
            {
                var downloadResult = await DownloadDBDialog.Show();
                await DownloadDone(downloadResult, true);
            }
            if (InitializerProcess.FirstRun)
            {
                await ShowLocationConsentBox();
            }
            FavoritesPart.SetContent();
            stateManager.ScheduleTaskEveryMinute(updateContent);

            //if (App.SourceTileId != null)
            //{
            //    NavigateToTile(App.SourceTileId.Value);
            //}

            var checkUpdateResult = await UpdateMonitor.CheckUpdate();
            if (checkUpdateResult == UpdateMonitor.Result.Found)
                IndicateUpdateAvailable();

            //await Task.Delay(1000);
            //Frame.Navigate(typeof(TestPage));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                FavoritesPart.SetContent();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            stateManager.OnNavigatingFrom(e);
        }

        void PlanningPart_WaySelected(object sender, WayModel e)
        {
            Frame.Navigate(typeof(PlanDetailsPage), e);
        }

        private void FavoritesPart_FavoriteSelected(object sender, TimetableParameter e)
        {
            Frame.Navigate(typeof(TimetablePage), e);
        }

        private void FavoritesPart_HistoryItemSelected(object sender, RouteGroup e)
        {
            Frame.Navigate(typeof(HistoryItemPage), e);
        }

        private void FavoritesPart_NearStopSelected(object sender, StopGroup e)
        {
            Frame.Navigate(typeof(StopPage), new StopParameter { StopGroup = e, IsNear = true });
        }

        void FavoritesPart_RecentSelected(object sender, TimetableParameter e)
        {
            Frame.Navigate(typeof(TimetablePage), e);
        }

        public static int PLANNING_INDEX = 1;
        public static int FAVORITES_INDEX = 0;

        //private AdControl AdControl;

        //public void SetPlanningSource(StopGroup stop)
        //{
        //}
        //public void SetPlanningDestination(StopGroup stop)
        //{
        //}

        private async void updateContent()
        {
            FavoritesPart.UpdateContent();
            if (SettingsModel.AutomaticNearSearch)
            {
                var nearestStop = await StopTransfers.GetNearestStop(await CurrentLocation.Get());
                if (nearestStop != null)
                {
                    FavoritesPart.NearStop = nearestStop.Stop.Group;
                    PlanningPart.SetNearStop(nearestStop.Stop.Group, nearestStop.EstimatedDuration);
                }
            }
        }

        private async void IndicateUpdateAvailable()
        {
            DownloadIcon.IsEnabled = true;
            if (Pivot.SelectedIndex == 0)
                DownloadIcon.Visibility = Visibility.Visible;

            var progressIndicator = StatusBar.GetForCurrentView().ProgressIndicator;
            progressIndicator.Text = App.Common.Services.Resources.LocalizedStringOf("MainTopOutOfDate");
            progressIndicator.ProgressValue = 0.0;
            progressIndicator.ShowAsync();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Pivot.SelectedIndex == 0)
            {
                if (!FavoritesPart.FavoritesEmpty)
                    BeginnerTips.CheckTileTip();

                PlanSettingsIcon.Visibility = Visibility.Collapsed;

                SearchIcon.Visibility = Visibility.Visible;
                NearIcon.Visibility = Visibility.Visible;
                MapIcon.Visibility = Visibility.Visible;
                if (DownloadIcon.IsEnabled)
                    DownloadIcon.Visibility = Visibility.Visible;
            }
            else
            {
                SearchIcon.Visibility = Visibility.Collapsed;
                NearIcon.Visibility = Visibility.Collapsed;
                MapIcon.Visibility = Visibility.Collapsed;
                DownloadIcon.Visibility = Visibility.Collapsed;

                PlanSettingsIcon.Visibility = Visibility.Visible;
            }
        }

        private void Search_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
        }

        private void Near_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NearPage));
        }

        private void Map_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MapPage), new StopParameter { IsNear = true });
        }

        private async void Update_Clicked(object sender, RoutedEventArgs e)
        {
            var checkResult = await UpdateMonitor.CheckUpdate(force: true);
            var progressIndicator = StatusBar.GetForCurrentView().ProgressIndicator;
            switch (checkResult)
            {
                case UpdateMonitor.Result.Found:
                    IndicateUpdateAvailable();
                    break;
                case UpdateMonitor.Result.NotFound:
                    progressIndicator.Text = App.Common.Services.Resources.LocalizedStringOf("MainProgressNoUpdate");
                    progressIndicator.ProgressValue = 0.0;
                    progressIndicator.ShowAsync();
                    await Task.Delay(3000);
                    progressIndicator.HideAsync();
                    break;
                case UpdateMonitor.Result.NoAccess:
                    progressIndicator.Text = App.Common.Services.Resources.LocalizedStringOf("MainProgressNoInternet");
                    progressIndicator.ProgressValue = 0.0;
                    progressIndicator.ShowAsync();
                    await Task.Delay(3000);
                    progressIndicator.HideAsync();
                    break;
            }
        }

        private void Settings_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private async void Buy_Clicked(object sender, RoutedEventArgs e)
        {
            var res = await App.Services.Http.GetAsync("http://users.hszk.bme.hu/~gb1120/allow_purchuse.txt");
            if (res == null)
            {
                new MessageDialog("A vásárláshoz internetkapcsolat szükséges.").ShowAsync();
                return;
            }
            StreamReader reader = new StreamReader(res);
            string message = reader.ReadLine();
            if (message != "OK")
            {
                new MessageDialog(message).ShowAsync();
                return;
            }
            try
            {
                // Kick off purchase; don't ask for a receipt when it returns
                await CurrentApp.RequestProductPurchaseAsync("OfflinePlanning", false);

                // Now that purchase is done, give the user the goods they paid for
                AppFields.OfflinePlanningPurchused = true;
                AppFields.AcquisitionType = 10;
                new MessageDialog("Köszönjük a vásárlást.").ShowAsync();
            }
            catch (Exception ex)
            {
                new MessageDialog("A vásárlás sikertelen volt: " + ex.Message).ShowAsync();
                // When the user does not complete the purchase (e.g. cancels or navigates back from the Purchase Page), an exception with an HRESULT of E_FAIL is expected.
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var downloadResult = await DownloadDBDialog.Show();
            DownloadDone(downloadResult);
        }
        private async Task DownloadDone(DatabaseDownloadResult result, bool terminateOnFailure = false)
        {
            if (result == DatabaseDownloadResult.Success)
            {
                await new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("MainDownloadDone")).ShowAsync();
                App.Current.Exit();
                if (DownloadIcon.IsEnabled || DownloadIcon.Visibility == Visibility.Visible)
                {
                    DownloadIcon.IsEnabled = false;
                    DownloadIcon.Visibility = Visibility.Collapsed;
                    StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
                }
            }
            else if (result == DatabaseDownloadResult.NoAccess)
            {
                await new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("DownloadNoAccess")).ShowAsync();
                if (terminateOnFailure)
                    App.Current.Exit();
            }
            //this.SetValue(SystemTray.BackgroundColorProperty, Colors.White);
        }

        private async Task ShowLocationConsentBox()
        {
            var msgDialog = new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("AppLocationConsent"), App.Common.Services.Resources.LocalizedStringOf("AppLocationConsentTitle"));
            msgDialog.Commands.Clear();
            msgDialog.Commands.Add(new UICommand { Label = App.Common.Services.Resources.LocalizedStringOf("AppLocationConsentEnable"), Id = 0 });
            msgDialog.Commands.Add(new UICommand { Label = App.Common.Services.Resources.LocalizedStringOf("AppLocationConsentDisable"), Id = 1 });
            var res = await msgDialog.ShowAsync();
            if ((int)res.Id == 0)
                SettingsModel.LocationServices = true;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PlanSettingsPage));
        }

        private void BuildApplicationBar()
        {
            if (!App.Config.IAPOfflinePlanning)
            {
                IAPButton.Visibility = Visibility.Collapsed;
            }
        }

        public static async void NavigateToTile(int id)
        {
            var tiles = await SecondaryTile.FindAllAsync();
            var sourceTile = tiles.First(t => int.Parse(t.TileId) == id);
            string[] args = sourceTile.Arguments.Split('-');
            Route sourceRoute = App.TB.Logic.GetRouteByID(int.Parse(args[0]));
            StopGroup sourceStop = App.TB.Logic.GetStopGroupByID(int.Parse(args[1]));
            await Task.Delay(100);
            NavigateToRouteStopPage(null, sourceRoute, sourceStop);
        }

        public static void NavigateToRouteStopPage(Page sourcePage, Route route, StopGroup stop)
        {
            Frame frame = sourcePage != null ? sourcePage.Frame : (Frame)Window.Current.Content;
            if (App.Config.DefaultRouteStopPage == DefaultPageType.TimetablePage)
            {
                frame.Navigate(typeof(TimetablePage), new TimetableParameter { Stop = stop, Route = route });
            }
            else if (App.Config.DefaultRouteStopPage == DefaultPageType.TripPage)
            {
                var trip = TransitProvider.GetCurrentTrips(DateTime.Now, route, stop, 0, 1).SingleOrDefault();
                if (trip != null)
                {
                    frame.Navigate(typeof(TripPage), new TripParameter { Stop = stop, Trip = trip.Item2, NextTrips = true });
                }
                else
                {
                    frame.Navigate(typeof(TimetablePage), new TimetableParameter { Stop = stop, Route = route });
                }
            }
        }
    }
}
