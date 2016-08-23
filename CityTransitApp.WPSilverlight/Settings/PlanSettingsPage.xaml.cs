using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.Common.ViewModels.Settings;
using System.ComponentModel;
using CityTransitApp.WPSilverlight.Resources;
using TransitBase;
using System.Globalization;

namespace CityTransitApp.WPSilverlight.Settings
{
    public partial class PlanSettingsPage : PhoneApplicationPage
    {
        private static PlanSettingsPage Current;
        public PlanSettingsPage()
        {
            Current = this;
            DataContext = PlanSettingsModel.Current;
            this.InitializeComponent();
            this.Unloaded += PlanSettingsPart_Unloaded;
            if (!App.Config.BarrierFreeOptions)
                BarrierFreePanel.Visibility = Visibility.Collapsed;
            PlanSettingsModel.Current.PropertyChanged += Current_PropertyChanged;
        }

        void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WheelchairAccessibleStops" && PlanSettingsModel.WheelchairAccessibleStops)
            {
                ShowAccessibleStopsErrorMessage();
            }
        }

        void PlanSettingsPart_Unloaded(object sender, RoutedEventArgs e)
        {
            PlanSettingsModel.Current.PropertyChanged -= Current_PropertyChanged;
        }

        private async void ShowAccessibleStopsErrorMessage()
        {
            //TODO magyar szöveg
            CustomMessageBox box = new CustomMessageBox
            {
                Caption = "Figyelem",
                Content = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = AppResources.PlanSettingsDisabledStopError,
                    FontSize = 23
                },
                LeftButtonContent = "Igen",
                RightButtonContent = "Nem"
            };
            box.Dismissed += (sender, args) =>
            {
                if (args.Result == CustomMessageBoxResult.RightButton)
                {
                    PlanSettingsModel.WheelchairAccessibleStops = false;
                }
            };
            box.Show();
        }

        private void WalkSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WalkSpeedSlider != null)
            {
                double roundedValue = PlanSettingsModel.WalkSpeedTranslation.Keys.MinBy(x => Math.Abs(WalkSpeedSlider.Value - x));
                if (WalkSpeedSlider.Value != roundedValue)
                    WalkSpeedSlider.Value = roundedValue;
            }
        }
    }
}