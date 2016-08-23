using CityTransitApp.CityTransitElements.PageParts;
using CityTransitApp.Implementations;
using CityTransitElements.Controllers;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase;
using TransitBase.BusinessLogic;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        private UIElement CurrentPositionLayer = null;
        MapController controller;
        object parameter;

        public PageStateManager stateManager;
        public MapPage()
        {
            InitializeComponent();
            stateManager = new PageStateManager(this, true);
            stateManager.InitializeState += InitializePageState;
            stateManager.SaveState += SavePageState;
            stateManager.RestoreState += RestorePageState;
        }

        #region frame stack handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateManager.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            controller.Dispose();
            stateManager.OnNavigatedFrom(e);
        }
        #endregion

        private void RestorePageState(object obj)
        {
            this.parameter = obj;
            createController(obj);
        }

        private object SavePageState()
        {
            return parameter;
        }

        protected async void InitializePageState(object parameter)
        {
            this.parameter = parameter;
            createController(parameter);
            stateManager.ScheduleTask<MapPage>(10000, page => page.DoLocationMarking(), preExecute: true);
        }

        private void createController(object parameter)
        {
            var mapProxy = new PhoneMapProxy(MapControl);
            if (parameter is StopParameter)
            {
                var stopController = new StopMapController();
                stopController.Bind(mapProxy, parameter);
                stopController.StopGroupSelected += stopController_StopGroupSelected;
                stopController.TimeTableSelected += stopController_TimeTableSelected;
                stopController.TripSelected += stopController_TripSelected;
                this.controller = stopController;
            }
            else if (parameter is TripParameter)
            {
                var tripController = new TripMapController();
                tripController.Bind(mapProxy, parameter);
                tripController.StopSelected += tripController_StopSelected;
                this.controller = tripController;
            }
            else if (parameter is Way)
            {
                var planController = new PlanMapController();
                planController.Bind(mapProxy, parameter);
                this.controller = planController;
            }
            else throw new ArgumentException("Invalid MapPage parameter.");
        }

        #region Map element navigation handlers
        void stopController_StopGroupSelected(object sender, StopParameter e)
        {
            Frame.Navigate(typeof(StopPage), e);
        }
        void stopController_TimeTableSelected(object sender, TimetableParameter e)
        {
            Frame.Navigate(typeof(TimetablePage), e);
        }
        void stopController_TripSelected(object sender, TripParameter e)
        {
            Frame.Navigate(typeof(TripPage), e);
        }

        void tripController_StopSelected(object sender, StopParameter e)
        {
            Frame.Navigate(typeof(StopPage), e);
        }
        #endregion

        private async void DoLocationMarking()
        {
            GeoCoordinate myLocation = await CurrentLocation.Get(upToDate: true);
            if (myLocation != null)
            {
                if (CurrentPositionLayer != null)
                    MapControl.Children.Remove(CurrentPositionLayer);

                CurrentPositionLayer = createUserMark();
                MapControl.SetLocation(CurrentPositionLayer, myLocation.ToGeopoint());
                MapControl.SetNormalizedAnchorPoint(CurrentPositionLayer, new Point(0.5, 0.5));
                MapControl.Children.Add(CurrentPositionLayer);
            }
        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            MapControl.MapServiceToken = App.Config.MapAuthenticationToken;
        }

        private UIElement createUserMark()
        {
            Grid root = new Grid();
            root.Children.Add(new Ellipse
            {
                Fill = new SolidColorBrush(Colors.White),
                Height = 30,
                Width = 30,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            root.Children.Add(new Ellipse
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 40, 140, 0)),
                Height = 22,
                Width = 22,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            root.Children.Add(new Ellipse
            {
                Stroke = new SolidColorBrush(Colors.Black),
                Height = 30,
                Width = 30,
                StrokeThickness = 2,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return root;
        }

        public bool IsMapEmpty
        {
            get
            {
                int emptyCount = 0;
                if (CurrentPositionLayer != null)
                    emptyCount = 1;
                return MapControl.Children.Count == emptyCount;
            }
        }

        public void ClearMap()
        {
            if (!IsMapEmpty)
            {
                MapControl.Children.Clear();
                if (CurrentPositionLayer != null)
                    MapControl.Children.Add(CurrentPositionLayer);
            }
        }
    }
}
