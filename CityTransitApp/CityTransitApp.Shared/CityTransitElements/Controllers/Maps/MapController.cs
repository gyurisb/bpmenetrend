using CityTransitApp;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TransitBase;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using CityTransitApp;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml.Shapes;

namespace CityTransitElements.Controllers
{
    public interface IMapControl : IDisposable
    {
        event EventHandler<Point> MapTapped;
        event EventHandler CenterChanged;
        FrameworkElement Element { get; }
        double ZoomLevel { get; }
        GeoCoordinate Center { get; }
        IList<DependencyObject> Children { get; }

        void AddChild(UIElement control, GeoCoordinate coordinate, Point anchorPoint);
        void AddChildAt(int position, UIElement control, GeoCoordinate coordinate, Point anchorPoint);
        void RemoveChild(UIElement control);
        void BringControlToForeground(UIElement control);
        void AddPolyLine(MapLine line);
        Task TrySetViewBoundsAsync(LocationRectangle bounds, bool animate);
    }

    public class MapLine
    {
        public IEnumerable<GeoCoordinate> Points;
        public Color Color;
        public double Thickness;
    }

    public class LocationRectangle
    {
        public LocationRectangle(double north, double west, double south, double east)
        {
            North = north;
            South = south;
            East = east;
            West = west;
        }

        public double North { get; set; }
        public double South { get; set; }
        public double East { get; set; }
        public double West { get; set; }

        public GeoCoordinate Center
        {
            get
            {
                return new GeoCoordinate { Latitude = (North + South) / 2, Longitude = (East + West) / 2 };
            }
        }

#if WINDOWS_PHONE_APP
        public GeoboundingBox ToGeoboundingBox()
        {
            return new GeoboundingBox(
                new BasicGeoposition { Latitude = North, Longitude = West },
                new BasicGeoposition { Latitude = South, Longitude = East }
                );
        }
#endif
    }

    public abstract class MapController : IDisposable
    {
        protected IMapControl MapControl;
        private List<Type> elementTypes;
        protected event EventHandler EmptyMapTap;
        protected event EventHandler<UIElement> MapElementTap;

        public virtual void Bind(IMapControl control, object parameter)
        {
            this.MapControl = control;
            MapControl.MapTapped += handleMapTapped;
        }

        protected void RegisterElementTypes(params Type[] elementTypes)
        {
            this.elementTypes = elementTypes.ToList();
        }

        protected void AddControlToMap(UIElement control, GeoCoordinate coordinate, Point point)
        {
            MapControl.AddChild(control, coordinate, point);
            control.Tapped += control_Tapped;
        }

        protected void AddControlToMap(UIElement control, GeoCoordinate coordinate)
        {
            MapControl.AddChild(control, coordinate, new Point(0.5, 0.5));
            control.Tapped += control_Tapped;
        }

        protected void AddControlToMapAt(int position, UIElement control, GeoCoordinate coordinate, Point point)
        {
            MapControl.AddChildAt(position, control, coordinate, point);
            control.Tapped += control_Tapped;
        }

        protected void RemoveControlFromMap(UIElement control)
        {
            MapControl.RemoveChild(control);
            control.Tapped -= control_Tapped;
        }

        protected async void BringControlToForeground(UIElement control)
        {
            MapControl.BringControlToForeground(control);
        }


