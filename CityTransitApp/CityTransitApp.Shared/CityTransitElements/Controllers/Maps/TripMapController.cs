using CityTransitApp.CityTransitElements.PageElements.MapMarkers;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TransitBase.BusinessLogic;
using Windows.UI.Xaml.Shapes;
using CityTransitApp;
using Windows.UI.Xaml.Media;
using TransitBase.Entities;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using TransitBase;
using Windows.UI.Xaml.Controls;
using CityTransitElements.Effects;

namespace CityTransitElements.Controllers
{
    public class TripMapController : MapController
    {
        public event EventHandler<StopParameter> StopSelected;

        private UIElement currentPopupLayer = null;
        private Dictionary<MapCircle, Action> tapActions = new Dictionary<MapCircle, Action>();

        public override void Bind(IMapControl map, object parameter)
        {
            base.Bind(map, parameter);
            base.RegisterElementTypes(typeof(TripStopPopup), typeof(MapCircle));
            var tripParam = (TripParameter)parameter;

            var trip = tripParam.Trip;
            var routeGroup = trip.Route.RouteGroup;
            //int position = tripParam.Position;
            //DateTime dateTime = tripParam.DateTime ?? DateTime.Today;
            //Way.Entry entry = new Way.Entry(trip.TripType);
            int position = trip.IndexAt(tripParam.Stop, tripParam.DateTime.Value);
            map.AddPolyLine(new MapLine
            {
                //Points = entry.ShapePoints,
                Points = trip.TripType.Shape.Points.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)).ToList(),
                Thickness = 7,
                Color = trip.Route.RouteGroup.GetColors().MainColor
            });

            int i = 0;
            var stops = trip.Stops;
            foreach (var stop in stops)
            {
                var content = base.CreateColoredCircle(
                    new SolidColorBrush(i == 0 ? routeGroup.GetColors().SecondaryColorBrush.Color : i == position ? Colors.White : i < position ? routeGroup.GetColors().PrimaryColorBrush.Color : Colors.White),
                    trip.Route.RouteGroup.GetColors().MainColorBrush,
                    routeGroup.GetColors().SecondaryColorBrush
                );
                AddControlToMap(content, stop.Item2.Coordinate);

                int curPos = i;
                tapActions[content] = delegate()
                {
                    clearOpenedOpopup(map);

                    var popupContent = new TripStopPopup(map);
                    popupContent.Initialize(trip, curPos, stops[position].Item1);
                    popupContent.StopClick += (sender1, clickedStop) =>
                    {
                        if (StopSelected != null)
                            StopSelected(this, new StopParameter
                            {
                                StopGroup = clickedStop.Group,
                                DateTime = tripParam.DateTime.Value.Date + stops[curPos].Item1,
                                Location = new ParameterLocation { Stop = clickedStop }
                            });
                    };
                    currentPopupLayer = popupContent;
                    AddControlToMap(popupContent, stop.Item2.Coordinate, new Point(0.5, 1));
                };

                i++;
            }

            this.EmptyMapTap += (sender, args) => clearOpenedOpopup(map);
            this.MapElementTap += (sender, element) =>
            {
                if (element is MapCircle)
                    tapActions[(MapCircle)element].Invoke();
            };
            SetBoundaries(map, stops);
        }

        private async void SetBoundaries(IMapControl map, Tuple<TimeSpan, Stop>[] stops)
        {
            var boundaries = GetBoundaries(stops.Select(s => s.Item2.Coordinate));
            await Task.Delay(250);
            map.TrySetViewBoundsAsync(boundaries, false);
        }

        private void clearOpenedOpopup(IMapControl map)
        {
            if (currentPopupLayer != null)
            {
                RemoveControlFromMap(currentPopupLayer);
                currentPopupLayer = null;
            }
        }
    }
}
