using CityTransitApp.CityTransitElements.PageElements.SearchPanels;
using CityTransitApp.CityTransitElements.PageParts;
using CityTransitApp.CityTransitElements.Pages;
using CityTransitApp.CityTransitServices.Tools;
using CityTransitApp.NetPortableServicesImplementations;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.BusinessLogic;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CityTransitApp.Common.ViewModels.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NearPage : Page
    {
        private class Distance
        {
            public int Value { get; private set; }
            public Distance(int distanceInMeters)
            {
                Value = distanceInMeters;
            }
            public override string ToString()
            {
                return StringFactory.LocalizeDistanceWithUnit(Value);
            }
        }

        private static Distance[] DistanceSelection = new Distance[] { new Distance(100), new Distance(250), new Distance(500), new Distance(1000), new Distance(2000) };

        private Distance radius;
        private bool initialized = false;
        private bool setNextTime = false;

        public PageStateManager stateManager;
        public NearPage()
        {
            InitializeComponent();
            radius = DistanceSelection[1];
            BtnDist.ItemsSource = DistanceSelection;
            stateManager = new PageStateManager(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New || setNextTime)
            {
                setNextTime = false;
                SetContent();
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            stateManager.OnNavigatedFrom(e);
        }

        private async Task setMostNarrowRange()
        {
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
            StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            for (int i = 0; i < DistanceSelection.Length; i++)
            {
                radius = DistanceSelection[i];
                var nearStops = (await StopTransfers.NearStops(await CurrentLocation.Get(), radius.Value)).ToList();
                if (nearStops.Count > 0)
                {
                    BtnDist.SelectedItem = radius;
                    ListView.ItemsSource = nearStops.Select(s => new StopModel(s.StopGroup, true)).ToList();
                    break;
                }
            }
            StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
        }

        public async void SetContent()
        {
            ListView.Visibility = Visibility.Collapsed;

            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
            StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            var loc = await CurrentLocation.Get();
            StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            if (loc == null)
            {
                if (SettingsModel.LocationServices == false)
                    showConsentMessage();
                else
                    showUnavailableMessage();
            }
            else
            {
                ListView.Visibility = Visibility.Visible;
                await setMostNarrowRange();
                initialized = true;
            }
        }

        public async void SetListContent()
        {
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
            StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            var location = await CurrentLocation.Get();

            //logging
            UniversalDirections.QueryTimes.Clear();
            var sw = Stopwatch.StartNew();
            var nearStops = await StopTransfers.NearStops(location, radius.Value);
            sw.Stop();

            ListView.ItemsSource = nearStops.Select(s => new StopModel(s.StopGroup, true)).ToList();
            ListView.Height = ListView.ItemsSource().Count > 0 ? double.NaN : 0;
            StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();

            PerfLogging.AddRow(
                "setListContent",
                sw.Elapsed,
                UniversalDirections.QueryTimes.Count,
                UniversalDirections.QueryTimes.Count > 0 ? UniversalDirections.QueryTimes.Average(x => x.TotalMilliseconds) : 0.0);
        }

        private void StopListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItem != null)
            {
                var selected = (sender as ListView).SelectedItem as StopModel;
                Frame.Navigate(typeof(StopPage), new StopParameter { StopGroup = selected.Stop, IsNear = true });
                (sender as ListView).SelectedItem = null;
            }
        }

        private void BtnDist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized)
            {
                if (e.RemovedItems.Count > 0 && e.AddedItems[0] != e.RemovedItems[0])
                {
                    radius = (Distance)BtnDist.SelectedItem;
                    SetListContent();
                }
            }
        }


        private async Task showConsentMessage()
        {
            bool isOk = await showCustomMessageBox(
                App.Common.Services.Resources.LocalizedStringOf("NearLocationServices"),
                App.Common.Services.Resources.LocalizedStringOf("NearLocDescription"),
                App.Common.Services.Resources.LocalizedStringOf("NearLocButton")
                );
            if (isOk)
            {
                Frame.Navigate(typeof(SettingsPage));
                setNextTime = true;
            }
        }

        private async Task showUnavailableMessage()
        {
            bool isOk = await showCustomMessageBox(
                App.Common.Services.Resources.LocalizedStringOf("NearUnavailable"),
                App.Common.Services.Resources.LocalizedStringOf("NearUnavailableDescr"),
                App.Common.Services.Resources.LocalizedStringOf("NearUnavailableButton")
                );
            if (isOk)
                SetContent();
        }

        private async Task<bool> showCustomMessageBox(string title, string content, string button)
        {
            ContentDialog box = new ContentDialog
            {
                Title = title,
                Content = new TextBlock
                {
                    Text = content,
                    TextWrapping = TextWrapping.Wrap,
                    //FontSize = 23
                },
                PrimaryButtonText = button,
                SecondaryButtonText = App.Common.Services.Resources.LocalizedStringOf("NearCancelButton"),
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                Foreground = (Brush)App.Current.Resources["ApplicationForegroundThemeBrush"]
            };
            var result = await box.ShowAsync();
            return result == ContentDialogResult.Primary;
            //if (result == ContentDialogResult.Primary)
            //{
            //    okResultAction();
            //}
            //else NavigationService.GoBack();
        }
    }
}