        void control_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MapElementTap != null)
                MapElementTap(this, (UIElement)sender);
            e.Handled = true;
        }
        void handleMapTapped(object sender, Point p)
        {
            Rect rect = new Rect(p.X - 50, p.Y, 100, 100);
            var elementsAtTap = VisualTreeHelper.FindElementsInHostCoordinates(rect, MapControl.Element).ToList();

            List<UIElement> controls = new List<UIElement>();
            if (elementTypes.Count > 1)
            {
                var controlsWithPriorities = elementsAtTap
                    .Where(c => elementTypes.Contains(c.GetType()))
                    .Select(c => new { Priority = elementTypes.IndexOf(c.GetType()), Control = c })
                    .ToList();
                if (controlsWithPriorities.Count > 0)
                {
                    int maxPriority = controlsWithPriorities.Min(x => x.Priority);
                    controls = controlsWithPriorities.Where(x => x.Priority == maxPriority).Select(x => x.Control).ToList();
                }
            }
            else if (elementTypes.Count == 1)
            {
                controls = elementsAtTap.Where(c => c.GetType() == elementTypes.Single()).ToList();
            }

            if (controls.Count > 0)
            {
                if (MapElementTap != null)
                {
                    if (controls.Count == 1)
                        MapElementTap(MapControl, controls.Single());
                    else
                        MapElementTap(MapControl, controls.MinBy(c => distanceTo(c, p, MapControl.Element)));
                }
            }
            else
            {
                if (EmptyMapTap != null)
                {
                    EmptyMapTap(MapControl, new EventArgs());
                }
            }
        }

        private static Point getPosition(UIElement element, UIElement relativeTo)
        {
            GeneralTransform myTransform = element.TransformToVisual(relativeTo);
            return myTransform.TransformPoint(new Point(0, 0));
        }
        private static double distanceTo(UIElement element, Point point, UIElement container)
        {
            Point elementPoint = getPosition(element, container);
            double dX = point.X - elementPoint.X;
            double dY = point.Y - elementPoint.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        public static LocationRectangle SetCenter(LocationRectangle rect, GeoCoordinate center)
        {
            double north, west, south, east;
            if (rect.North - center.Latitude < center.Latitude - rect.South)
            {
                north = center.Latitude + center.Latitude - rect.South;
                south = rect.South;
            }
            else
            {
                north = rect.North;
                south = center.Latitude - (rect.North - center.Latitude);
            }

            if (rect.East - center.Longitude < center.Longitude - rect.West)
            {
                west = rect.West;
                east = center.Longitude + (center.Longitude - rect.West);
            }
            else
            {
                west = center.Longitude - (rect.East - center.Longitude);
                east = rect.East;
            }

            return new LocationRectangle(north, west, south, east);
        }

        public static LocationRectangle WidenBoundaries(LocationRectangle rect)
        {
            var center = rect.Center;
            double latUnit = 150.0 / App.Config.LatitudeDegreeDistance;
            double lonUnit = 150.0 / App.Config.LongitudeDegreeDistance;
            LocationRectangle widedRect = new LocationRectangle(
                north: Math.Max(rect.North, rect.Center.Latitude + latUnit),
                south: Math.Min(rect.South, rect.Center.Latitude - latUnit),
                east: Math.Max(rect.East, rect.Center.Longitude + lonUnit),
                west: Math.Min(rect.West, rect.Center.Longitude - lonUnit)
                );
            return widedRect;
        }

        public static LocationRectangle GetBoundaries(IEnumerable<GeoCoordinate> points)
        {
            double north = double.MinValue, west = double.MaxValue, south = double.MaxValue, east = double.MinValue;
            foreach (var point in points)
            {
                north = Math.Max(north, point.Latitude);
                east = Math.Max(east, point.Longitude);
                south = Math.Min(south, point.Latitude);
                west = Math.Min(west, point.Longitude);
            }
            return new LocationRectangle(north, west, south, east);
        }

        public static GeoCoordinate Average(IEnumerable<GeoCoordinate> coords)
        {
            double lat = 0, lon = 0;
            int n = 0;
            foreach (var coord in coords)
            {
                lat += coord.Latitude;
                lon += coord.Longitude;
                n++;
            }
            return new GeoCoordinate { Latitude = lat / n, Longitude = lon / n };
        }

        public MapCircle CreateColoredCircle(Brush fill, Brush stroke, Brush overlay)
        {
            var content = new MapCircle();
            var frame = new Ellipse
            {
                Width = 28,
                Height = 28,
                Fill = new SolidColorBrush(Colors.Transparent)
            };
            var ellipse = new Ellipse
            {
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 3,
                Width = 22,
                Height = 22
            };
            content.PointerEntered += (sender, args) => frame.Fill = overlay;
            content.PointerExited += (sender, args) => frame.Fill = new SolidColorBrush(Colors.Transparent);
            content.PointerCanceled += (sender, args) => frame.Fill = new SolidColorBrush(Colors.Transparent);
            content.Children.Add(frame);
            content.Children.Add(ellipse);
            return content;
        }

        public virtual void Dispose()
        {
            MapControl.Dispose();
        }
    }

    public class MapCircle : Grid { }
}
