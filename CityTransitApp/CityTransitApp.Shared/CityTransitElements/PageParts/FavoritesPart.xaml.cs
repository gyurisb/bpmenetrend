using CityTransitApp.CityTransitElements.PageElements;
using CityTransitApp.CityTransitElements.PageElements.SearchPanels;
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
using TransitBase.Entities;
using UserBase;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class FavoritesPart : UserControl
    {
        public event EventHandler<RouteGroup> HistoryItemSelected;
        public event EventHandler<TimetableParameter> FavoriteSelected;
        public event EventHandler<TimetableParameter> RecentSelected;
        public event EventHandler<StopGroup> NearStopSelected;

        private MainViewModel ViewModel { get; set; }

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
                    NearPanel.DataContext = new StopModel(nearStop);
                    //NearText.Text = nearStop.Name;
                }
                else
                    NearPanel.Visibility = Visibility.Collapsed;
            }
        }

        public FavoritesPart()
        {
            this.InitializeComponent();
            DataContext = ViewModel = new MainViewModel();
        }
        public void SetContent()
        {
            ViewModel.SetContent();
        }

        public void UpdateContent()
        {
            ViewModel.UpdateContent();
        }

        private void FavoriteListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItem != null)
            {
                var selected = (sender as ListView).SelectedItem as RouteStopModel;
                if (FavoriteSelected != null)
                    FavoriteSelected(this, new TimetableParameter { Route = selected.Route, Stop = selected.Stop });
                (sender as ListView).SelectedItem = null;
            }
        }

        private void RemoveFavorites_Click(object sender, RoutedEventArgs e)
        {
            var pair = (sender as FrameworkElement).DataContext as RouteStopModel;
            ViewModel.RemoveFavorite(pair);
            //SetContent();
        }

        private async void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            var selected = (sender as FrameworkElement).DataContext as RouteStopModel;

            bool ret = await AppTileCreater.CreateTile(selected.Stop, selected.Route, LayoutRoot);

            if (ret == false)
            {
                new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("TileAlreadyDone")).ShowAsync();
                await AppTileUpdater.UpdateTile(selected.Route, selected.Stop);
            }
        }

        private async void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryList.SelectedItem != null)
            {
                await Task.Delay(100);
                RouteGroup selected = HistoryList.SelectedItem as RouteGroup;
                if (HistoryItemSelected != null)
                    HistoryItemSelected(this, selected);
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
                if (RecentSelected != null)
                    RecentSelected(this, new TimetableParameter { Route = route, Stop = stop });

                RecentList.SelectedItem = null;
            }
        }

        private void NearPanel_Tap(object sender, TappedRoutedEventArgs e)
        {
            if (NearStopSelected != null)
                NearStopSelected(this, nearStop);
        }

        private void NearClose_Tap(object sender, TappedRoutedEventArgs e)
        {
            NearStop = null;
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

        private void Animation_Tap(object sender, TappedRoutedEventArgs e)
        {
            //Microsoft.Phone.Controls.TiltEffect e;
            //Animations.Rotate((UIElement)sender, true, 50);
        }

        public bool FavoritesEmpty { get { return ViewModel.FavoritesEmpty; } }

        private void RouteStopPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
