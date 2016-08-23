using CityTransitApp.CityTransitElements.PageElements.SearchPanels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.Entities;
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
using UserBase.BusinessLogic;
using UserBase.Interface;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        //private ResultCounter[] ResultCounters { get { return new ResultCounter[] { ResultCounter }; } }
        //private ResultCounter[] SearchResults { get { return new ResultCounter[] { SearchResult1, SearchResult2, SearchResult3 }; } }

        private bool HasText { get { return SearchText.Text.Length > 0; } }
        //private bool HasContent { get { return SearchList.Visibility == Visibility.Visible || StopList.Visibility == Visibility.Visible || RouteStopList.Visibility == Visibility.Visible; } }
        private bool HasContent { get { return SearchList.Visibility == Visibility.Visible; } }

        //keresési találatoknál a legnépszerűbb legfelül
        public PageStateManager stateManager;
        public SearchPage()
        {
            InitializeComponent();
            //DataContext = this;
            CategorySelector.TreeModel = App.Config.CategoryTree;
            stateManager = new PageStateManager(this);
            stateManager.InitializeState += InitializePageState;
            //StopList.ItemsSource = new ObservableCollection<StopGroup>();
            //registerForBottomScroll(ScrollViewer_BottomScroll);
        }

        #region frame stack handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageStateManager.SetStatusBarColor(this, ((SolidColorBrush)App.Current.Resources["AppSecondaryBackgroundBrush"]).Color);
            stateManager.OnNavigatedTo(e, false);
            App.RegisterBackKeyHandler(this, BackKeyPressed);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            stateManager.OnNavigatedFrom(e);
            App.UnregisterBackKeyHandler(this);
        }
        #endregion

        protected async void InitializePageState(object parameter)
        {
            await Task.Delay(200);
            SearchText.Focus(FocusState.Pointer);
            await SetContent(full: true);
        }


        private void BackKeyPressed(App.BackKeyHandlerToken e)
        {
            if (HasContent)
            {
                e.Handled = true;
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

        private async void BtnSearch_Click(object sender, TappedRoutedEventArgs e)
        {
            await SetContent(full: true);
        }
        private void BtnClear_Click(object sender, TappedRoutedEventArgs e)
        {
            SearchText.Text = "";
        }

        private void ClearContent()
        {
            CategorySelector.Visibility = Visibility.Collapsed;
            HistoryList.Visibility = Visibility.Collapsed;
            SearchList.Visibility = Visibility.Collapsed;
        }

        public async Task SetContent(bool full)
        {
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
                SearchList.ResetSearchResult();
            }
            else
            {
                ClearContent();
                await SearchList.SetContent(SearchText.Text, full);
                SearchList.Visibility = Visibility.Visible;
            }
        }

        private void CategorySelector_Selected(object sender, IEnumerable<RouteGroup> selection)
        {
            CategorySelector.Visibility = Visibility.Collapsed;
            HistoryList.Visibility = Visibility.Collapsed;
            SearchList.Visibility = Visibility.Visible;

            SearchList.SetContent(selection);
        }


        private void SearchText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Focus(Windows.UI.Xaml.FocusState.Pointer);
            }
        }

        #region Search list selected event handlers
        private void SearchList_StopSelected(object sender, StopGroup stop)
        {
            Frame.Navigate(typeof(StopPage), new StopParameter { StopGroup = stop });
        }
        private void SearchList_RouteGroupSelected(object sender, RouteGroup selected)
        {
            Frame.Navigate(typeof(TimetablePage), new TimetableParameter { Route = UserEstimations.BestRoute(selected) });
        }
        private void SearchList_RouteSelected(object sender, Route selected)
        {
            Frame.Navigate(typeof(TimetablePage), new TimetableParameter { Route = selected });
        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryList.SelectedItem != null)
            {
                var selected = HistoryList.SelectedItem as StopModel;
                HistoryList.SelectedItem = null;
                Frame.Navigate(typeof(StopPage), new StopParameter { StopGroup = selected.Stop });
            }
        }
        #endregion

        private async void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchList.ClearCategorySelection();
            TextDefult.Visibility = HasText ? Visibility.Collapsed : Visibility.Visible;
            BtnClear.Visibility = HasText ? Visibility.Visible : Visibility.Collapsed;
            await SetContent(full: false);
        }

        private void TextDefult_Tap(object sender, TappedRoutedEventArgs e)
        {
            SearchText.Focus(Windows.UI.Xaml.FocusState.Pointer);
        }

        private async void SearchText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchText.Text.Length > 0 && !SearchList.ClearCategorySelection())
                await SetContent(full: true);
        }

        //#region fields
        //public event PropertyChangedEventHandler PropertyChanged;
        //private FrameworkElement ScrollPanel;
        //private ScrollViewer ScrollViewer;
        //#endregion
        //#region Pageing helpers
        //IList<RouteGroup> remRoutes;
        //IList<StopModel> remStops;
        //private readonly int PageLimit = 20;
        //private void AddRoutes(IList<RouteGroup> routes, bool recursive = false)
        //{
        //    if (!recursive) //SearchList.ItemsSource.Clear();
        //    {
        //        SearchList.ItemsSource = new ObservableCollection<RouteGroup>();
        //        RouteFooterVisibility = Visibility.Visible;
        //    }
        //    foreach (var route in routes.Take(PageLimit))
        //        SearchList.ItemsSource.Add(route);
        //    remRoutes = routes.Skip(PageLimit).ToList();
        //    if (remRoutes.Count == 0)
        //    {
        //        remRoutes = null;
        //        RouteFooterVisibility = Visibility.Collapsed;
        //    }
        //}
        //private void AddStops(IList<StopModel> stops, bool recursive = false)
        //{
        //    if (!recursive) //StopList.ItemsSource.Clear();
        //    {
        //        StopList.ItemsSource = new ObservableCollection<StopModel>();
        //        StopFooterVisibility = Visibility.Visible;
        //    }
        //    foreach (var stop in stops.Take(PageLimit))
        //        StopList.ItemsSource.Add(stop);
        //    remStops = stops.Skip(PageLimit).ToList();
        //    if (remStops.Count == 0)
        //    {
        //        remStops = null;
        //        StopFooterVisibility = Visibility.Collapsed;
        //    }
        //}
        //void ScrollViewer_BottomScroll()
        //{
        //    if (remRoutes != null)
        //        AddRoutes(remRoutes, true);
        //    else if (remStops != null)
        //        AddStops(remStops, true);
        //}
        //#endregion
        //#region Bottom scroll helpers
        //private bool atBottom = false;
        //private void registerForBottomScroll(Action action)
        //{
        //    registerForNotification("VerticalOffset", ScrollViewer, (sender, args) =>
        //    {
        //        if (ScrollViewer.VerticalOffset >= ScrollPanel.ActualHeight - ScrollViewer.ActualHeight && ScrollPanel.ActualHeight > ScrollViewer.ActualHeight)
        //        {
        //            if (!atBottom)
        //            {
        //                atBottom = true;
        //                action();
        //            }
        //        }
        //        else atBottom = false;
        //    });
        //}

        //public static void registerForNotification(string propertyName, FrameworkElement element, PropertyChangedCallback callback)
        //{
        //    //Bind to a depedency property  
        //    Binding b = new Binding(propertyName) { Source = element };
        //    var prop = System.Windows.DependencyProperty.RegisterAttached(
        //        "ListenAttached" + propertyName,
        //        typeof(object),
        //        typeof(UserControl),
        //        new System.Windows.PropertyMetadata(callback));
        //    element.SetBinding(prop, b);
        //}
        //#endregion
        //#region FooterBinding
        //public void footerChanged()
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs("StopFooterVisibility"));
        //        PropertyChanged(this, new PropertyChangedEventArgs("RouteFooterVisibility"));
        //    }
        //}
        //private Visibility stopFooterVisibility;
        //private Visibility routeFooterVisibility;
        //public Visibility StopFooterVisibility
        //{
        //    get { return stopFooterVisibility; }
        //    set
        //    {
        //        stopFooterVisibility = value;
        //        footerChanged();
        //    }
        //}
        //public Visibility RouteFooterVisibility
        //{
        //    get { return routeFooterVisibility; }
        //    set
        //    {
        //        routeFooterVisibility = value;
        //        footerChanged();
        //    }
        //}
        //#endregion
    }
}
