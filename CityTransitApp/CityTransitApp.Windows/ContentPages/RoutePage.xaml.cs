using CityTransitApp.CityTransitElements.BaseElements;
using CityTransitApp.CityTransitElements.PageElements;
using CityTransitApp.CityTransitElements.PageParts;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.BusinessLogic.Helpers;
using TransitBase.Entities;
using UserBase.BusinessLogic;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using CityTransitElements.Controllers;
using CityTransitApp.Implementations;
using CityTransitElements.Effects;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CityTransitApp.ContentPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RoutePage : Page
    {
        //private RoutePageViewModel viewModel;
        //private RoutePageViewModel ViewModel
        //{
        //    get { return viewModel; }
        //    set { DataContext = viewModel = value; }
        //}
        private RoutePageViewModel ViewModel { get { return stateManager.ViewModel; } }

        private TripMapController mapController;
        private PageStateManager<RoutePageViewModel> stateManager;

        public RoutePage()
        {
            this.InitializeComponent();
            this.LayoutUpdated += Page_LayoutUpdated;
            this.stateManager = new PageStateManager<RoutePageViewModel>(this);
            stateManager.InitializeState += InitializePage;
            stateManager.RestoreState += RestorePage;
            stateManager.SaveState += SavePage;

            double screenWidth = App.GetAppInfo().GetScreenWidth();
            if (screenWidth > 1600)
            {
                ListColumn.Width = new GridLength(screenWidth * 0.2);
                DetailsColumn.Width = new GridLength(screenWidth * 0.3);
                MapColumn.Width = new GridLength(screenWidth * 0.5);
            }
            else
            {
                ListColumn.Width = new GridLength(screenWidth * 0.2);
                DetailsColumn.Width = new GridLength(screenWidth * 0.4);
                MapColumn.Width = new GridLength(screenWidth * 0.8);
            }
        }

        void Page_LayoutUpdated(object sender, object e)
        {
            var flyoutStyle = new Style(typeof(FlyoutPresenter));
            flyoutStyle.Setters.Add(new Setter(FrameworkElement.MaxHeightProperty, Map.ActualHeight + 80));
            StopListFlyout.FlyoutPresenterStyle = flyoutStyle;
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
            if (mapController != null)
                mapController.Dispose();
        }
        #endregion

        private void RestorePage()
        {
            ViewModel.Timetable.PropertyChanged += timetable_PropertyChanged;
            ViewModel.Trip.ScrollIntoViewRequired += trip_ScrollIntoViewRequired;
            MainPage.Current.SetHeaderBrush(ViewModel.Timetable.Route.RouteGroup.GetColors().BgColorBrush);
            MainPage.Current.SetHeaderFontBrush(ViewModel.Timetable.Route.RouteGroup.GetColors().FontColorBrush);
            bindMapController();
        }

        private void SavePage()
        {
            ViewModel.Timetable.PropertyChanged -= timetable_PropertyChanged;
            ViewModel.Trip.ScrollIntoViewRequired -= trip_ScrollIntoViewRequired;
        }

        private async void InitializePage(object obj)
        {
            ViewModel.Timetable.PropertyChanged += timetable_PropertyChanged;
            ViewModel.Trip.ScrollIntoViewRequired += trip_ScrollIntoViewRequired;
            var timetableParam = (TimetableParameter)obj;
            MainPage.Current.SetHeaderBrush(timetableParam.Route.RouteGroup.GetColors().BgColorBrush);
            MainPage.Current.SetHeaderFontBrush(timetableParam.Route.RouteGroup.GetColors().FontColorBrush);
            ViewModel.Trip.PostInizialize();
            if (ViewModel.NoInitialStop)
            {
                await Task.Delay(250);
                ShowRouteListFlyout(StopHeaderText, null);
            }
            bindMapController();
        }

        private void bindMapController()
        {
            if (mapController != null)
                mapController.Dispose();

            var tripParam = new TripParameter
            {
                Stop = ViewModel.Trip.Stop,
                Trip = ViewModel.Trip.Trip,
                DateTime = ViewModel.Trip.GetTimeOfCurrentStop()
            };

            this.mapController = new TripMapController();
            mapController.Bind(new WinMapProxy(Map), tripParam);
            mapController.StopSelected += mapController_StopSelected;
        }

        void mapController_StopSelected(object sender, StopParameter e)
        {
            Frame.Navigate(typeof(StopPage), e);
        }

        private void timetable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BodySource")
            {
                if (ViewModel.Timetable.BodySource.ScrollTarget != null)
                {
                    TimetableList.ScrollIntoView(ViewModel.Timetable.BodySource.ScrollTarget);
                }
            }
        }

        private void trip_ScrollIntoViewRequired(object sender, object item)
        {
            TripStopsList.ScrollIntoView(ViewModel.TripList.First(x => x.RoutedItem == item));
        }

        private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            ViewModel.Timetable.SetBodyContentAsync();
        }

        //private void RouteBorder_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    ViewModel.Timetable.ReverseDirection();
        //}

        //private HashSet<SimpleGridView> gridViews = new HashSet<SimpleGridView>();
        //private void GridView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    gridViews.Add((SimpleGridView)sender);
        //}
        //private void GridView_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    gridViews.Remove((GridView)sender);
        //}

        private void GridView_SelectionChanged(object sender, object selectedItem)
        {
            //SimpleGridView senderView = (SimpleGridView)sender;
            //foreach (var view in gridViews)
            //    if (view != senderView && view.SelectedItem != null)
            //        view.SelectedItem = null;

            var minute = (TimeTableBodyMinute)selectedItem;
            if (ViewModel.SelectedMinute != null)
                ViewModel.SelectedMinute.IsSelected = false;
            minute.IsSelected = true;
            ViewModel.SelectedMinute = minute;
            ViewModel.Trip.ChangeToTrip(minute.Time, minute.Trip);
            bindMapController();
        }

        private void ShowAttachedFlyout(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Calendar_DateSelected(object sender, DateTime e)
        {
            ViewModel.Timetable.SelectedDay = e;
            CalendarFlyout.Hide();
            ViewModel.Timetable.SetBodyContentAsync();
        }

        private void ShowRouteListFlyout(object sender, TappedRoutedEventArgs e)
        {
            ShowAttachedFlyout(sender, e);
            ViewModel.RouteStopList = ViewModel.Timetable.Route.TravelRoute;
        }

        private async void RouteStopSelected(object sender, object item)
        {
            RouteStopEntry selected = (RouteStopEntry)item;
            StopListFlyout.Hide();
            ViewModel.Timetable.Stop = selected.Stop;
            ViewModel.Timetable.SetRouteStopValue();
            ViewModel.Timetable.SetBodyContentAsync();
            bool success = ViewModel.Trip.ChangeStop(selected.Stop);
            if (success)
                bindMapController();
        }

        private void DirectionSelected(object sender, object item)
        {
            Route newRoute = (Route)item;
            if (newRoute != ViewModel.Timetable.Route)
            {
                ViewModel.Timetable.ReverseDirection(newRoute);
                ViewModel.Trip.ReverseDirection(newRoute);

                ViewModel.TripParam.Stop = ViewModel.Timetable.Stop;
                ViewModel.TripParam.Trip = ViewModel.Trip.Trip;
                ViewModel.TripParam.DateTime = ViewModel.Trip.GetTimeOfCurrentStop();
                bindMapController();
            }
            DirectionListFlyout.Hide();
        }

        void TripStopsList_ItemSelected(object sender, object e)
        {
            TripElementModel selected = (TripElementModel)e;
            Frame.Navigate(typeof(StopPage), new StopParameter
            {
                StopGroup = selected.RoutedItem.Stop.Group, 
                SourceStop = selected.RoutedItem.Stop, 
                DateTime = ViewModel.Trip.GetTimeOf(selected.RoutedItem) 
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

        #region ViewModel classes

        public class RoutePageViewModel : ViewModel<TimetableParameter>
        {
            public RouteGroup RouteGroup { get { return Timetable.Route.RouteGroup; } }
            public bool NoInitialStop { get; private set; }
            public TripParameter TripParam { get; private set; }

            //body data
            public TimetableViewModel Timetable { get; private set; }
            public TripViewModel Trip { get; private set; }
            public TripListModel TripList { get { return Get<TripListModel>(); } set { Set(value); } }
            public TimeTableBodyMinute SelectedMinute;

            //header data
            public IList<RouteStopEntry> RouteStopList { get { return Get<IList<RouteStopEntry>>(); } set { Set(value); } }

            //colors
            public Brush PrimaryColorBrush { get { return RouteGroup.GetColors().BgColorBrush; } }
            public Brush SecondaryColorBrush { get { return new SolidColorBrush(Colors.Black); } }
            public Brush ForegroundBrush { get { return RouteGroup.GetColors().FontColorBrush; } }

            public override void Initialize(TimetableParameter timetableParam)
            {
                if (timetableParam.Stop == null)
                {
                    timetableParam.Stop = timetableParam.Route.TravelRoute.First().Stop;
                    NoInitialStop = true;
                }
                Timetable = new TimetableViewModel();
                Timetable.Initialize(timetableParam);
                AddTasksToSchedule(Timetable.TasksToSchedule);

                var nextTrip = TransitProvider.GetCurrentTrips(timetableParam.SelectedTime ?? DateTime.Now, timetableParam.Route, timetableParam.Stop, 0, 1).Single();
                if (nextTrip == null)
                    nextTrip = TransitProvider.GetCurrentTrips(timetableParam.SelectedTime ?? DateTime.Now, timetableParam.Route, timetableParam.Stop, 1, 0).Single();
                if (nextTrip == null)
                    nextTrip = Tuple.Create(DateTime.Now, timetableParam.Route.Trips.First());
               
                this.TripParam = new TripParameter
                {
                    Trip = nextTrip.Item2,
                    Stop = timetableParam.Stop,
                    DateTime = timetableParam.SelectedTime
                };
                Trip = new TripViewModel();
                Trip.Initialize(TripParam);
                AddTasksToSchedule(Trip.TasksToSchedule);
                setTripList();

                Trip.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "ItemsSource")
                    {
                        setTripList();
                    }
                };
            }

            private void setTripList()
            {
                var tripListElements = Trip.ItemsSource.Select(x => new TripElementModel { Position = x.Position, StopName = x.Stop.Name, RoutedItem = x });
                var tripList = new TripListModel(tripListElements);
                tripList.SelectedIndex = 0;
                this.TripList = tripList;
            }
        }

        public class TripListModel : List<TripElementModel>
        {
            public TripListModel(IEnumerable<TripElementModel> elements)
                : base(elements)
            {
                foreach (var item in this)
                    item.Outer = this;
            }
            public int SelectedIndex;
        }

        public class TripElementModel
        {
            public TripListModel Outer;
            public TimeStopListModel<Stop> RoutedItem;
            public int Position { get; set; }
            public string StopName { get; set; }
            public string TimeText { get { return RoutedItem.Time; } }

            public bool IsNotFirst { get { return this != Outer.First(); } }
            public bool IsNotLast { get { return this != Outer.Last(); } }
            public bool IsSelected { get { return Position == Outer.SelectedIndex; } }
            public bool IsBeforeSelected { get { return Position < Outer.SelectedIndex; } }
            public bool IsAfterSelected { get { return Position > Outer.SelectedIndex; } }

            public Brush ForegroundBrush { get { return IsBeforeSelected ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.Black); } }
            public Brush BackgroundBrush { get { return IsSelected ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.Transparent); } }
        }

        #endregion

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Timetable.ToggleFavorite();
        }
    }

}
