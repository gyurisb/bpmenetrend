using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using TransitBase.Entities;
using System.Windows.Media;
using CityTransitApp.WPSilverlight.Effects;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using Microsoft.Phone.Controls;

namespace CityTransitApp.WPSilverlight.Dialogs
{
    public partial class RoutePickerDialog : PhoneApplicationPage
    {
        private RouteGroup routeGroup;
        private List<Route> routes;
        private int routeIndex = 0;
        private StopGroup stop;
        private DialogToken token;

        public RoutePickerDialog()
        {
            InitializeComponent();
            Animations.OnMouseColorChange(DirBorder);
        }

        public Brush RouteBackground { get { return routeGroup.GetColors().PrimaryColorBrush; } }
        public Brush DirBackground { get { return routeGroup.GetColors().PrimaryColorBrush; } }
        public Brush BorderBackground { get { return routeGroup.GetColors().SecondaryColorBrush; } }
        public string RouteNr { get { return routeGroup.Name; } }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            token = DialogToken.Current;

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

        private async void setContent()
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

            await Task.Delay(100);

            int current = list.IndexOf(list.First(x => x.Position == 0));
            int ind = Math.Max(0, current - 4);
            if (ind != 0)
                ContentListView.ScrollTo(list[ind]);
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

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            NavigationService.Navigated += CancelNavigated;
            NavigationService.GoBack();
        }
        private void CancelNavigated(object sender, NavigationEventArgs e)
        {
            NavigationService.Navigated -= CancelNavigated;
            token.ResultReady();
        }

        private void Check_Clicked(object sender, EventArgs e)
        {
            token.Result = new RoutePickerDialogResult { Stop = stop, Route = routes[routeIndex] };
            token.ResultReady();
            NavigationService.GoBack();
        }

        private async void BtnDir_Tap(object sender, System.Windows.Input.GestureEventArgs e)
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

            var currentPage = (PhoneApplicationPage)App.RootFrame.Content;
            currentPage.NavigationService.Navigate(new Uri("/Dialogs/RoutePickerDialog.xaml", UriKind.Relative));
            await token.WorkTask;
            return token.Result;
        }

        private class DialogToken
        {
            public static DialogToken Current { get; private set; }
            public DialogToken() { Current = this; }

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