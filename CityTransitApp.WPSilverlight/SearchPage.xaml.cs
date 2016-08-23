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
using CityTransitApp.WPSilverlight.PageElements.SearchElements;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CityTransitApp.Common.ViewModels;
using TransitBase;
using UserBase.Interface;
using System.Windows.Input;
using CityTransitServices.Tools;
using CityTransitApp.WPSilverlight.Tools;

namespace CityTransitApp.WPSilverlight
{
    public partial class SearchPage : PhoneApplicationPage
    {
        private ResultCounter[] SearchResults { get { return new ResultCounter[] { SearchResult1, SearchResult2, SearchResult3 }; } }

        private bool hadSearch;
        private bool HasText { get { return SearchText.Text.Length > 0; } }
        private bool HasContent { get { return RouteList.Visibility == Visibility.Visible || StopList.Visibility == Visibility.Visible || RouteStopList.Visibility == Visibility.Visible; } }

        //keresési találatoknál a legnépszerűbb legfelül
        public SearchPage()
        {
            InitializeComponent();
            DataContext = this;
            CategorySelector.TreeModel = App.Config.CategoryTree;
            //RouteList.ItemsSource = new ObservableCollection<RouteGroup>();
            //StopList.ItemsSource = new ObservableCollection<StopGroup>();
            //registerForBottomScroll(ScrollViewer_BottomScroll);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                await Task.Delay(200);
                SearchText.Focus();
                await SetContent(full: true);
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (HasContent)
            {
                e.Cancel = true;
                if (HasText)
                {
                    SearchText.Text = "";
                }
                else
                {
                    BtnSearch_Click(null, null);
                }
            }
        }

