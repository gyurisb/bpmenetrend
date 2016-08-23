using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.BusinessLogic.Helpers;
using TransitBase.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StopPage : Page
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
        private GeoCoordinate Location { get { return ViewModel.Location; } }
        private Stop SourceStop { get { return ViewModel.SourceStop; } }
        private StopParameter OriginalParameter { get { return ViewModel.OriginalParameter; } }
        protected bool ShowTransfers { get { return !CurrentTime; } }
        #endregion

        private LongListBottomObserver llObserver;
        
        private bool currentlyVisible = true;
        private void flashTimes()
        {
            currentlyVisible = !currentlyVisible;
            foreach (var group in ItemsSource)
                foreach (var item in group.Items)
                    if (item.NextTime == 0)
                        item.IsTimeVisible = currentlyVisible;
        }

        public PageStateManager stateManager;
        public StopPage()
        {
            InitializeComponent();

            stateManager = new PageStateManager(this, true);
            stateManager.InitializeState += InitializePageState;
            stateManager.SaveState += SavePageState;
            stateManager.RestoreState += RestorePageState;
        }
        private void registerBottomObserver()
        {
            if (ShowTransfers)
            {
                llObserver = new LongListBottomObserver(ContentListView);
                llObserver.ScrollToBottom += ContentListView_ScrollToBottom;
            }
        }
        private void unregisterBottomObserver()
        {
            if (llObserver != null)
            {
                llObserver.ScrollToBottom -= ContentListView_ScrollToBottom;
                llObserver.Dispose();
                llObserver = null;
            }
        }
        #region frame stack handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            stateManager.OnNavigatedFrom(e);
            unregisterBottomObserver();
        }
        #endregion

        private void RestorePageState(object obj)
        {
            this.ViewModel = (StopViewModel)obj;
            var viewSource = (CollectionViewSource)Resources["src"];
            viewSource.Source = ViewModel.ItemsSource;
            if (ViewModel.ScrollPosition != null)
                ContentListView.ScrollToGroupedPosition((ListViewPositionResult)ViewModel.ScrollPosition);
            registerBottomObserver();
        }

        private object SavePageState()
        {
            ViewModel.ScrollPosition = ContentListView.GetGroupedScrollPosition(ViewModel.ItemsSource);
            return ViewModel;
        }

        protected async void InitializePageState(object parameter)
        {
            var param = (StopParameter)parameter;
            this.ViewModel = new StopViewModel();
            //ViewModel.Initialize(parameter, stateManager);
            ViewModel.Initialize(parameter);
            var viewSource = (CollectionViewSource)Resources["src"];
            viewSource.Source = ViewModel.ItemsSource;
            registerBottomObserver();

            if (ViewModel.CurrentTime)
                stateManager.ScheduleTask<StopPage>(500, page => page.flashTimes());
            foreach (var task in ViewModel.TasksToSchedule)
                stateManager.ScheduleTaskEveryMinute(task);
        }

        async void ContentListView_ScrollToBottom(object sender, EventArgs e)
        {
            if (llObserver == null)
                return;

            //if (nearStops.Count > 0)
            //{
            ViewModel.InProgress = true;

            while (llObserver.IsAtBottom())
            {
                bool hasMore = await ViewModel.AddNearStopToItemSource();
                if (!hasMore)
                    break;
            }

            ViewModel.InProgress = false;
            //}
        }

        private void ContentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentListView.SelectedItem != null && ContentListView.SelectedItem is RouteModel)
            {
                RouteModel route = ContentListView.SelectedItem as RouteModel;
                StopGroup stop = route.Stop.Group;
                if (route.NextTripTime != null)
                {
                    Frame.Navigate(typeof(TripPage), new TripParameter
                    {
                        Trip = route.NextTrip,
                        Stop = stop,
                        NextTrips = true,
                        DateTime = OriginalParameter.DateTime,
                        Location = OriginalParameter.Location
                    });
                }
                else
                {
                    Frame.Navigate(typeof(TimetablePage), new TimetableParameter
                    {
                        Route = route.Route,
                        Stop = stop,
                        SelectedTime = OriginalParameter.DateTime
                    });
                }
                ContentListView.SelectedItem = null;
            }
        }

        private void StopHeader_Tap(object sender, TappedRoutedEventArgs e)
        {
            StopGroupModel selected = (StopGroupModel)((FrameworkElement)sender).DataContext;
            Frame.Navigate(typeof(MapPage), new StopParameter
                {
                    StopGroup = selected.Stop,
                    DateTime = OriginalParameter.DateTime,
                    Location = OriginalParameter.Location
                });
        }


        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("HelpStop")).ShowAsync();
        }

    }

    #region footer grouping

    public class RouteListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalItemTemplate { get; set; }
        public DataTemplate FooterItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is RouteModel)
                return NormalItemTemplate;
            if (item is StopGroupModel)
                return FooterItemTemplate;

            throw new NotSupportedException();
        }
    }

    #endregion

    #region converters

    public class BottomMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new Thickness(0, 0, 0, (double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
