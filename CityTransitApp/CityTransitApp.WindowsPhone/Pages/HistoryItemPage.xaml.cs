using CityTransitElements.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase.BusinessLogic;
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
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using UserBase.Interface;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryItemPage : Page
    {
        //TODO legnépszerűbb két megálló megjelenítése legfelül
        private RouteGroup route;

        public PageStateManager stateManager;
        public HistoryItemPage()
        {
            InitializeComponent();
            Animations.OnMouseColorChange(Dir1Border);
            Animations.OnMouseColorChange(Dir2Border);
            stateManager = new PageStateManager(this);
            stateManager.InitializeState += InitializePageState;
        }

        public string RouteNumber { get { return route.Name; } }
        public Brush HeaderColor { get { return route.GetColors().PrimaryColorBrush; } }
        public Brush RouteColor { get { return route.GetColors().BgColorBrush; } }
        public Brush MainColor { get { return route.GetColors().MainColorBrush; } }
        public Brush FontColor { get { return route.GetColors().FontColorBrush; } }

        public async void InitializePageState(object parameter)
        {
            route = (RouteGroup)parameter;
            DataContext = this;

            await Task.Delay(25);

            if (route.FirstRoute != null)
                Dir1Text.Text = route.FirstRoute.Name;
            if (route.SecondRoute != null)
                Dir2Text.Text = route.SecondRoute.Name;

            bool even = true;
            //var stops = route.FirstRoute.StopTimes.Select(x => x.Item2).Union(route.SecondRoute.StopTimes.Select(x0 => x0.Item2));
            ListView.ItemsSource = App.UB.History.TimetableEntries
                .Where(item => item.Route.RouteGroup == route)
                .GroupBy(x => x.Stop)
                .Select(x => new
                {
                    Stop = x.Key,
                    Rating1 = x.Min(y => HistoryHelpers.DayPartDistance(y)),
                    Rating2 = x.Sum(y => y.RawCount),
                    RouteDir = x.GroupBy(y => y.Route).MaxBy(y => y.Sum(z => z.RawCount)).Key
                })
                .OrderByDescending(x => x.Rating2).OrderBy(x => x.Rating1)
                .Take(5)
                .Select(s => new HistoryItem { Even = (even = !even), Stop = s.Stop, Route = s.RouteDir })
                .ToList();
        }

        #region frame stack handlers
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            stateManager.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            stateManager.OnNavigatedFrom(e);
        }
        #endregion

        class HistoryItem
        {
            public bool Even;
            public StopGroup Stop;
            public Route Route;

            public string Name { get { return Stop.Name; } }
            public Brush BackgroundBrush { get { return (Brush)App.Current.Resources[Even ? "AppBackgroundBrush" : "AppSecondaryBackgroundBrush"]; } }
        }

        private void Dir1_Tap(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(TimetablePage), new TimetableParameter { Route = route.FirstRoute });
        }

        private void Dir2_Tap(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(TimetablePage), new TimetableParameter { Route = route.SecondRoute });
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ListView.SelectedItem as HistoryItem;
            if (selected != null)
            {
                ListView.SelectedItem = null;
                MainPage.NavigateToRouteStopPage(this, selected.Route, selected.Stop);
            }
        }
    }
}
