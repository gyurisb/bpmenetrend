using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Maps.Controls;
using CityTransitApp.WPSilverlight.Controllers;
using CityTransitServices.Tools;
using TransitBase;

namespace CityTransitApp.WPSilverlight
{
    public partial class MapPage : PhoneApplicationPage
    {
        private PeriodicTask locationMarkingTask;
        private MapLayer CurrentPositionLayer = null;
        internal List<PeriodicTask> Tasks = new List<PeriodicTask>();

        public MapPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                if (NavigationContext.QueryString.ContainsKey("stopGroupID"))
                {
                    var controller = new StopMapController();
                    controller.Bind(this);
                }
                else if (NavigationContext.QueryString.ContainsKey("tripId"))
                {
                    var controller = new TripMapController();
                    controller.Bind(this);
                }
                else if (NavigationContext.QueryString.ContainsKey("plan"))
                {
                    var controller = new PlanMapController();
                    controller.Bind(this);
                }

                locationMarkingTask = new PeriodicTask(10000, DoLocationMarking);
                locationMarkingTask.Run(preExecute: true);
            }
            else
            {
                if (locationMarkingTask != null)
                    locationMarkingTask.Resume();
                foreach (var task in Tasks)
                    task.Resume();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (locationMarkingTask != null)
                locationMarkingTask.Cancel();
            foreach (var task in Tasks)
                task.Cancel();
        }

        private async void DoLocationMarking()
        {
            GeoCoordinate myLocation = await CurrentLocation.Get(upToDate: true);
            if (myLocation != null)
            {
                if (CurrentPositionLayer != null)
                    Map.Layers.Remove(CurrentPositionLayer);
                MapLayer myLocationLayer = new MapLayer();
                myLocationLayer.Add(new MapOverlay
                {
                    Content = createUserMark(),
                    PositionOrigin = new Point(0.5, 0.5),
                    GeoCoordinate = MapController.Convert(myLocation)
                });
                CurrentPositionLayer = myLocationLayer;
                Map.Layers.Add(myLocationLayer);
            }
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = App.Config.MapApplicationID;
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = App.Config.MapAuthenticationToken;
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
                return Map.Layers.Count == emptyCount;
            }
        }

        public void ClearMap()
        {
            if (!IsMapEmpty)
            {
                Map.Layers.Clear();
                if (CurrentPositionLayer != null)
                    Map.Layers.Add(CurrentPositionLayer);
            }
        }
    }
}