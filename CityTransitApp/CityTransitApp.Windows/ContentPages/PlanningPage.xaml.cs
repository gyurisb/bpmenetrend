using CityTransitApp.CityTransitElements.PageParts;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Implementations;
using CityTransitElements.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CityTransitApp.ContentPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlanningPage : Page
    {
        private PageStateManager stateManager;
        private PlanMapController mapController;

        public PlanningViewModel ViewModel { get; set; }

        public PlanningPage()
        {
            this.InitializeComponent();
            stateManager = new PageStateManager(this);
            stateManager.InitializeState += InitializePage;

            double screenWidth = App.GetAppInfo().GetScreenWidth();
            if (screenWidth > 1600)
            {
                ListColumn.Width = new GridLength(screenWidth * 0.25);
                DetailsColumn.Width = new GridLength(screenWidth * 0.25);
                MapColumn.Width = new GridLength(screenWidth * 0.5);
            }
            else
            {
                ListColumn.Width = new GridLength(screenWidth * 0.4);
                DetailsColumn.Width = new GridLength(screenWidth * 0.4);
                MapColumn.Width = new GridLength(screenWidth * 0.8);
            }
        }

        async void InitializePage(object obj)
        {
            var param = (PlanningParameter)obj;
            ViewModel = new PlanningViewModel();
            ViewModel.Initialize(param);
            DataContext = ViewModel;

            await ViewModel.PlanAsync(param);
            if (ViewModel.FoundRoutes.Any())
            {
                ResultList.SelectedItem = ViewModel.FoundRoutes.First();
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
            map_PointerExited(null, null);
        }
        #endregion

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultList.SelectedItem != null)
            {
                DetailsPart.SetContent((WayModel)ResultList.SelectedItem);

                if (mapController != null)
                    mapController.Dispose();
                mapController = new PlanMapController();
                mapController.Bind(new WinMapProxy(Map), ((WayModel)ResultList.SelectedItem).Way);
            }
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
