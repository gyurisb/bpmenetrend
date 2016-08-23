using CityTransitApp.CityTransitElements.BaseElements;
using CityTransitApp.CityTransitElements.PageElements;
using CityTransitApp.CityTransitElements.PageElements.SearchPanels;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Implementations;
using CityTransitElements.Controllers;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.Entities;
using UserBase.BusinessLogic;
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
using PlannerComponent.Interface;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CityTransitApp.ContentPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefaultPage : Page
    {
        private MainViewModel ViewModel { get; set; }
        private PageStateManager stateManager;
        private StopMapController mapController;

        private DateTime selectedDay = DateTime.Today;

        public DefaultPage()
        {
            this.InitializeComponent();
            this.stateManager = new PageStateManager(this);
            stateManager.InitializeState += InitializePage;
            //stateManager.RestoreState += RestorePage;

            App.UB.History.HistoryCleared += History_HistoryCleared;

            StopPicker.LoadItemsSourceTo(SourceBox, true);
            StopPicker.LoadItemsSourceTo(DestBox, false);

            DateTime time = DateTime.Now;
            time += TimeSpan.FromMinutes(time.Minute % 5 == 0 ? 0 : 5 - time.Minute % 5);
            HourBox.ItemsSource = Enumerable.Range(0, 24).ToList();
            HourBox.SelectedIndex = time.Hour;
            MinuteBox.ItemsSource = Enumerable.Range(0, 60 / 5).Select(i => (i * 5).ToString("D2")).ToList();
            MinuteBox.SelectedIndex = (int)(time.Minute / 5.0);
        }

        //void RestorePage(object obj)
        //{
        //    bindMapController();
        //}

        void History_HistoryCleared(object sender, EventArgs e)
        {
            ViewModel.SetContent();
        }

        private void InitializePage(object obj)
        {
            DataContext = ViewModel = new MainViewModel();
            ViewModel.SetContent();
            stateManager.ScheduleTaskEveryMinute(updateContent);
            bindMapController();
        }

        private void bindMapController()
        {
            this.mapController = new StopMapController();
            mapController.Bind(new WinMapProxy(Map), new StopParameter { IsNear = true });
            mapController.StopGroupSelected += mapController_StopGroupSelected;
            mapController.TimeTableSelected += mapController_TimeTableSelected;
            mapController.TripSelected += mapController_TripSelected;
        }
        #region Map element navigation handlers
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
        #endregion

        private void updateContent()
        {
            ViewModel.UpdateContent();
        }

        #region frame stack handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
            ViewModel.SetContent();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            stateManager.OnNavigatedFrom(e);
            map_PointerExited(null, null);
        }
        #endregion

        private void SpecialOptionsLabel_Tapped(object sender, PointerRoutedEventArgs e)
        {
            if (SpecialOptionsPanel.Visibility == Visibility.Collapsed)
            {
                SpecialOptionsPanel.Visibility = Visibility.Visible;
                SpecialOptionsArrow.Text = "" + (char)0xf107;
            }
            else
            {
                SpecialOptionsPanel.Visibility = Visibility.Collapsed;
                SpecialOptionsArrow.Text = "" + (char)0xf104;
            }
        }

        private void RemoveFavorites_Click(object sender, RoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            ViewModel.RemoveFavorite(pair);
        }

        private void FavUp_Click(object sender, RoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            ViewModel.FavUp(pair);
        }

        private void FavDown_Click(object sender, RoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            ViewModel.FavDown(pair);
        }


        private void Favorite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            Frame.Navigate(typeof(RoutePage), new TimetableParameter
            {
                Route = pair.Route,
                Stop = pair.Stop
            });
        }

        private async void History_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var routeGroup = (sender as FrameworkElement).DataContext as RouteGroup;
            var route = await UserEstimations.BestRouteAsync(routeGroup);
            var stop = await UserEstimations.BestStopAsync(route);
            Frame.Navigate(typeof(RoutePage), new TimetableParameter
            {
                Route = route,
                Stop = stop
            });
        }

        private void RouteStopPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        private void RouteStopPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            e.Handled = true;
        }

        private void PlanBtn_Click(object sender, RoutedEventArgs e)
        {
            StopGroup source = null, dest = null;

            var sourceModel = (StopPicker.StopModel)SourceBox.Selected;
            if (sourceModel != null)
                source = sourceModel.Value;
            var destModel = (StopPicker.StopModel)DestBox.Selected;
            if (destModel != null)
                dest = destModel.Value;

            if (source == null)
            {
                new MessageDialog("A kiindulási megálló nem található. Kérem adja meg újra!").ShowAsync();
                return;
            }
            if (dest == null)
            {
                new MessageDialog("A cél megálló nem található. Kérem adja meg újra!").ShowAsync();
                return;
            }

            DateTime selectedTime = selectedDay 
                + TimeSpan.FromHours((int)HourBox.SelectedItem)
                + TimeSpan.FromMinutes(int.Parse((string)MinuteBox.SelectedItem));
            PlanningTimeType planningType = TypeBox.SelectedIndex == 0 ? PlanningTimeType.Departure : PlanningTimeType.Arrival;

            Frame.Navigate(typeof(PlanningPage), new PlanningParameter
            {
                SourceStop = source,
                DestStop = dest,
                DateTime = selectedTime,
                PlanningType = planningType
            });

        }

        private void DateBox_Click(object sender, RoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((FrameworkElement)sender);
        }

        void Calendar_DateSelected(object sender, DateTime e)
        {
            DateText.Text = e.ToRelativeDateString();
            CalendarFlyout.Hide();
            selectedDay = e.Date;
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
    }
}
