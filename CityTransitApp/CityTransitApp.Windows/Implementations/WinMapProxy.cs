using CityTransitElements.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CityTransitApp.Implementations
{
    class WinMapProxy : IMapControl
    {
        private MapControl.Map composedControl;
        private Dictionary<UIElement, Border> containerOfElement = new Dictionary<UIElement, Border>();
        private List<MapControl.MapPolyline> polyLines = new List<MapControl.MapPolyline>();

        public event EventHandler<Point> MapTapped;
        public event EventHandler CenterChanged;

        public WinMapProxy(MapControl.Map composedControl)
        {
            this.composedControl = composedControl;
            composedControl.Tapped += composedControl_Tapped;
            composedControl.ViewportChanged += composedControl_CenterChanged;
        }

        public void Dispose()
        {
            composedControl.Tapped -= composedControl_Tapped;
            composedControl.ViewportChanged -= composedControl_CenterChanged;
            foreach (var container in containerOfElement.Values)
                composedControl.Children.Remove(container);
            foreach (var line in polyLines)
                composedControl.Children.Remove(line);
        }

        private void composedControl_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (MapTapped != null)
                MapTapped(this, e.GetPosition(composedControl));
        }

        private void composedControl_CenterChanged(object sender, EventArgs e)
        {
            if (CenterChanged != null)
                CenterChanged(this, new EventArgs());
        }

        public FrameworkElement Element
        {
            get { return composedControl; }
        }

        public double ZoomLevel
        {
            get { return composedControl.ZoomLevel; }
        }

        public GeoCoordinate Center
        {
            get 
            {
                var center = composedControl.Center;
                return new GeoCoordinate(center.Latitude, center.Longitude); 
            }
        }

        public IList<DependencyObject> Children
        {
            //get { return composedControl.Children.Where(c => c is Border).Cast<Border>().Select(b => b.Child).Cast<DependencyObject>().ToList(); }
            get { return containerOfElement.Keys.Cast<DependencyObject>().ToList(); }
        }

        public void AddChild(UIElement control, GeoCoordinate coordinate, Point anchorPoint)
        {
            var container = new Border { Child = control };
            container.Loaded += (sender, args) =>
            {
                //container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                //container.Arrange(new Rect(0, 0, container.DesiredSize.Width, container.DesiredSize.Height));
                container.Margin = new Thickness(-container.ActualWidth * anchorPoint.X, -container.ActualHeight * anchorPoint.Y, 0, 0);
            };
            MapControl.MapPanel.SetLocation(container, new MapControl.Location(coordinate.Latitude, coordinate.Longitude));
            containerOfElement.Add(control, container);
            composedControl.Children.Add(container);
        }

        public void AddChildAt(int position, UIElement control, GeoCoordinate coordinate, Point anchorPoint)
        {
            throw new NotImplementedException();
            //MapControl.MapPanel.SetLocation(control, new MapControl.Location(coordinate.Latitude, coordinate.Longitude));
            //composedControl.Children.Insert(position, control);
        }

        public void RemoveChild(UIElement control)
        {
            var container = containerOfElement[control];
            containerOfElement.Remove(control);
            composedControl.Children.Remove(container);
        }

        public void BringControlToForeground(UIElement control)
        {
            var container = containerOfElement[control];
            composedControl.Children.Remove(container);
            composedControl.Children.Add(container);
        }

        public void AddPolyLine(MapLine line)
        {
            var pointList = line.Points.ToArray();
            var cleanPointList = new List<GeoCoordinate>();
            cleanPointList.Add(pointList[0]);
            for (int i = 1; i < pointList.Length - 1; i++)
                if (!(pointList[i - 1] == pointList[i + 1] && pointList[i] != pointList[i - 1]))
                    cleanPointList.Add(pointList[i]);
            cleanPointList.Add(pointList[pointList.Length - 1]);

            var polyLine = new MapControl.MapPolyline
            {
                StrokeThickness = line.Thickness,
                Stroke = new SolidColorBrush(line.Color),
                Locations = cleanPointList.Select(x => new MapControl.Location(x.Latitude, x.Longitude)).ToList(),
                Fill = new SolidColorBrush(line.Color),
                FillRule = FillRule.Nonzero
            };
            composedControl.Children.Add(polyLine);
            polyLines.Add(polyLine);
        }

        public async Task TrySetViewBoundsAsync(LocationRectangle bounds, bool animate)
        {
            composedControl.ZoomToBounds(new MapControl.Location(bounds.South, bounds.West), new MapControl.Location(bounds.North, bounds.East));
        }
    }
}
