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
using CityTransitApp.WPSilverlight.Resources;

namespace CityTransitApp.WPSilverlight.Settings
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public static SettingsPage Current;
        public SettingsPage()
        {
            Current = this;
            DataContext = SettingsModel.Current;
            this.InitializeComponent();
            if (!App.Config.BarrierFreeOptions)
                BarrierFreePanel.Visibility = Visibility.Collapsed;

            foreach (var binding in bindings.Values)
            {
                binding.UpdateBinding();
            }
        }

        private static Dictionary<string, ModellSwitchBinding> bindings = new Dictionary<string, ModellSwitchBinding>
        {
            { "AutomaticUpdateCheck", new ModellSwitchBinding(() => SettingsModel.AutomaticUpdateCheck, () => Current.ToggleSwitch) },
            { "WheelchairUnderlined", new ModellSwitchBinding(() => SettingsModel.WheelchairUnderlined, () => Current.WheelchairSwitch) },
            { "LocationServices", new ModellSwitchBinding(() => SettingsModel.LocationServices, () => Current.LocationSwitch) },
            { "AutomaticNearSearch", new ModellSwitchBinding(() => SettingsModel.AutomaticNearSearch, () => Current.NearSearchSwitch) },
        };

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SettingsModel.Current.PropertyChanged += Current_PropertyChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SettingsModel.Current.PropertyChanged -= Current_PropertyChanged;
        }

        void Current_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bindings[e.PropertyName].UpdateBinding();
        }

        private async void ClearHistoryBtn_Clicked(object sender, RoutedEventArgs e)
        {

            CustomMessageBox cmb = new CustomMessageBox
            {
                Message = AppResources.SettingsClearConfirm,
                LeftButtonContent = AppResources.SettingsYes,
                RightButtonContent = AppResources.SettingsNo
            };
            cmb.Dismissed += (sender0, args) =>
            {
                if (args.Result == CustomMessageBoxResult.LeftButton)
                    App.UB.History.Clear();
            };
            cmb.Show();
        }

        private class ModellSwitchBinding
        {
            public Func<bool> GetModelValue;
            public Func<ToggleSwitch> GetSwitch;

            public ModellSwitchBinding(Func<bool> getModelValue, Func<ToggleSwitch> getSwitch)
            {
                GetModelValue = getModelValue;
                GetSwitch = getSwitch;
            }

            public void UpdateBinding()
            {
                if (Current == null) return;
                GetSwitch().Content = GetModelValue() ? AppResources.SettingsOn : AppResources.SettingsOff;
            }
        }
    }
}