        private async void BtnSearch_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            await SetContent(full: true);
        }
        private void BtnClear_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SearchText.Text = "";
        }

        private void ClearContent()
        {
            CategorySelector.Visibility = Visibility.Collapsed;
            HistoryList.Visibility = Visibility.Collapsed;
            RouteList.Visibility = Visibility.Collapsed;
            StopList.Visibility = Visibility.Collapsed;
            RouteStopList.Visibility = Visibility.Collapsed;
        }

        public async Task SetContent(bool full, Func<object, bool> searchFilter = null)
        {
            searchFilter = searchFilter ?? (o => true);

            if (SearchText.Text.Length == 0)
            {
                var history = await Task.Run(() =>
                    App.UB.History.StopEntries
                        .GroupBy(p => p.Stop)
                        .Select(x => Tuple.Create(x.Key, x.Min(e => HistoryHelpers.DayPartDistance(e)), x.Sum(p => p.RawCount)))
                        .OrderByDescending(t => t.Item3)
                        .OrderBy(t => t.Item2)
                    );
                ClearContent();
                CategorySelector.Visibility = Visibility.Visible;
                HistoryList.Visibility = Visibility.Visible;
                HistoryList.ItemsSource = history.Select(t => new StopModel(t.Item1)).Take(8).ToArray();
                ResetSearchResult();
            }
            else
            {
                IEnumerable<StopModel> stops = new StopModel[0];
                IEnumerable<RouteGroup> routes = new RouteGroup[0];
                string searchText = SearchText.Text;

                if (searchText.Length > 0)
                {
                    routes = await Task.Run(() =>
                        App.Model.FindRoutes(searchText)
                        .OrderByText(r => r.Name)
                        .ToList()
                    );
                }

                if (searchText.Length >= 3)
                {
                    //elvileg nem teljesen pontos, mert lehetnek azonos route-ok a szummában de közelítésnek megteszi
                    //+ A megálló végállomásként is számít
                    stops = await Task.Run(() =>
                        App.Model
                        .FindStops(searchText)
                        .Select(s => new StopModel(s, true, true))
                        .OrderByText(s => s.Name)
                        .OrderByDescending(s => s.RouteCount)
                        .OrderBy(s => s.HighestPriority)
                        .ToList()
                    );
                }

                ClearContent();
                if (stops.Take(1).Count() == 0)
                {
                    RouteList.Visibility = Visibility.Visible;
                    RouteList.ItemsSource = full ? routes.Where(o => searchFilter(o)).ToList() : routes.Take(5).ToList();
                }
                else if (routes.Take(1).Count() == 0)
                {
                    var stops1 = full ? stops.ToList() : stops.Take(5).ToList();
                    StopList.Visibility = Visibility.Visible;
                    StopList.ItemsSource = stops1.Where(searchFilter).ToList();
                }
                else
                {
                    var firstRoutes = routes.Where(r => r.Name.Normalize().Contains(searchText.Normalize())).Cast<object>().ToList();
                    var lastRoutes = routes.Cast<object>().Except(firstRoutes);
                    var routesAndStops = firstRoutes.Concat(stops).Concat(lastRoutes).ToList();
                    RouteStopList.Visibility = Visibility.Visible;
                    RouteStopList.ItemsSource = full ? routesAndStops.Where(searchFilter).ToList() : routesAndStops.Take(5).ToList();
                }
                SetSearchResult(stops.Select(s => s.Stop), routes);
            }
        }

        private void SetSearchResult(IEnumerable<StopGroup> stops, IEnumerable<RouteGroup> routes)
        {
            var model = new SearchResultModel(stops, routes);
            foreach (var searchResult in SearchResults)
                searchResult.ResultModel = model;
        }

        private void ResetSearchResult()
        {
            foreach (var searchResult in SearchResults)
                searchResult.ResultModel = SearchResultModel.Empty;
        }

        async void SearchResult_ResultCategorySelected(object sender, SearchResultCategory e)
        {
            await Task.Delay(150);
            Func<object, bool> searchFilter = null;
            if (e.Type == SearchResultCategoryType.Stop)
            {
                RouteList.Visibility = Visibility.Collapsed;
                searchFilter = (o => o is StopModel);
            }
            else
            {
                StopList.Visibility = Visibility.Collapsed;
                searchFilter = (o => o is RouteGroup && e.RouteTypes.Contains((o as RouteGroup).Type));
            }
            await SetContent(true, searchFilter);
            hadSearch = false;
            ResetSearchResult();
        }

        private void CategorySelector_Selected(object sender, IEnumerable<RouteGroup> selection)
        {
            CategorySelector.Visibility = Visibility.Collapsed;
            HistoryList.Visibility = Visibility.Collapsed;
            RouteList.Visibility = Visibility.Visible;
            StopList.Visibility = Visibility.Collapsed;

            RouteList.ItemsSource = selection.OrderByText(r => r.Name).ToList();
        }

        private void StopListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as LongListSelector).SelectedItem != null)
            {
                var selected = (sender as LongListSelector).SelectedItem as StopModel;
                if (selected == null) return;
                MainPage.Current.NavigationService.Navigate(new Uri("/StopPage.xaml?id=" + selected.Stop.ID, UriKind.Relative));
                (sender as LongListSelector).SelectedItem = null;
            }
        }

        private void SearchText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Focus();
            }
        }

        private void RouteGroupClicked(object sender, EventArgs e)
        {
            RouteGroup selected = sender as RouteGroup;
            MainPage.Current.NavigationService.Navigate(new Uri("/TimetablePage.xaml?routeID=" + UserEstimations.BestRoute(selected).ID, UriKind.Relative));
        }
        private void routeClicked(object sender, EventArgs e)
        {
            Route selected = sender as Route;
            if (sender == null) return;
            MainPage.Current.NavigationService.Navigate(new Uri("/TimetablePage.xaml?routeID=" + selected.ID, UriKind.Relative));
        }

        private async void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            hadSearch = true;
            TextDefult.Visibility = HasText ? Visibility.Collapsed : Visibility.Visible;
            BtnClear.Visibility = HasText ? Visibility.Visible : Visibility.Collapsed;
            await SetContent(full: false);
        }

        private void TextDefult_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SearchText.Focus();
        }

        private async void SearchText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchText.Text.Length > 0 && hadSearch)
                await SetContent(full: true);
        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryList.SelectedItem != null)
            {
                var selected = HistoryList.SelectedItem as StopModel;
                HistoryList.SelectedItem = null;
                MainPage.Current.NavigationService.Navigate(new Uri("/StopPage.xaml?id=" + selected.Stop.ID, UriKind.Relative));
            }
        }
    }

    public class RouteStopTemplateSelector : TemplateSelector
    {
        public DataTemplate RouteTemplate { get; set; }
        public DataTemplate StopTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, int index, int totalCount, DependencyObject container)
        {
            if (item is RouteGroup)
                return RouteTemplate;
            else if (item is StopModel)
                return StopTemplate;
            else throw new InvalidOperationException();
        }
    }
}