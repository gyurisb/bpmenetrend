using CityTransitApp.CityTransitElements.PageElements;
using CityTransitApp.Common.ViewModels;
using CityTransitElements.Effects;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RoutePickerDialog : Page
    {
        private RouteGroup routeGroup;
        private List<Route> routes;
        private int routeIndex = 0;
        private StopGroup stop;
        private DialogToken token;

        public RoutePickerDialog()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            Animations.OnMouseColorChange(DirBorder);
        }

        public Brush RouteBackground { get { return routeGroup.GetColors().PrimaryColorBrush; } }
        public Brush DirBackground { get { return routeGroup.GetColors().PrimaryColorBrush; } }
        public Brush BorderBackground { get { return routeGroup.GetColors().SecondaryColorBrush; } }
        public string RouteNr { get { return routeGroup.Name; } }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            token = (DialogToken)e.Parameter;

            this.routeGroup = token.OriginalRoute.RouteGroup;
            this.routes = routeGroup.Routes.ToList();
            this.stop = token.OriginalStop;
            this.routeIndex = routes.IndexOf(token.OriginalRoute);

            if (stop == null)
                this.stop = await UserEstimations.BestStopAsync(token.OriginalRoute);

            DataContext = this;
            TextName.Text = routeGroup.Description.Replace(" / ", "\n");

            setName();
            setContent();
            //Animations.FadeInFromBottomAfter(ContentListView, this, 25);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            token.ResultReady();
        }

        private void setName()
        {
            BtnDir.Text = routes[routeIndex].Name;
        }

        private void changeContent()
        {
            var list = ContentListView.ItemsSource<TimeStopListModel<StopGroup>>();
            var selectedItem = list.FirstOrDefault(x => x.Stop == stop) ?? list.First();
            int delta = Int32.Parse(selectedItem.Time);
            int n = 1, sgn = -1;

            foreach (var item in list)
            {
                item.Position = sgn * n++;
                item.Time = (Int32.Parse(item.Time) - delta).ToString();
                if (item.Stop == selectedItem.Stop)
                {
                    item.Position = 0;
                    sgn = 1;
                }
            }
        }

        private void setContent()
        {
            Route route = routes[routeIndex];
            if (stop == route.TravelRoute.Last().Stop)
                this.stop = route.TravelRoute.First().Stop;

            var list = route.TravelRoute
                .Select(e => new TimeStopListModel<StopGroup> { Time = e.Time.ToString(), Stop = e.Stop })
                .ToList();
            list.Last().Disabled = true;

            ContentListView.ItemsSource = list;
            changeContent();

            int current = list.IndexOf(list.First(x => x.Position == 0));
            int ind = Math.Max(0, current - 4);
            if (ind != 0)
                ContentListView.ForceScrollIntoView(list[ind]);
        }

        private void Item_TimeClicked(object sender, RoutedEventArgs e)
        {
            var stopTime = (sender as FrameworkElement).DataContext as TimeStopListModel<StopGroup>;
            this.stop = stopTime.Stop;
            changeContent();
        }
        private async void Item_StopClicked(object sender, RoutedEventArgs e)
        {
            var stopTime = (sender as FrameworkElement).DataContext as TimeStopListModel<StopGroup>;
            this.stop = stopTime.Stop;
            changeContent();
            await Task.Delay(150);
            Check_Clicked(this, null);
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigated += CancelNavigated;
            Frame.GoBack();
        }
        private void CancelNavigated(object sender, NavigationEventArgs e)
        {
            Frame.Navigated -= CancelNavigated;
            token.ResultReady();
        }

        private void Check_Clicked(object sender, RoutedEventArgs e)
        {
            token.Result = new RoutePickerDialogResult { Stop = stop, Route = routes[routeIndex] };
            token.ResultReady();
            Frame.GoBack();
        }

        private async void BtnDir_Tap(object sender, TappedRoutedEventArgs e)
        {
            routeIndex = (routeIndex + 1) % routes.Count;
            setName();
            ContentListView.ItemsSource = null;
            stop = await Task.Run(() => stop.OppositeOn(routes[routeIndex]));
            setContent();
        }


        public static async Task<RoutePickerDialogResult> ShowAsync(Route direction, StopGroup stopGroup = null)
        {
            var token = new DialogToken { OriginalRoute = direction, OriginalStop = stopGroup };

            var currentFrame = (Frame)Window.Current.Content;
            currentFrame.Navigate(typeof(RoutePickerDialog), token);
            await token.WorkTask;
            return token.Result;
        }

        private class DialogToken
        {
            private bool firstReady = true;
            public Route OriginalRoute;
            public StopGroup OriginalStop;
            public RoutePickerDialogResult Result;
            public Task WorkTask = new Task(() => { });
            public void ResultReady()
            {
                if (firstReady)
                {
                    firstReady = false;
                    WorkTask.Start();
                }
            }
        }
    }

    public class RoutePickerDialogResult
    {
        public Route Route;
        public StopGroup Stop;
    }
}
