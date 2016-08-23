using CityTransitApp.WPSilverlight.PageElements.MapElements;
using Microsoft.Phone.Maps.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TransitBase.BusinessLogic;
using TransitBase;

namespace CityTransitApp.WPSilverlight.Controllers
{
    class TripMapController : MapController
    {
        private MapLayer currentPopupLayer = null;
        private Dictionary<Ellipse, Action> tapActions = new Dictionary<Ellipse, Action>();

        public override async void Bind(MapPage page)
        {
            base.Bind(page);
            base.RegisterElementTypes(typeof(TripStopPopup), typeof(Ellipse));

            var trip = App.Model.GetTripByID(
                int.Parse(page.NavigationContext.QueryString["tripId"]),
                int.Parse(page.NavigationContext.QueryString["routeId"])
            );
            var routeGroup = trip.Route.RouteGroup;
            int position = int.Parse(page.NavigationContext.QueryString["position"]);
            DateTime dateTime = DateTime.Parse(page.NavigationContext.QueryString["dateTime"]);
            Microsoft.Phone.Maps.Controls.MapPolyline line = new Microsoft.Phone.Maps.Controls.MapPolyline
            {
                StrokeColor = trip.Route.RouteGroup.GetColors().MainColor,
                StrokeThickness = 8
            };
            Way.Entry entry = new Way.Entry(trip.TripType);
            line.Path.AddRange(entry.ShapePoints.Select(c => Convert(c)));
            page.Map.MapElements.Add(line);

            int i = 0;
            var stops = trip.Stops;
            foreach (var stop in stops)
            {
                MapLayer mapLayer = new MapLayer();
                var content = new Ellipse
                {
                    Fill = new SolidColorBrush(i == 0 ? routeGroup.GetColors().SecondaryColorBrush.Color : i == position ? Colors.White : i < position ? routeGroup.GetColors().PrimaryColorBrush.Color : Colors.White),
                    Stroke = trip.Route.RouteGroup.GetColors().MainColorBrush,
                    StrokeThickness = 3,
                    Width = 26,
                    Height = 26
                };
                mapLayer.Add(new MapOverlay()
                {
                    GeoCoordinate = Convert(stop.Item2.Coordinate),
                    PositionOrigin = new Point(0.5, 0.5),
                    Content = content
                });
                page.Map.Layers.Add(mapLayer);

                int curPos = i;
                tapActions[content] = delegate()
                {
                    clearOpenedOpopup(page);

                    var popupContent = new TripStopPopup();
                    popupContent.Initialize(trip, curPos, stops[position].Item1);
                    popupContent.StopClick += (sender1, clickedStop) =>
                    {
                        string uri = String.Format("/StopPage.xaml?id={0}&dateTime={1}&location={2}", clickedStop.Group.ID, dateTime + stops[curPos].Item1 - stops[position].Item1, clickedStop.ID);
                        page.NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                    };
                    MapLayer popupLayer = new MapLayer();
                    popupLayer.Add(new MapOverlay()
                    {
                        GeoCoordinate = Convert(stop.Item2.Coordinate),
                        PositionOrigin = new Point(0.5, 1),
                        Content = popupContent
                    });
                    currentPopupLayer = popupLayer;
                    page.Map.Layers.Add(popupLayer);
                };

                i++;
            }

            this.EmptyMapTap += (sender, args) => clearOpenedOpopup(page);
            this.MapElementTap += (sender, element) =>
            {
                if (element is Ellipse)
                    tapActions[(Ellipse)element].Invoke();
            };

            var boundaries = GetBoundaries(stops.Select(s => Convert(s.Item2.Coordinate)));
            await Task.Delay(250);
            page.Map.SetView(boundaries, MapAnimationKind.None);
        }

        private void clearOpenedOpopup(MapPage page)
        {
            if (currentPopupLayer != null)
            {
                page.Map.Layers.Remove(currentPopupLayer);
                currentPopupLayer = null;
            }
        }
    }
}
