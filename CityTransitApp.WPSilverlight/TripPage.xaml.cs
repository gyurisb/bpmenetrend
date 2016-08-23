using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.WPSilverlight.Tools;
using System.Threading.Tasks;
using CityTransitApp.Common.ViewModels;
using TransitBase.Entities;
using CityTransitServices.Tools;
using CityTransitApp.WPSilverlight.Resources;

namespace CityTransitApp.WPSilverlight
{
    public partial class TripPage : PhoneApplicationPage
    {
        private TripViewModel ViewModel { get; set; }

        #region virtual properties
        private Trip Trip { get { return ViewModel.Trip; } }
        private StopGroup Stop { get { return ViewModel.Stop; } }
        private TimeSpan[] Times { get { return ViewModel.Times; } }
        private DateTime DateTime { get { return ViewModel.DateTime; } }
        private DateTime NextTripTime { get { return ViewModel.NextTripTime; } }
        private int CurPos { get { return ViewModel.CurPos; } }
        private TransitBase.GeoCoordinate Location { get { return ViewModel.Location; } }
        private bool Near { get { return ViewModel.Near; } }
        private bool IsTimeSet { get { return ViewModel.IsTimeSet; } }
        private Stop SourceStop { get { return ViewModel.SourceStop; } }
        private bool ShowAmPm { get { return ViewModel.ShowAmPm; } }
        private bool ShowDistance { get { return ViewModel.ShowDistance; } }
        private int CurrentPageIndex { get { return ViewModel.CurrentPageIndex; } }
        private Stop CurrentStop { get { return Trip.Stops.First(x => x.Item2.Group == Stop).Item2; } }
        #endregion

        private PivotItem[] pivotItems;
        //private TabHeader TabHeader = new TabHeader();

        private PeriodicTask contentSetterTask;

        public TripPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                string stopID = "", tripID = "", routeID = "", dateStr = "", next = "", loc = "";
                //required parameters
                if (!NavigationContext.QueryString.TryGetValue("stopID", out stopID))
                    throw new FormatException("TimeTable opened without parameter stopID");
                if (!NavigationContext.QueryString.TryGetValue("tripID", out tripID))
                    throw new FormatException("TimeTable opened without parameter tripID");
                if (!NavigationContext.QueryString.TryGetValue("routeID", out routeID))
                    throw new FormatException("TimeTable opened without parameter routeID");

                bool isTimeSet = false, near = false, hasLocation;
                DateTime? dateTime = null;
                StopGroup stop = null;
                Trip trip = null;
                Stop sourceStop = null;
                TransitBase.GeoCoordinate location = null;

                //optional parameters
                if (isTimeSet = NavigationContext.QueryString.TryGetValue("dateTime", out dateStr))
                    dateTime = Convert.ToDateTime(dateStr);
                //else dateTime = DateTime.Now;
                if (hasLocation = NavigationContext.QueryString.TryGetValue("location", out loc))
                {
                    if (loc == "near")
                    {
                        near = true;
                        //location = CurrentLocation.Last;
                    }
                    else
                    {
                        sourceStop = App.Model.GetStopByID(int.Parse(loc));
                        location = sourceStop.Coordinate;
                    }
                }
                //boolean parameters
                bool nextTripStrip = NavigationContext.QueryString.TryGetValue("nexttrips", out next);
                stop = App.Model.GetStopGroupByID(System.Convert.ToInt32(stopID));
                trip = App.Model.GetTripByID(System.Convert.ToInt32(tripID), System.Convert.ToInt32(routeID));


                ViewModel = new TripViewModel();
                ViewModel.ScrollIntoViewRequired += ViewModel_ScrollIntoViewRequired;
                ViewModel.Initialize(new TripParameter
                {
                    Trip = trip,
                    NextTrips = nextTripStrip,
                    Stop = stop,
                    DateTime = dateTime,
                    Location = !hasLocation ? null : new ParameterLocation
                    {
                        IsNear = near,
                        Stop = sourceStop
                    }
                });
                this.DataContext = ViewModel;
                ViewModel.PostInizialize();
                if (ViewModel.TasksToSchedule.Any())
                {
                    contentSetterTask = new PeriodicTask(ViewModel.TasksToSchedule.Single());
                    contentSetterTask.RunEveryMinute(false);
                }
                if (ViewModel.IsTimeStripVisible)
                    addContentToPivot();
                setFavoriteIcon();
            }
            else
            {
                if (contentSetterTask != null)
                    contentSetterTask.Resume();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (contentSetterTask != null)
                contentSetterTask.Cancel();
        }

        private async void ViewModel_ScrollIntoViewRequired(object sender, object scrollTargetItem)
        {
            await Task.Delay(100);
            ContentListView.ScrollTo(scrollTargetItem);
        }

