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
using CityTransitApp.Common.ViewModels;
using System.Collections.ObjectModel;
using UserBase.Interface;
using CityTransitApp.WPSilverlight.Resources;
using System.Threading.Tasks;
using CityTransitServices.Tools;
using TransitBase;
using CityTransitApp.WPSilverlight.Tools;

namespace CityTransitApp.WPSilverlight.PageParts
{
    public partial class FavoritesTab : UserControl
    {
        public bool FavoriteEmpty = true;

        private StopGroup nearStop = null;
        public StopGroup NearStop
        {
            get { return nearStop; }
            set
            {
                nearStop = value;
                if (nearStop != null)
                {
                    NearPanel.Visibility = Visibility.Visible;
                    NearPanel.DataContext = new StopModel(nearStop, fill: false);
                    //NearText.Text = nearStop.Name;
                }
                else
                    NearPanel.Visibility = Visibility.Collapsed;
            }
        }

        public FavoritesTab()
        {
            InitializeComponent();
            FavoriteList.ItemsSource = new ObservableCollection<RouteStopModel>();
        }

        public void SetContent()
        {

            var favoriteList = App.UB.Favorites
                .OrderBy(fav => fav.Position ?? -1)
                .Select(x => new RouteStopModel { Route = x.Route, Stop = x.Stop, Padding = 2 })
                .ToList();
            if (favoriteList.Count > 0)
            {
                foreach (var item in favoriteList)
                    item.UpdateNextTrips();

                FavoriteEmpty = false;
                FavoriteList.ItemsSource.Clear();
                FavoriteList.ItemsSource.AddObjectRange(favoriteList);
                FavoriteList.Height = double.NaN;
                NoFavoritesText.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoFavoritesText.Visibility = Visibility.Visible;
                FavoriteList.Height = 0;
            }

            int rowCount = (int)Math.Floor(Application.Current.Host.Content.ActualWidth / 100.0);
            var history = App.UB.History.TimetableEntries;
            if (history.Count() > 0)
            {
                HistoryList.Height = double.NaN;
                HistoryList.ItemsSource = history
                    .GroupBy(p => p.Route.RouteGroup)
                    .Select(x => new { RouteGroup = x.Key, Rating1 = x.Min(y => HistoryHelpers.DayPartDistance(y)), Rating2 = x.Sum(y => y.RawCount) })
                    .OrderByDescending(t => t.Rating2).OrderBy(t => t.Rating1)
                    .Select(t0 => t0.RouteGroup)
                    .Take(rowCount * 4)
                    .ToList();
            }
            else HistoryList.Height = 0;

            var recent = App.UB.History.GetRecents().ToList();
            if (recent.Count > 0)
            {
                RecentList.Height = double.NaN;
                RecentLabel.Visibility = Visibility.Visible;
                RecentList.ItemsSource = recent
                    .Select(x => x.Route.RouteGroup)
                    .Distinct()
                    .Take(rowCount)
                    .ToList();
            }
            else
            {
                RecentList.Height = 0;
                RecentLabel.Visibility = Visibility.Collapsed;
            }
        }

        public void UpdateContent()
        {
            if (FavoriteList.ItemsSource == null) return;
            foreach (var item in FavoriteList.ItemsSource.Cast<RouteStopModel>())
            {
                item.UpdateNextTrips();
            }
        }

        private void FavoriteListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as LongListSelector).SelectedItem != null)
            {
                var selected = (sender as LongListSelector).SelectedItem as RouteStopModel;
                MainPage.NavigateToRouteStopPage(MainPage.Current, selected.Route, selected.Stop);
                (sender as LongListSelector).SelectedItem = null;
            }
        }

        private void RemoveFavorites_Click(object sender, RoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            App.UB.Favorites.Remove(pair.Route, pair.Stop);
            FavoriteList.ItemsSource.Remove(pair);
            //SetContent();
        }
        private void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            var selected = (sender as FrameworkElement).DataContext as RouteStopModel;

            bool ret = TilesApp.CreateTile(selected.Stop, selected.Route);

            if (ret == false)
            {
                MessageBox.Show(AppResources.TileAlreadyDone);
                Tiles.UpdateTile(selected.Stop, selected.Route);
            }
        }

        private async void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryList.SelectedItem != null)
            {
                await Task.Delay(100);
                RouteGroup selected = HistoryList.SelectedItem as RouteGroup;
                MainPage.Current.NavigationService.Navigate(new Uri("/HistoryItemPage.xaml?id=" + selected.ID, UriKind.Relative));
                HistoryList.SelectedItem = null;
            }
        }

        private async void RecentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecentList.SelectedItem != null)
            {
                await Task.Delay(100);
                RouteGroup selected = RecentList.SelectedItem as RouteGroup;
                Route route = UserEstimations.BestRoute(selected);
                StopGroup stop = UserEstimations.BestStop(route);
                MainPage.Current.NavigationService.Navigate(new Uri("/TimeTablePage.xaml?stopID=" + stop.ID + "&routeID=" + route.ID, UriKind.Relative));
                //MainPage.NavigateToRouteStop(MainPage.Current, route, stop);
                RecentList.SelectedItem = null;
            }
        }

        private void NearPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MainPage.Current.NavigationService.Navigate(new Uri("/StopPage.xaml?id=" + nearStop.ID + "&location=near", UriKind.Relative));
        }

        private void NearClose_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NearStop = null;
        }

        private void FavUp_Click(object sender, RoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            int index = FavoriteList.ItemsSource.IndexOf(pair);
            if (index > 0)
            {
                App.UB.Favorites.PushBack(pair.Route, pair.Stop);
                FavoriteList.ItemsSource.RemoveAt(index);
                FavoriteList.ItemsSource.Insert(index - 1, pair);
            }
        }

        private void FavDown_Click(object sender, RoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            int index = FavoriteList.ItemsSource.IndexOf(pair);
            if (index < FavoriteList.ItemsSource.Count - 1)
            {
                App.UB.Favorites.PushForward(pair.Route, pair.Stop);
                FavoriteList.ItemsSource.RemoveAt(index);
                FavoriteList.ItemsSource.Insert(index + 1, pair);
            }
        }

        private void Animation_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //Microsoft.Phone.Controls.TiltEffect e;
            //Animations.Rotate((UIElement)sender, true, 50);
        }
    }

    public class FavoritesModel
    {
        public StopModel NearStop { get; set; }
        public ObservableCollection<IRouteStopPair> Favorites { get; set; }
        public ObservableCollection<IRouteStopPair> History { get; set; }
    }
}
