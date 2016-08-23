using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TransitBase;
using CityTransitApp.Common.ViewModels.Settings;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class PlanSettingsPart : UserControl
    {
        private static PlanSettingsPart Current;
        public PlanSettingsPart()
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
            //fontsize=23, textwrapping
            var msg = new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanSettingsDisabledStopError"), "Figyelem");
            msg.Commands.Add(new UICommand { Label = App.Common.Services.Resources.LocalizedStringOf("SettingsYes"), Id = true });
            msg.Commands.Add(new UICommand { Label = App.Common.Services.Resources.LocalizedStringOf("SettingsNo"), Id = false });
            var res = await msg.ShowAsync();
            if ((bool)res.Id == false)
            {
                Current.WhStopBox.IsChecked = false;
                PlanSettingsModel.WheelchairAccessibleStops = false;
            }
        }

        private void WalkSpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
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
