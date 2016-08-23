using CityTransitApp.CityTransitElements.BaseElements;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.BusinessLogic;
using TransitBase.BusinessLogic.Helpers;
using TransitBase.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CityTransitElements.Controllers;
using CityTransitApp.Implementations;
using CityTransitServices.Tools;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CityTransitApp.ContentPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StopPage : Page
    {
        private PageStateManager<StopPageViewModel> stateManager;
        private StopMapController mapController;

        private StopPageViewModel ViewModel { get { return stateManager.ViewModel; } }

        public StopPage()
        {
            this.InitializeComponent();
            stateManager = new PageStateManager<StopPageViewModel>(this);
            stateManager.InitializeState += Initialize;
            stateManager.RestoreState += Restore;
        }

        private void Restore()
        {
            bindMapController();
        }

        private void Initialize(object obj)
        {
            var param = (StopParameter)obj;
            bindMapController();
            initializeNearStops(param);
        }

        private async Task initializeNearStops(StopParameter param)
        {
            if (param.Location == null || param.Location.IsNear)
            {
                this.ViewModel.NearStops = param.StopGroup
                    .Transfers(300)
                    .GroupBy(t => t.Target.Group)
                    .Where(g => g.Key != param.StopGroup)
                    .Select(g => new NearStopModel { Stop = g.Key, Distance = g.Average(t => t.Distance) })
                    .ToList();
            }
            //else if (param.Location.IsNear)
            //{
            //    throw new NotImplementedException();
            //}
            else if (param.Location.Stop != null)
            {
                var nearStops = await StopTransfers.NearStopsFrom(param.Location.Stop, 300);
                this.ViewModel.NearStops = nearStops
                    .GroupBy(t => t.Stop.Group)
                    .Where(g => g.Key != param.StopGroup)
                    .Select(g => new NearStopModel { Stop = g.Key, Distance = g.Average(t => t.DistanceInMeters) })
                    .OrderBy(m => m.Distance)
                    .ToList();
            }
            else throw new InvalidOperationException();
        }

        private void bindMapController()
        {
            this.mapController = new StopMapController();
            mapController.Bind(new WinMapProxy(Map), ViewModel.OriginalParameter);
            mapController.StopGroupSelected += mapController_StopGroupSelected;
            mapController.TimeTableSelected += mapController_TimeTableSelected;
            mapController.TripSelected += mapController_TripSelected;
        }

        void mapController_StopGroupSelected(object sender, StopParameter e)
        {
            Frame.Navigate(typeof(StopPage), e);
        }

        void mapController_TimeTableSelected(object sender, TimetableParameter e)
        {
            Frame.Navigate(typeof(RoutePage), e);
        }

        void mapController_TripSelected(object sender, TripParameter e)
        {
            Frame.Navigate(typeof(RoutePage), new TimetableParameter
            {
                Route = e.Trip.Route,
                SelectedTime = e.DateTime,
                Stop = e.Stop
            });
        }

        #region frame stack handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            stateManager.OnNavigatedFrom(e);
            map_PointerExited(null, null);
            mapController.Dispose();
        }
        #endregion

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        void RouteGrid_ItemSelected(object sender, object e)
        {
            var selected = (RouteModel)e;
            Frame.Navigate(typeof(RoutePage), new TimetableParameter
            {
                Route = selected.Route,
                SelectedTime = selected.NextTripTime ?? ViewModel.OriginalParameter.DateTime ?? DateTime.Now,
                Stop = ViewModel.Stop
            });
        }

        void NearStops_ItemSelected(object sender, ItemClickEventArgs e)
        {
            var selected = (NearStopModel)e.ClickedItem;
            Frame.Navigate(typeof(StopPage), new StopParameter
            {
                StopGroup = selected.Stop,
                DateTime = ViewModel.OriginalParameter.DateTime,
                Location = ViewModel.OriginalParameter.Location
            });
        }

        private void map_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ContentScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
            MapCanvas.Visibility = Visibility.Collapsed;
        }

        private void map_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ContentScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
            MapCanvas.Visibility = Visibility.Visible;
        }

        public class StopPageViewModel : StopViewModel
        {
            public StopPageViewModel()
            {
                ItemsSource.CollectionChanged += (sender, e) => routesListsChanged();

            }

            //Virtual properties
            public Visibility TransferVisibility { get { return OriginalParameter.DateTime != null ? Visibility.Visible : Visibility.Collapsed; } }
            public string TimeText { get { return OriginalParameter.DateTime != null ? OriginalParameter.DateTime.Value.ToRelativeString() : ""; } }

            public IList DepartureList { get { return ItemsSource.Any() ? ItemsSource.First().Items.Where(x => x.NextTime != null).ToList() : null; } }
            public IList OtherRoutesList { get { return ItemsSource.Any() ? ItemsSource.First().Items.Where(x => x.NextTime == null).ToList() : null; } }
            public bool DepartureListAny { get { return DepartureList != null && DepartureList.Cast<object>().Any(); } }
            public bool OtherRoutesListAny { get { return OtherRoutesList != null && OtherRoutesList.Cast<object>().Any(); } }

            //Derived properties
            public IList NearStops { get { return Get<IList>(); } set { Set(value); nearChanged(); } }
            public bool NearStopsAny { get { return NearStops != null && NearStops.Cast<object>().Any(); } }

            private void routesListsChanged()
            {
                base.OnPropertyChanged("DepartureList");
                base.OnPropertyChanged("OtherRoutesList");
                base.OnPropertyChanged("DepartureListAny");
                base.OnPropertyChanged("OtherRoutesListAny");
            }
            private void nearChanged()
            {
                base.OnPropertyChanged("NearStopsAny");
            }
        }
        private class NearStopModel
        {
            public StopGroup Stop { get; set; }
            public double Distance { get; set; }
            public string DistanceWithUnit
            {
                get { return StringFactory.LocalizeDistanceWithUnit(Distance); }
            }
        }
    }
}
