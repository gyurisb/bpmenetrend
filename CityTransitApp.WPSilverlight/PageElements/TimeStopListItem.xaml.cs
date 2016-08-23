using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.WPSilverlight.Effects;
using CityTransitApp.Common.ViewModels;

namespace CityTransitApp.WPSilverlight.PageElements
{
    public partial class TimeStopListItem : UserControl
    {
        public TimeStopListItem()
        {
            InitializeComponent();
            TimeText.SetValue(RotateEffect.IsEnabledProperty, true);
            StopText.SetValue(RotateEffect.IsEnabledProperty, true);
        }

        public event EventHandler<RoutedEventArgs> TimeClick;
        public event EventHandler<RoutedEventArgs> StopClick;

        private void TimeClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (TimeClick != null && !((sender as FrameworkElement).DataContext as ITimeStopListModel).Disabled)
                TimeClick(this, new RoutedEventArgs());
        }

        private void StopClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (StopClick != null && !((sender as FrameworkElement).DataContext as ITimeStopListModel).Disabled)
                StopClick(this, new RoutedEventArgs());
        }
    }
}
