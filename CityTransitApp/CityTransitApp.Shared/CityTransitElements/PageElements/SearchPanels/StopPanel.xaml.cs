using System;
using System.Collections.Generic;
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
using TransitBase;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageElements.SearchPanels
{
    public sealed partial class StopPanel : UserControl
    {
        public StopPanel()
        {
            this.InitializeComponent();
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
