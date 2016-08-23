using CityTransitElements.Effects;
using CityTransitApp.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageElements
{
    public sealed partial class TimeStopListItem : UserControl
    {
        public TimeStopListItem()
        {
            InitializeComponent();
            TimeText.SetValue(RotateEffect.IsEnabledProperty, true);
            StopText.SetValue(RotateEffect.IsEnabledProperty, true);
        }

        public event EventHandler<RoutedEventArgs> TimeClick;
        public event EventHandler<RoutedEventArgs> StopClick;

        private void TimeClicked(object sender, TappedRoutedEventArgs e)
        {
            if (TimeClick != null && !((sender as FrameworkElement).DataContext as ITimeStopListModel).Disabled)
                TimeClick(this, new RoutedEventArgs());
        }

        private void StopClicked(object sender, TappedRoutedEventArgs e)
        {
            if (StopClick != null && !((sender as FrameworkElement).DataContext as ITimeStopListModel).Disabled)
                StopClick(this, new RoutedEventArgs());
        }
    }
}
