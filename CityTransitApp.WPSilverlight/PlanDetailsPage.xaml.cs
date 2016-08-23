using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.WPSilverlight.PageParts;
using CityTransitApp.WPSilverlight.Resources;

namespace CityTransitApp.WPSilverlight
{
    public partial class PlanDetailsPage : PhoneApplicationPage
    {
        private int wayIndex;

        public PlanDetailsPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            WayModel selected = PlanningTab.Current.SelectedWay;
            DataContext = selected;
            ContentList.ItemsSource = new WayModel[] { selected };
        }

        private void Map_Clicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MapPage.xaml?plan=" + wayIndex, UriKind.Relative));
        }

        private void BuildLocalizedApplicationBar()
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.PlanDetailsToMap;
        }
    }
}