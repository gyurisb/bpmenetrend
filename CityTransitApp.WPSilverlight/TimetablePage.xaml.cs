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
using CityTransitServices.Tools;
using CityTransitApp.WPSilverlight.Effects;
using System.Threading.Tasks;
using System.Windows.Media;
using CityTransitApp.WPSilverlight.Resources;
using CityTransitApp.WPSilverlight.Tools;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.WPSilverlight.Dialogs;

namespace CityTransitApp.WPSilverlight
{
    public partial class TimetablePage : PhoneApplicationPage
    {
        private TimetableViewModel ViewModel { get; set; }

        public static TimetablePage Current;
        private static readonly Color HourBgColor = Color.FromArgb(255, 100, 100, 100);
        private PeriodicTask contentSetterTask;

        private ApplicationBarIconButton favoriteMenuIcon
        {
            get { return ApplicationBar.Buttons[0] as ApplicationBarIconButton; }
        }

        public TimetablePage()
        {
            Current = this;
            DataContext = this;
            InitializeComponent();
            BuildLocalizedApplicationBar();
            Animations.OnMouseColorChange(NrBorder);
            Animations.OnMouseColorChange(StopBorder);
        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                string stopID = null, routeID = null, time = null;
                NavigationContext.QueryString.TryGetValue("stopID", out stopID);
                NavigationContext.QueryString.TryGetValue("routeID", out routeID);
                NavigationContext.QueryString.TryGetValue("selectedTime", out time);
                StopGroup stop = null;
                DateTime? selectedTime = null;
                Route route;

                route = App.Model.GetRouteByID(int.Parse(routeID));
                await Task.Delay(50);
                if (stopID != null)
                    stop = App.Model.GetStopGroupByID(int.Parse(stopID));
                if (time != null)
                    selectedTime = Convert.ToDateTime(time);

                this.ViewModel = new TimetableViewModel();
                this.DataContext = ViewModel;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.Initialize(new TimetableParameter { Route = route, Stop = stop, SelectedTime = selectedTime });
                if (ViewModel.TasksToSchedule.Any())
                {
                    contentSetterTask = new PeriodicTask(ViewModel.TasksToSchedule.Single());
                    contentSetterTask.RunEveryMinute();
                }

                if (ViewModel.Stop == null)
                    StopTapped(this, null);

            }
            else
            {
                if (contentSetterTask != null)
                    contentSetterTask.Resume();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (contentSetterTask != null)
                contentSetterTask.Cancel();
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FavoriteIcon")
            {
                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IconUri = (Uri)ViewModel.FavoriteIcon;
            }
        }

        private async void StopTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var arg = await RoutePickerDialog.ShowAsync(ViewModel.Route, ViewModel.Stop);
            if (arg != null)
            {
                ViewModel.Stop = arg.Stop;
                ViewModel.Route = arg.Route;
                if (ViewModel.Stop == null)
                    NavigationService.GoBack();
                App.UB.History.AddTimetableHistory(ViewModel.Route, ViewModel.Stop, 3);
                ViewModel.SetRouteStopValue();
                ViewModel.SetBodyContentAsync();
            }
            else if (ViewModel.Stop == null)
            {
                NavigationService.GoBack();
            }
        }

        private async void RouteTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            await ViewModel.ReverseDirection();
        }

        private void Pin_Clicked(object sender, EventArgs e)
        {
            bool ret = TilesApp.CreateTile(ViewModel.Stop, ViewModel.Route);

            if (ret == false)
            {
                MessageBox.Show("A kijelölt elemről már létrehozott egy csempét!");
                Tiles.UpdateTile(ViewModel.Stop, ViewModel.Route);
            }
        }

        private void Favorite_Clicked(object sender, EventArgs e)
        {
            ViewModel.ToggleFavorite();
        }

        private void DateControl_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedDay = e.NewDateTime.Value;
                if (ViewModel.Stop != null)
                    ViewModel.SetBodyContentAsync();
            }
        }

        private void TimetableBody_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            Trip trip = TimetableBody.Selected;
            DateTime time = TimetableBody.SelectedTime;
            NavigationService.Navigate(new Uri("/TripPage.xaml?tripID=" + trip.ID + "&routeID=" + trip.Route.ID + "&stopID=" + ViewModel.Stop.ID + "&dateTime=" + time.ToString(), UriKind.Relative));
        }

        private void BuildLocalizedApplicationBar()
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.TimetableMenuAddFavs;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppResources.TimetableMenuPin;
            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Text = AppResources.HelpLabel;
        }

        private void HelpMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(AppResources.HelpTimetable);
        }
    }
}