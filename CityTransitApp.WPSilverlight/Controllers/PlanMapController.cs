using CityTransitApp.WPSilverlight.PageElements.MapElements;
using CityTransitApp.WPSilverlight.PageParts;
using Microsoft.Phone.Maps.Controls;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TransitBase.BusinessLogic;
using TransitBase.Entities;

namespace CityTransitApp.WPSilverlight.Controllers
{
    class PlanMapController : MapController
    {
        private int wayIndex;
        private List<MapLayer> openedPopupLayers = new List<MapLayer>();
        private List<PlanItemPopupModel> mapItems = new List<PlanItemPopupModel>();
        private Dictionary<Ellipse, Action> tapActions = new Dictionary<Ellipse, Action>();

        public override async void Bind(MapPage page)
        {
            base.Bind(page);
            base.RegisterElementTypes(typeof(PlanItemPopup), typeof(Ellipse));

            Way selected = PlanningTab.Current.SelectedWay.Way;

            double north = double.MinValue, west = double.MaxValue, south = double.MaxValue, east = double.MinValue;

            foreach (var item in selected)
            {
                Microsoft.Phone.Maps.Controls.MapPolyline line = new Microsoft.Phone.Maps.Controls.MapPolyline
                {
                    StrokeColor = item.Route.RouteGroup.GetColors().MainColor,
                    StrokeThickness = 8
                };

                foreach (var x in item.ShapePoints)
                {
                    line.Path.Add(Convert(x));
                    north = Math.Max(north, x.Latitude);
                    east = Math.Max(east, x.Longitude);
                    south = Math.Min(south, x.Latitude);
                    west = Math.Min(west, x.Longitude);
                }
                page.Map.MapElements.Add(line);

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
                if (element is Ellipse)
                    tapActions[(Ellipse)element].Invoke();
            };

            await Task.Delay(250);
            page.Map.SetView(new LocationRectangle(north, west, south, east), MapAnimationKind.None);
        }

        private void PutStopCircle(MapPage page, Route route, Stop stop, TimeSpan time, int stopCount, PlanItemPopupType type, double walkBeforeMeters = 0.0)
        {
            MapLayer mapLayer = new MapLayer();
            var content = new Ellipse
            {
                Fill = route.RouteGroup.GetColors().PrimaryColorBrush,
                Stroke = route.RouteGroup.GetColors().MainColorBrush,
                StrokeThickness = 3,
                Width = 26,
                Height = 26
            };
            mapLayer.Add(new MapOverlay()
            {
                GeoCoordinate = Convert(stop.Coordinate),
                PositionOrigin = new Point(0.5, 0.5),
                Content = content
            });
            page.Map.Layers.Add(mapLayer);

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

        private void closePopups(MapPage page)
        {
            foreach (var currentPopupLayer in openedPopupLayers)
                page.Map.Layers.Remove(currentPopupLayer);
            openedPopupLayers.Clear();
        }

        private void openPopup(MapPage page, PlanItemPopupModel popupModel, bool downside = false)
        {
            var popupContent = new PlanItemPopup();
            popupContent.SetModel(popupModel);
            MapLayer popupLayer = new MapLayer();
            popupLayer.Add(new MapOverlay()
            {
                GeoCoordinate = Convert(popupModel.Stop.Coordinate),
                PositionOrigin = new Point(0.5, downside ? 0 : 1),
                Content = popupContent
            });
            openedPopupLayers.Add(popupLayer);
            page.Map.Layers.Add(popupLayer);
        }
    }
}
