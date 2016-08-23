using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TransitBase.Entities;
using System.Windows.Media;
using TransitBase;
using CityTransitApp.WPSilverlight.Effects;
using System.Threading.Tasks;

namespace CityTransitApp.WPSilverlight
{
    public partial class HistoryItemPage : PhoneApplicationPage
    {//TODO legnépszerűbb két megálló megjelenítése legfelül
        private RouteGroup route;

        public HistoryItemPage()
        {
            InitializeComponent();
            Animations.OnMouseColorChange(Dir1Border);
            Animations.OnMouseColorChange(Dir2Border);
        }

        public string RouteNumber { get { return route.Name; } }
        public Brush HeaderColor { get { return route.GetColors().PrimaryColorBrush; } }
        public Brush RouteColor { get { return route.GetColors().BgColorBrush; } }
        public Brush MainColor { get { return route.GetColors().MainColorBrush; } }
        public Brush FontColor { get { return route.GetColors().FontColorBrush; } }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                string param = "";

                if (!NavigationContext.QueryString.TryGetValue("id", out param))
                    throw new Exception("HistoryItemPage opened without parameter");

                int id = System.Convert.ToInt32(param);
                route = App.Model.GetRouteGroupByID(id);
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
                        Rating1 = x.Min(y => UserBase.Interface.HistoryHelpers.DayPartDistance(y)),
                        Rating2 = x.Sum(y => y.RawCount),
                        RouteDir = x.GroupBy(y => y.Route).MaxBy(y => y.Sum(z => z.RawCount)).Key
                    })
                    .OrderByDescending(x => x.Rating2).OrderBy(x => x.Rating1)
                    .Take(5)
                    .Select(s => new HistoryItem { Even = (even = !even), Stop = s.Stop, Route = s.RouteDir })
                    .ToList();
            }
        }

        class HistoryItem
        {
            public bool Even;
            public StopGroup Stop;
            public Route Route;

            public string Name { get { return Stop.Name; } }
            public Brush BackgroundBrush { get { return (Brush)App.Current.Resources[Even ? "AppBackgroundBrush" : "AppSecondaryBackgroundBrush"]; } }
        }

        private void Dir1_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TimetablePage.xaml?routeID=" + route.FirstRoute.ID, UriKind.Relative));
        }

        private void Dir2_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/TimetablePage.xaml?routeID=" + route.SecondRoute.ID, UriKind.Relative));
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