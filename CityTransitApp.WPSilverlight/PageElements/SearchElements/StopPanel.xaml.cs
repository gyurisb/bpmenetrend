using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace CityTransitApp.WPSilverlight.PageElements.SearchElements
{
    public partial class StopPanel : UserControl
    {
        public StopPanel()
        {
            InitializeComponent();
        }

        public TextTrimming DescriptionTrimming
        {
            get { return DescriptionText.TextTrimming; }
            set { DescriptionText.TextTrimming = value; }
        }

        public double BorderThickness
        {
            get { return Root.BorderThickness.Top; }
            set { Root.BorderThickness = new Thickness(0, value, 0, 0); }
        }
    }
}
