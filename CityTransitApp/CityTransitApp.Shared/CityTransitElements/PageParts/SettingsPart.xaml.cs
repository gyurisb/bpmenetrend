using CityTransitApp.Common;
using CityTransitApp.Common.ViewModels.Settings;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class SettingsPart : UserControl
    {
        public static SettingsPart Current;
        public SettingsPart()
        {
            Current = this;
            DataContext = SettingsModel.Current;
            this.InitializeComponent();
            if (!App.Config.BarrierFreeOptions)
                BarrierFreePanel.Visibility = Visibility.Collapsed;
#if !WINDOWS_PHONE_APP
            LocationPanel.Visibility = Visibility.Collapsed;
#endif

            trySetLocalizedSwitchText(() => Current.ToggleSwitch);
            trySetLocalizedSwitchText(() => Current.WheelchairSwitch);
            trySetLocalizedSwitchText(() => Current.LocationSwitch);
            trySetLocalizedSwitchText(() => Current.NearSearchSwitch);
        }

        private static void trySetLocalizedSwitchText(Func<ToggleSwitch> getSwitch)
        {
            if (Current == null) return;
            getSwitch().OnContent = App.Common.Services.Resources.LocalizedStringOf("SettingsOn");
            getSwitch().OffContent = App.Common.Services.Resources.LocalizedStringOf("SettingsOff");
        }

        private async void ClearHistoryBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var msg = new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("SettingsClearConfirm"));
            msg.Commands.Add(new UICommand { Label = App.Common.Services.Resources.LocalizedStringOf("SettingsYes"), Id = true });
            msg.Commands.Add(new UICommand { Label = App.Common.Services.Resources.LocalizedStringOf("SettingsNo"), Id = false });
            var res = await msg.ShowAsync();
            if ((bool)res.Id)
            {
                App.UB.History.Clear();
            }
        }
    }
}
