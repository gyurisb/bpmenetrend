using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.BusinessLogic;
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
using CityTransitApp;
using CityTransitApp.Common.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class PlanDetailsPart : UserControl
    {
        private int wayIndex;

        public bool HasHeader { get; set; }

        public PlanDetailsPart()
        {
            this.InitializeComponent();
        }

        public void SetContent(WayModel selected)
        {
            DataContext = selected;
            selected.HasHeader = HasHeader;
            ContentList.ItemsSource = selected;
        }
    }
}
