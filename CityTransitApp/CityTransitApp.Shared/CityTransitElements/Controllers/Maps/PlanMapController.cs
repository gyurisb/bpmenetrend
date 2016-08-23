using CityTransitApp.CityTransitElements.PageElements.MapMarkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TransitBase.BusinessLogic;
using TransitBase.Entities;
using Windows.UI.Xaml.Shapes;
using CityTransitApp;
using CityTransitServices.Tools;
using Windows.Foundation;
using Windows.UI.Xaml;
using TransitBase;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace CityTransitElements.Controllers
{
    public class PlanMapController : MapController
    {
        private int wayIndex;
        private List<UIElement> openedPopupLayers = new List<UIElement>();
        private List<PlanItemPopupModel> mapItems = new List<PlanItemPopupModel>();
        private Dictionary<MapCircle, Action> tapActions = new Dictionary<MapCircle, Action>();

        public override void Bind(IMapControl page, object parameter)
        {
            base.Bind(page, parameter);
            base.RegisterElementTypes(typeof(PlanItemPopup), typeof(MapCircle));
            var selected = (Way)parameter;

            double north = double.MinValue, west = double.MaxValue, south = double.MaxValue, east = double.MinValue;

            foreach (var item in selected)
            {
                var pointList = new List<GeoCoordinate>();
                foreach (var x in item.ShapePoints)
                {
                    pointList.Add(x);
                    north = Math.Max(north, x.Latitude);
                    east = Math.Max(east, x.Longitude);
                    south = Math.Min(south, x.Latitude);
                    west = Math.Min(west, x.Longitude);
                }

                page.AddPolyLine(new MapLine
                {
                    Color = item.Route.RouteGroup.GetColors().MainColor,
                    Thickness = 7,
                    Points = pointList
                });

                //Microsoft.Phone.Maps.Controls.MapLayer mapLayer = new Microsoft.Phone.Maps.Controls.MapLayer();
                //mapLayer.Add(new MapOverlay()
                //{
                //    GeoCoordinate = line.Path[line.Path.Count / 2],
                //    Content = new Pushpin() { Content = item.Route.RouteGroup.Name + " - " + item.Route.Name }
                //});
                //Map.Layers.Add(mapLayer);

                PutStopCircle(page, item.Route, item.StartStop, item.StartTime, item.StopCount, item == selected.First() ? PlanItemPopupType.Start : PlanItemPopupType.MidStart);
                PutStopCircle(page, item.Route, item.EndStop, item.EndTime, item.StopCount, item == selected.Last() ? PlanItemPopupType.Finish : PlanItemPopupType.MidFinish);
            }

            this.EmptyMapTap += (sender, args) => closePopups(page);
            this.MapElementTap += (sender, element) =>
            {
                if (element is MapCircle)
                    tapActions[(MapCircle)element].Invoke();
            };
            SetBoundaries(page, north, west, south, east);
        }

        private async void SetBoundaries(IMapControl page, double north, double west, double south, double east)
        {
            await Task.Delay(250);
            page.TrySetViewBoundsAsync(new LocationRectangle(north, west, south, east), false);
        }

        private void PutStopCircle(IMapControl page, Route route, Stop stop, TimeSpan time, int stopCount, PlanItemPopupType type, double walkBeforeMeters = 0.0)
        {
            var content = base.CreateColoredCircle(
                route.RouteGroup.GetColors().PrimaryColorBrush,
                route.RouteGroup.GetColors().MainColorBrush,
                route.RouteGroup.GetColors().SecondaryColorBrush
                );
            AddControlToMap(content, stop.Coordinate);

            var model = new PlanItemPopupModel
            {
                Time = time,
                Stop = stop,
                Route = route,
                StopCount = stopCount,
                Type = type
            };
            mapItems.Add(model);

            tapActions[content] = delegate
            {
                closePopups(page);
                int index = mapItems.IndexOf(model);
                if (index == 0 || index == mapItems.Count - 1)
                {
                    openPopup(page, model);
                }
                else
                {
                    int pairIndex = index % 2 == 0 ? index - 1 : index + 1;
                    var pairModel = mapItems[pairIndex];
                    bool isDownside = model.Stop.Latitude < pairModel.Stop.Latitude;
                    openPopup(page, model, isDownside);
                    openPopup(page, pairModel, !isDownside);
                }
            };
        }

        private void closePopups(IMapControl page)
        {
            foreach (var currentPopupLayer in openedPopupLayers)
                RemoveControlFromMap(currentPopupLayer);
            openedPopupLayers.Clear();
        }

        private void openPopup(IMapControl page, PlanItemPopupModel popupModel, bool downside = false)
        {
            var popupContent = new PlanItemPopup(page);
            popupContent.SetModel(popupModel);
            openedPopupLayers.Add(popupContent);
            AddControlToMap(popupContent, popupModel.Stop.Coordinate, new Point(0.5, downside ? 0 : 1));
        }
    }
}