        private void ContentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentListView.SelectedItem != null)
            {
                var selected = (TimeStopListModel<Stop>)ContentListView.SelectedItem;
                ContentListView.SelectedItem = null;
                int index = ContentListView.ItemsSource.IndexOf(selected);
                DateTime dateTime = NextTripTime + Times[index];
                string uri = "/StopPage.xaml?id=" + selected.Stop.Group.ID + "&dateTime=" + dateTime.ToString() + "&location=" + selected.Stop.ID;
                NavigationService.Navigate(new Uri(uri, UriKind.Relative));
            }
        }

        private async void Direction_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            await ViewModel.ReverseDirection();
            setFavoriteIcon();
        }

        #region pageswapping

        private void addContentToPivot()
        {
            ViewModel.CurrentPageIndex = 0;
            ViewModel.usePivotPageing = true;

            LayoutRoot.Children.Remove(ContentListView);
            PivotPage0.Content = ContentListView;
            ContentPivot.Visibility = Visibility.Visible;
            pivotItems = new PivotItem[] { PivotPage0, PivotPage1, PivotPage2 };

            ContentPivot.SelectionChanged += ContentPivot_SelectionChanged;
        }

        private void TabHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int targetHeaderIndex = (int)e.AddedItems.Cast<object>().Single();
            int currentHeaderIndex = (int)e.RemovedItems.Cast<object>().Single();
            if (targetHeaderIndex < currentHeaderIndex)
            {
                swipePivotLeft();
            }
            else if (targetHeaderIndex > currentHeaderIndex)
            {
                swipePivotRight();
            }
        }
        private void swipePivotLeft()
        {
            int targetIndex = CurrentPageIndex - 1;
            if (targetIndex == -1) targetIndex = 2;
            fromHeaderSelected = true;
            ContentPivot.SelectedIndex = targetIndex;
        }
        private void swipePivotRight()
        {
            int targetIndex = CurrentPageIndex + 1;
            if (targetIndex == 3) targetIndex = 0;
            fromHeaderSelected = true;
            ContentPivot.SelectedIndex = targetIndex;
        }

        private bool fromHeaderSelected = false;
        private async void ContentPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool fromHeaderSelected = this.fromHeaderSelected;
            this.fromHeaderSelected = false;
            if (fromHeaderSelected)
                await Task.Delay(100);

            int prevPageIndex = ViewModel.CurrentPageIndex;
            ViewModel.CurrentPageIndex = ContentPivot.SelectedIndex;

            if ((CurrentPageIndex == 1 && prevPageIndex == 0) || (CurrentPageIndex == 2 && prevPageIndex == 1) || (CurrentPageIndex == 0 && prevPageIndex == 2))
            {
                var headerIndex = ViewModel.HeaderSelectedIndex;
                if (!fromHeaderSelected)
                    headerIndex += 1;
                await changeToHeader(headerIndex, fromHeaderSelected);
            }
            if ((CurrentPageIndex == 0 && prevPageIndex == 1) || (CurrentPageIndex == 1 && prevPageIndex == 2) || (CurrentPageIndex == 2 && prevPageIndex == 0))
            {
                var headerIndex = ViewModel.HeaderSelectedIndex;
                if (!fromHeaderSelected)
                    headerIndex -= 1;
                await changeToHeader(headerIndex, fromHeaderSelected);
            }
            pivotItems[prevPageIndex].Content = null;
            pivotItems[CurrentPageIndex].Content = ContentListView;
        }

        private async Task changeToHeader(int headerIndex, bool fromHeaderSelected)
        {
            if (headerIndex > 3 || headerIndex < 0)
            {
                if (fromHeaderSelected)
                    throw new ArgumentException("Cannot tap outside of the header.");
                Timetable_Click(this, null);

                if (headerIndex > 3) headerIndex = 3;
                else if (headerIndex < 0) headerIndex = 0;

                await Task.Delay(500);
            }
            else
            {
                ViewModel.ChangeToHeader(headerIndex, fromHeaderSelected);
            }
        }

        #endregion


        #region AppBar handlers

        private void BuildLocalizedApplicationBar()
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.TripMenuTimetable;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppResources.TripMenuMap;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).Text = AppResources.TimetableMenuAddFavs;
            (ApplicationBar.Buttons[3] as ApplicationBarIconButton).Text = AppResources.TimetableMenuPin;
            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppResources.HelpLabel;
        }

        private ApplicationBarIconButton favoriteMenuIcon
        {
            get { return ApplicationBar.Buttons[2] as ApplicationBarIconButton; }
        }
        private void setFavoriteIcon()
        {
            favoriteMenuIcon.IconUri = App.UB.Favorites.Contains(Trip.Route, Stop) ?
                new Uri("Assets/AppBar/favs.removefrom.png", UriKind.Relative) : new Uri("Assets/AppBar/favs.addto.png", UriKind.Relative);
        }

        private void Timetable_Click(object sender, EventArgs e)
        {
            if (NavigationService.BackStack.First().Source.OriginalString.Contains("TimetablePage"))
                NavigationService.GoBack();

            string uri = String.Format("/TimetablePage.xaml?stopID={0}&routeID={1}", Stop.ID, Trip.Route.ID);
            if (IsTimeSet) uri += "&selectedTime=" + NextTripTime.ToString();
            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void Map_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/MapPage.xaml?tripId={0}&routeId={1}&position={2}&dateTime={3}", Trip.ID, Trip.Route.ID, CurPos, NextTripTime), UriKind.Relative));
        }

        private void Pin_Clicked(object sender, EventArgs e)
        {
            bool ret = TilesApp.CreateTile(Stop, Trip.Route);

            if (ret == false)
            {
                //TODO MAGYAR SZÖVEG
                MessageBox.Show(App.Common.Services.Resources.LocalizedStringOf("PinAlreadyDone"));
                Tiles.UpdateTile(Stop, Trip.Route);
            }
        }

        private void Favorite_Clicked(object sender, EventArgs e)
        {

            if (!App.UB.Favorites.Contains(Trip.Route, Stop))
            {
                App.UB.Favorites.Add(Trip.Route, Stop);
                (sender as ApplicationBarIconButton).IconUri = new Uri("Assets/AppBar/favs.removefrom.png", UriKind.Relative);
            }
            else
            {
                App.UB.Favorites.Remove(Trip.Route, Stop);
                (sender as ApplicationBarIconButton).IconUri = new Uri("Assets/AppBar/favs.addto.png", UriKind.Relative);
            }
        }


        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(App.Common.Services.Resources.LocalizedStringOf("HelpTrip"));
        }
        #endregion
    }
}