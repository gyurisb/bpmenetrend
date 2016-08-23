using CityTransitApp.CityTransitElements.PageParts;
using CityTransitApp.Pages.Dialogs;
using CityTransitElements.Controllers;
using CityTransitElements.Effects;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.BusinessLogic.Helpers;
using TransitBase.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TimetablePage : Page
    {
        private static readonly Color HourBgColor = Color.FromArgb(255, 100, 100, 100);

        private TimetableViewModel ViewModel { get { return stateManager.ViewModel; } }

        public PageStateManager<TimetableViewModel> stateManager;
        public TimetablePage()
        {
            //DataContext = this;
            InitializeComponent();
            stateManager = new PageStateManager<TimetableViewModel>(this, hasCommandBar: true);
            stateManager.InitializeState += InitializePageState;
            stateManager.SaveState += SavePageState;
            stateManager.RestoreState += RestorePageState;
            //if (App.Config.Advertising)
            //{
            //    AdControl.ApplicationId = App.Config.ApplicationID;
            //    AdControl.AdUnitId = App.Config.TimetableAdUnitID;
            //    AdControl.Visibility = Visibility.Visible;
            //}
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

        private void RestorePageState()
        {
            TimetableBody.SetCurrentOffset(ViewModel.BodyVerticalOffset);
        }

        private void SavePageState()
        {
            ViewModel.BodyVerticalOffset = TimetableBody.GetCurrentOffset();
        }

        private async void InitializePageState(object parameter)
        {
            var param = (TimetableParameter)parameter;
            if (param.Stop == null)
            {
                await Task.Delay(100);
                StopTapped(this, null);
            }
        }

        private async void StopTapped(object sender, TappedRoutedEventArgs e)
        {
            var result = await RoutePickerDialog.ShowAsync(ViewModel.Route, ViewModel.Stop);
            if (result != null)
            {
                if (result.Stop == null)
                    Frame.GoBack();
                ViewModel.Stop = result.Stop;
                ViewModel.Route = result.Route;
                App.UB.History.AddTimetableHistory(result.Route, result.Stop, 3);
                ViewModel.SetRouteStopValue();
                ViewModel.SetBodyContentAsync();
            }
            else if (ViewModel.Stop == null)
            {
                Frame.GoBack();
            }
        }

        private async void RouteTapped(object sender, TappedRoutedEventArgs e)
        {
            await ViewModel.ReverseDirection();
        }


        private async void Pin_Clicked(object sender, RoutedEventArgs e)
        {
            bool ret = await AppTileCreater.CreateTile(ViewModel.Stop, ViewModel.Route, LayoutRoot);

            if (ret == false)
            {
                new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PinAlreadyDone")).ShowAsync();
                AppTileUpdater.UpdateTile(ViewModel.Route, ViewModel.Stop, LayoutRoot);
            }
        }

        private void Favorite_Clicked(object sender, RoutedEventArgs e)
        {
            ViewModel.ToggleFavorite();
        }

        private void TimetableBody_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            Trip trip = TimetableBody.Selected;
            DateTime time = TimetableBody.SelectedTime;
            Frame.Navigate(typeof(TripPage), new TripParameter { Trip = trip, Stop = ViewModel.Stop, DateTime = time });
        }

//        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
//        {
//#if DEBUG
//            new MessageDialog(e.ErrorCode.ToString() + ": " + e.Error.Message).ShowAsync();
//#endif
//        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("HelpTimetable")).ShowAsync();
        }

        private async void DateContainer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var flyout = new DatePickerFlyout();
            flyout.Date = ViewModel.SelectedDay;
            var selectedTime = await flyout.ShowAtAsync(DateControl);
            if (selectedTime != null)
            {
                ViewModel.SelectedDay = selectedTime.Value.LocalDateTime;
                ViewModel.SetBodyContentAsync();
            }
        }
    }
}
