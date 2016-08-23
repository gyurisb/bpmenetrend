using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using TransitBase.Entities;
using CityTransitApp.WPSilverlight.Effects;

namespace CityTransitApp.WPSilverlight.PageElements
{
    public partial class RouteStopPanel : UserControl
    {
        public RouteStopPanel()
        {
            InitializeComponent();
            Root.SetValue(RotateEffect.IsEnabledProperty, true);
            //Root.SetValue(TiltEffect.IsTiltEnabledProperty, true);
        }

        public bool TimeStrip
        {
            get
            {
                return TimeStripGrid.Visibility == Visibility.Visible;
            }

            set
            {
                TimeStripGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
