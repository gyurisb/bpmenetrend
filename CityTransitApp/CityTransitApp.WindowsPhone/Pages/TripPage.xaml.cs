using CityTransitApp.CityTransitElements.PageElements;
using CityTransitElements.Controllers;
using CityTransitElements.Effects;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public sealed partial class TripPage : Page
    {
        private TripViewModel ViewModel { get { return stateManager.ViewModel; } }

        #region virtual properties
        private Trip Trip { get { return ViewModel.Trip; } }
        private StopGroup Stop { get { return ViewModel.Stop; } }
        private TimeSpan[] Times { get { return ViewModel.Times; } }
        private DateTime DateTime { get { return ViewModel.DateTime; } }
        private DateTime NextTripTime { get { return ViewModel.NextTripTime; } }
        private int CurPos { get { return ViewModel.CurPos; } }
        private GeoCoordinate Location { get { return ViewModel.Location; } }
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

        public PageStateManager<TripViewModel> stateManager;
        public TripPage()
        {
            InitializeComponent();
            stateManager = new PageStateManager<TripViewModel>(this);
            stateManager.InitializeState += InitializePageState;
            stateManager.SaveState += SavePageState;
            stateManager.RestoreState += RestorePageState;
        }
        #region frame stack handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            stateManager.OnNavigatedFrom(e);
        }
        #endregion

        private void RestorePageState()
        {
            ViewModel.ScrollIntoViewRequired += ViewModel_ScrollIntoViewRequired;
            if (ViewModel.usePivotPageing)
                addContentToPivot();
            setFavoriteIcon();
            if (ViewModel.ScrollPosition != null)
                ContentListView.ScrollToGroupedPosition((ListViewPositionResult)ViewModel.ScrollPosition);
        }

        private void SavePageState()
        {
            ViewModel.ScrollPosition = ContentListView.GetScrollPosition();
            ViewModel.ScrollIntoViewRequired -= ViewModel_ScrollIntoViewRequired;
        }

        private void InitializePageState(object parameter)
        {
            ViewModel.ScrollIntoViewRequired += ViewModel_ScrollIntoViewRequired;
            var param = (TripParameter)parameter;
            ViewModel.PostInizialize();
            if (ViewModel.IsTimeStripVisible)
                addContentToPivot();
            setFavoriteIcon();
        }

        private void ViewModel_ScrollIntoViewRequired(object sender, object scrollTargetItem)
        {
            ContentListView.ForceScrollIntoView(scrollTargetItem);
        }

        private void ContentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentListView.SelectedItem != null)
            {
                var selected = (TimeStopListModel<Stop>)ContentListView.SelectedItem;
                ContentListView.SelectedItem = null;
                Frame.Navigate(typeof(StopPage), new StopParameter
                {
                    StopGroup = selected.Stop.Group,
                    DateTime = ViewModel.GetTimeOf(selected),
                    SourceStop = selected.Stop
                });
            }
        }

        private async void Direction_Tap(object sender, TappedRoutedEventArgs e)
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
            int targetHeaderIndex = (int)e.AddedItems.Single();
            int currentHeaderIndex = (int)e.RemovedItems.Single();
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

        private void setFavoriteIcon()
        {
            FavoriteMenuIcon.Icon = App.UB.Favorites.Contains(Trip.Route, Stop) ? new SymbolIcon(Symbol.UnFavorite) : new SymbolIcon(Symbol.Favorite);  
        }
        private void Timetable_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.BackStack.First().SourcePageType == typeof(TimetablePage))
                Frame.GoBack();

            Frame.Navigate(typeof(TimetablePage), new TimetableParameter
            {
                Stop = Stop,
                Route = Trip.Route,
                SelectedTime = IsTimeSet ? NextTripTime : (DateTime?)null
            });
        }

        private void Map_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MapPage), new TripParameter
            {
                Trip = Trip,
                //DateTime = ViewModel.IsTimeSet ? ViewModel.DateTime : (DateTime?)null
                DateTime = ViewModel.GetTimeOfCurrentStop(),
                Stop = ViewModel.Stop
            });
        }

        private async void Pin_Clicked(object sender, RoutedEventArgs e)
        {
            bool ret = await AppTileCreater.CreateTile(Stop, Trip.Route, LayoutRoot);

            if (ret == false)
            {
                //TODO MAGYAR SZÖVEG
                new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PinAlreadyDone")).ShowAsync();
                AppTileUpdater.UpdateTile(Trip.Route, Stop, LayoutRoot);
            }
        }

        private void Favorite_Clicked(object sender, RoutedEventArgs e)
        {

            if (!App.UB.Favorites.Contains(Trip.Route, Stop))
            {
                App.UB.Favorites.Add(Trip.Route, Stop);
                FavoriteMenuIcon.Icon = new SymbolIcon(Symbol.UnFavorite);
            }
            else
            {
                App.UB.Favorites.Remove(Trip.Route, Stop);
                FavoriteMenuIcon.Icon = new SymbolIcon(Symbol.Favorite);
            }
        }


        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("HelpTrip")).ShowAsync();
        }
        #endregion

    }
}
