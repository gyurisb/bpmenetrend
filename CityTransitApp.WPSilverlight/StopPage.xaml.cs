using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using CityTransitApp.Common.ViewModels;
using TransitBase.Entities;
using CityTransitApp.WPSilverlight.Resources;
using System.Collections.ObjectModel;
using CityTransitServices.Tools;
using CityTransitApp.WPSilverlight.Tools;

namespace CityTransitApp.WPSilverlight
{
    public partial class StopPage : PhoneApplicationPage
    {
        private StopViewModel viewModel;
        private StopViewModel ViewModel
        {
            get { return viewModel; }
            set { this.DataContext = viewModel = value; }
        }

        #region Virtual viewmodel properties
        public ObservableCollection<StopGroupModel> ItemsSource { get { return ViewModel.ItemsSource; } }
        private StopGroup Stop { get { return ViewModel.Stop; } }
        private bool CurrentTime { get { return ViewModel.CurrentTime; } }
        private bool Near { get { return ViewModel.Near; } }
        private DateTime StartTime { get { return ViewModel.StartTime; } }
        private TransitBase.GeoCoordinate Location { get { return ViewModel.Location; } }
        private Stop SourceStop { get { return ViewModel.SourceStop; } }
        private StopParameter OriginalParameter { get { return ViewModel.OriginalParameter; } }
        protected bool ShowTransfers { get { return !CurrentTime; } }
        #endregion

        string postQuery;
        PeriodicTask contentSetterTask, flashTask;
        private LongListBottomObserver llObserver;

        public StopPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                string param = "";

                if (!NavigationContext.QueryString.TryGetValue("id", out param))
                {
                    throw new Exception("RoutePage opened without parameter");
                }
                StopGroup stop = App.Model.GetStopGroupByID(int.Parse(param));
                DateTime? dateTime = null;
                bool near = false;
                Stop sourceStop = null;
                bool noLocation = true;

                if (NavigationContext.QueryString.TryGetValue("dateTime", out param))
                {
                    dateTime = Convert.ToDateTime(param);
                    this.postQuery += "&dateTime=" + param;
                }

                if (NavigationContext.QueryString.TryGetValue("location", out param))
                {
                    if (param == "near") near = true;
                    else sourceStop = App.Model.GetStopByID(int.Parse(param));

                    noLocation = false;
                    this.postQuery += "&location=" + param;
                }

                this.ViewModel = new StopViewModel(addFooter: false);
                //ViewModel.Initialize(parameter, stateManager);
                ViewModel.Initialize(new StopParameter
                    {
                        StopGroup = stop,
                        DateTime = dateTime,
                        Location = noLocation ? null : new ParameterLocation
                        {
                            IsNear = near,
                            Stop = sourceStop
                        }
                    });

                if (dateTime == null)
                {
                    contentSetterTask = new PeriodicTask(ViewModel.TasksToSchedule.Single());
                    contentSetterTask.RunEveryMinute();

                    flashTask = new PeriodicTask(500, flashTimes);
                    flashTask.Run();
                }
            }
            else
            {
                if (contentSetterTask != null)
                    contentSetterTask.Resume();
                if (flashTask != null)
                    flashTask.Resume();
            }
            if (ShowTransfers)
            {
                llObserver = new LongListBottomObserver(ContentListView);
                llObserver.ScrollToBottom += ContentListView_ScrollToBottom;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (contentSetterTask != null)
                contentSetterTask.Cancel();
            if (flashTask != null)
                flashTask.Cancel();
            if (llObserver != null)
            {
                llObserver.Dispose();
                llObserver = null;
            }
        }

        private bool currentlyVisible = true;
        private void flashTimes()
        {
            currentlyVisible = !currentlyVisible;
            foreach (var group in ViewModel.ItemsSource)
                foreach (var item in group.Items)
                    if (item.NextTime == 0)
                        item.IsTimeVisible = currentlyVisible;
        }

        async void ContentListView_ScrollToBottom(object sender, EventArgs e)
        {
            if (llObserver == null)
                return;

            ViewModel.InProgress = true;

            bool hasMore = true;
            while (hasMore && llObserver.IsAtBottom())
            {
                hasMore = await ViewModel.AddNearStopToItemSource();
            }

            ViewModel.InProgress = false;
        }

        private void ContentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentListView.SelectedItem != null && ContentListView.SelectedItem is RouteModel)
            {
                RouteModel route = ContentListView.SelectedItem as RouteModel;
                StopGroup stop = route.Stop.Group;
                string uri = null;
                if (route.NextTripTime != null)
                {
                    uri = "/TripPage.xaml?tripID=" + route.NextTrip.ID + "&routeID=" + route.NextTrip.Route.ID + "&stopID=" + stop.ID + "&nexttrips=true" + postQuery;
                }
                else
                {
                    uri = "/TimetablePage.xaml?stopID=" + stop.ID + "&routeID=" + route.Route.ID;
                    if (!ViewModel.CurrentTime) uri += "&selectedTime=" + ViewModel.StartTime.ToString();
                }
                NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                ContentListView.SelectedItem = null;
            }
        }

        private void StopHeader_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            StopGroupModel selected = (StopGroupModel)((FrameworkElement)sender).DataContext;
            NavigationService.Navigate(new Uri("/MapPage.xaml?stopGroupID=" + selected.Stop.ID + postQuery, UriKind.Relative));
        }

        private void BuildLocalizedApplicationBar()
        {
            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppResources.HelpLabel;
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(AppResources.HelpStop);
        }
    }
}