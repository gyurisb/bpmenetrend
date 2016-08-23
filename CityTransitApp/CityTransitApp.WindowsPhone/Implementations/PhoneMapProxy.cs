using CityTransitElements.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using CityTransitServices;
using TransitBase;

namespace CityTransitApp.Implementations
{
    class PhoneMapProxy : IMapControl
    {
        public event EventHandler<Point> MapTapped;
        public event EventHandler CenterChanged;

        private MapControl composedControl;
        public FrameworkElement Element { get { return composedControl; } }


        public PhoneMapProxy(MapControl composedControl)
        {
            this.composedControl = composedControl;
            composedControl.MapTapped += composedControl_MapTapped;
            composedControl.Tapped += composedControl_Tapped;
            composedControl.CenterChanged += composedControl_CenterChanged;
        }

        void composedControl_CenterChanged(MapControl sender, object args)
        {
            if (CenterChanged != null)
                CenterChanged(sender, new EventArgs());
        }

        void composedControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MapTapped != null)
                MapTapped(sender, e.GetPosition(composedControl));
        }

        void composedControl_MapTapped(MapControl sender, MapInputEventArgs e)
        {
            if (MapTapped != null)
                MapTapped(sender, e.Position);
        }

        public void AddChild(UIElement control, GeoCoordinate coordinate, Point anchorPoint)
        {
            MapControl.SetLocation(control, coordinate.ToGeopoint());
            MapControl.SetNormalizedAnchorPoint(control, anchorPoint);
            composedControl.Children.Add(control);
        }

        public void AddChildAt(int position, UIElement control, GeoCoordinate coordinate, Point anchorPoint)
        {
            MapControl.SetLocation(control, coordinate.ToGeopoint());
            MapControl.SetNormalizedAnchorPoint(control, anchorPoint);
            composedControl.Children.Insert(position, control);
        }

        public void RemoveChild(UIElement control)
        {
            composedControl.Children.Remove(control);
        }

        public void BringControlToForeground(UIElement control)
        {
            composedControl.Children.DeepRemove(control);
            composedControl.Children.Add(control);
        }

        public void AddPolyLine(MapLine line)
        {
            MapPolyline polyLine = new MapPolyline
            {
                StrokeColor = line.Color,
                StrokeThickness = line.Thickness,
                Path = new Geopath(line.Points.Select(x => x.ToBasicGeoposition()))
            };

            composedControl.MapElements.Add(polyLine);
        }

        public async Task TrySetViewBoundsAsync(LocationRectangle bounds, bool animate)
        {
            await composedControl.TrySetViewBoundsAsync(bounds.ToGeoboundingBox(), null, animate ? MapAnimationKind.Default : MapAnimationKind.None);
        }

        public double ZoomLevel
        {
            get { return composedControl.ZoomLevel; }
        }

        public GeoCoordinate Center
        {
            get
            {
                var location = composedControl.Center;
                return new GeoCoordinate { Latitude = location.Position.Latitude, Longitude = location.Position.Longitude };
            }
        }

        public IList<DependencyObject> Children
        {
            get { return composedControl.Children; }
        }

        public void Dispose()
        {
            composedControl.MapTapped -= composedControl_MapTapped;
            composedControl.Tapped -= composedControl_Tapped;
            composedControl.CenterChanged -= composedControl_CenterChanged;
            //composedControl.Children.Clear();
            //composedControl.MapElements.Clear();
        }
    }
}
