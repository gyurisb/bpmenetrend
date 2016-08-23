using CityTransitElements.Effects;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class RouteStopPanel : UserControl
    {
        public RouteStopPanel()
        {
            InitializeComponent();
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

        public double CornerRadius
        {
            get { return Root.CornerRadius.TopLeft; }
            set
            {
                Root.CornerRadius = new CornerRadius(value);
                NrBorder.CornerRadius = new CornerRadius(value, 0, 0, value);
            }
        }

        public double NumberWidth
        {
            get { return NrBorder.Width; }
            set { NrBorder.Width = value; }
        }
    }
}
