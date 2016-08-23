using Microsoft.Phone.Maps.Controls;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CityTransitApp.WPSilverlight.Controllers
{
    abstract class MapController
    {
        protected MapPage Page;
        private List<Type> elementTypes;
        protected event EventHandler EmptyMapTap;
        protected event EventHandler<UIElement> MapElementTap;

        public virtual void Bind(MapPage page)
        {
            this.Page = page;
            Page.Tap += Page_Tap;
        }

        protected void RegisterElementTypes(params Type[] elementTypes)
        {
            this.elementTypes = elementTypes.ToList();
        }

        void Page_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var element = (UIElement)sender;
            Point p = e.GetPosition(element);
            Rect rect = new Rect(p.X - 50, p.Y, 100, 100);
            var elementsAtTap = VisualTreeHelper.FindElementsInHostCoordinates(rect, element);

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
                        MapElementTap(Page.Map, controls.Single());
                    else
                        MapElementTap(Page.Map, TransitBase.Extensions.MinBy(controls, c => distanceTo(c, p, element)));
                }
            }
            else
            {
                if (EmptyMapTap != null)
                {
                    EmptyMapTap(Page.Map, new EventArgs());
                }
            }
        }

        private Point getPosition(UIElement element, UIElement relativeTo)
        {
            GeneralTransform myTransform = element.TransformToVisual(relativeTo);
            return myTransform.Transform(new Point(0, 0));
        }
        private double distanceTo(UIElement element, Point point, UIElement container)
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

        public static GeoCoordinate Convert(TransitBase.GeoCoordinate coordinate)
        {
            return new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
        }
        public static TransitBase.GeoCoordinate Convert(GeoCoordinate coordinate)
        {
            return new TransitBase.GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
        }
    }
}
