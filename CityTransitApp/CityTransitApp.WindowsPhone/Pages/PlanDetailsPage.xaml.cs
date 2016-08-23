using CityTransitApp.CityTransitElements.PageParts;
using CityTransitApp.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlanDetailsPage : Page
    {
        private WayModel wayModel;

        public PageStateManager stateManager;
        public PlanDetailsPage()
        {
            this.InitializeComponent();
            stateManager = new PageStateManager(this);
            stateManager.InitializeState += InitializePageState;
        }

        #region frame stack handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            stateManager.OnNavigatedFrom(e);
        }
        #endregion

        private void InitializePageState(object parameter)
        {
            this.wayModel = (WayModel)parameter;
            PlanDetailsPart.SetContent(wayModel);
        }

        private void Map_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MapPage), wayModel.Way);
        }
    }
}
