using CityTransitApp.CityTransitElements.PageElements.SearchPanels;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CityTransitApp.ContentPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListPage : Page
    {
        public ListPageViewModel ViewModel { get; private set; }

        private PageStateManager stateManager;
        public ListPage()
        {
            this.InitializeComponent();
            this.stateManager = new PageStateManager(this);
            stateManager.InitializeState += InitializePage;
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

        private void InitializePage(object param)
        {
            string text = (string)param;
            this.DataContext = ViewModel = new ListPageViewModel();
            ViewModel.SetContent(text, true);
        }


        public void SetText(string text)
        {
            ViewModel.SetContent(text, true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public class ListPageViewModel : SearchViewModel
        {
            public ListPageViewModel()
            {
                this.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "ResultItems")
                    {
                        base.OnPropertyChanged("RouteItems");
                        base.OnPropertyChanged("StopItems");
                    }
                    else if (args.PropertyName == "ResultCounterData")
                    {
                        base.OnPropertyChanged("HasRoute");
                        base.OnPropertyChanged("HasStop");
                    }
                };
            }

            public IList RouteItems { get { return ResultItems != null ? ResultItems.Cast<object>().Where(x => x is RouteGroup).ToList() : null; } }
            public IList StopItems { get { return ResultItems != null ?ResultItems.Cast<object>().Where(x => x is StopModel).ToList() : null; } }

            public bool HasRoute { get { return ResultItems != null && ResultItems.Count - ResultCounterData.StopCount > 0; } }
            public bool HasStop { get { return ResultItems != null && ResultCounterData.StopCount > 0; } }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void StopList_SelectionChanged(object sender, TappedRoutedEventArgs e)
        {
            var selected = (StopModel)((FrameworkElement)sender).DataContext;
            Frame.Navigate(typeof(StopPage), new StopParameter
            {
                StopGroup = selected.Stop
            });
        }

        private void RouteList_SelectionChanged(object sender, EventArgs e)
        {
            //var selected = (RouteGroup)((FrameworkElement)sender).DataContext;
            var selected = (RouteGroup)sender;
            Frame.Navigate(typeof(RoutePage), new TimetableParameter
            {
                Route = selected.FirstRoute,
                //Stop = selected.FirstRoute.TravelRoute.First().Stop
            });
        }

        private void RoutePanel_RouteClick(object sender, EventArgs e)
        {
            var selected = (Route)sender;
            Frame.Navigate(typeof(RoutePage), new TimetableParameter
            {
                Route = selected
            });
        }
    }
}
