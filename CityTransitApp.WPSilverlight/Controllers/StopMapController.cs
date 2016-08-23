using CityTransitApp.WPSilverlight.PageElements.MapElements;
using CityTransitServices.Tools;
using Microsoft.Phone.Maps.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.Entities;

namespace CityTransitApp.WPSilverlight.Controllers
{
    class StopMapController : MapController
    {
        private StopPushpin Selected = null;
        private List<StopPushpin> addedPushpins = new List<StopPushpin>();
        private HashSet<Stop> markedStops = new HashSet<Stop>();
        private List<GeoCoordinate> lowMarkedLocations = new List<GeoCoordinate>();
        private List<GeoCoordinate> highMarkedLocations = new List<GeoCoordinate>();
        private PeriodicTask timeUpdaterTask, locationUpdaterTask;
        private Dictionary<UIElement, Action> tapActions = new Dictionary<UIElement, Action>();
        private SemaphoreSlim mapFillingLock = new SemaphoreSlim(1);

        //given fields as parameters
        StopGroup stopGroup = null;
        HashSet<Stop> mainStops = new HashSet<Stop>();
        GeoCoordinate[] mainPoints = new GeoCoordinate[0];
        DateTime dateTime = DateTime.Now;
        GeoCoordinate location = null;
        Stop sourceStop = null;
        bool isNear = false, isNow = true;
        Dictionary<string, string> postQuery = new Dictionary<string, string>();
        bool fromMainPage = false;

        public override async void Bind(MapPage page)
        {
            base.Bind(page);
            base.RegisterElementTypes(typeof(StopPushpin));

            int stopGroupId = int.Parse(page.NavigationContext.QueryString["stopGroupID"]);
            if (stopGroupId != 0)
            {
                this.stopGroup = App.Model.GetStopGroupByID(stopGroupId);
                this.mainStops = new HashSet<Stop>(stopGroup.Stops);
                this.mainPoints = mainStops.Select(s => s.Coordinate).ToArray();
            }
            else
            {
                this.fromMainPage = true;
                if (StopTransfers.LastNearestStop != null)
                {
                    this.mainPoints = StopTransfers.LastNearestStop.Stop.Group.Stops.Select(s => s.Coordinate).ToArray();
                }
            }
            string locationStr = null, dateTimeStr = null;

            if (page.NavigationContext.QueryString.TryGetValue("location", out locationStr))
            {
                this.postQuery.Add("location", locationStr);
                if (locationStr != "near")
                {
                    this.sourceStop = App.Model.GetStopByID(int.Parse(locationStr));
                    this.location = sourceStop.Coordinate;
                }
                else
                {
                    this.location = CurrentLocation.Last;
                    isNear = true;
                }
            }
            if (page.NavigationContext.QueryString.TryGetValue("dateTime", out dateTimeStr))
            {
                this.postQuery.Add("dateTime", dateTimeStr);
                this.dateTime = System.Convert.ToDateTime(dateTimeStr);
                isNow = false;
            }

            if (isNow)
            {
                timeUpdaterTask = new PeriodicTask(DoTimeUpdate);
                timeUpdaterTask.RunEveryMinute();
                page.Tasks.Add(timeUpdaterTask);
            }
            if (isNear)
            {
                locationUpdaterTask = new PeriodicTask(10000, DoLocationUpdate);
                locationUpdaterTask.Run(delay: 1000);
                page.Tasks.Add(locationUpdaterTask);
            }


            //foreach (var transfer in transfers)
            //{
            //    Microsoft.Phone.Maps.Controls.MapPolyline line = new Microsoft.Phone.Maps.Controls.MapPolyline
            //    {
            //        StrokeColor = Colors.Gray,
            //        StrokeDashed = true,
            //        StrokeThickness = 8
            //    };
            //    line.Path.Add(transfer.Origin.Coordinate);
            //    line.Path.AddRange(transfer.InnerPoints.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
            //    line.Path.Add(transfer.Target.Coordinate);
            //    Map.MapElements.Add(line);
            //}
            this.EmptyMapTap += (sender, args) => clearSelection();
            this.MapElementTap += (sender, element) => tapActions[element].Invoke();

            var boundAddition = isNear ? new GeoCoordinate[] { CurrentLocation.Last ?? App.Config.CenterLocation } : new GeoCoordinate[0];
            var boundaries = calculateBoundaries(mainPoints.Concat(boundAddition));

            //await Task.Delay(250);
            await initializeMapLabels(page);
            page.Map.SetView(boundaries, MapAnimationKind.None);
            while (page.Map.ZoomLevel < 15)
            {
                page.Map.SetView(boundaries, MapAnimationKind.None);
                await Task.Delay(100);
            }
            //while (page.IsMapEmpty)
            //{
            //    initializeMapLabels(page);
            //    await Task.Delay(100);
            //}

            page.Map.CenterChanged += async (sender, args) =>
            {
                await mapFillingLock.WaitAsync();
                if (page.Map.ZoomLevel >= App.Config.LowStopsMapLevel)
                {
                    var newLocation = page.Map.Center;
                    await tryCreateStopLabelsAt(page, Convert(newLocation));
                }
                else if (page.Map.ZoomLevel >= App.Config.HighStopsMapLevel)
                {
                    var newLocation = page.Map.Center;
                    await tryCreateHighStopLabelsAt(page, Convert(newLocation));
                    clearMap(page, 2.1);
                }
                else
                {
                    clearMap(page);
                }
                mapFillingLock.Release();
            };

            if (isNear && stopGroupId == 0 && mainPoints.Length == 0)
            {
                var nearestResult = await StopTransfers.GetNearestStop(await CurrentLocation.Get());
                if (nearestResult != null)
                {
                    this.location = CurrentLocation.Last;
                    this.mainPoints = nearestResult.Stop.Group.Stops.Select(s => s.Coordinate).ToArray();
                    boundAddition = new GeoCoordinate[] { CurrentLocation.Last ?? App.Config.CenterLocation };

                    boundaries = calculateBoundaries(mainPoints.Concat(boundAddition));

                    page.Map.SetView(boundaries, MapAnimationKind.None);
                    await initializeMapLabels(page);
                }
            }
        }

        private LocationRectangle calculateBoundaries(IEnumerable<GeoCoordinate> points)
        {
            var points_ = points.Select(c => Convert(c));
            var boundaries = GetBoundaries(points_);
            boundaries = WidenBoundaries(boundaries);
            boundaries = SetCenter(boundaries, Average(points_));
            return boundaries;
        }

        private void DoTimeUpdate()
        {
            this.dateTime = DateTime.Now;
            updateAll();
        }

        private void DoLocationUpdate()
        {
            location = CurrentLocation.Last;
            updateAll();
        }

        private void updateAll()
        {
            foreach (var pushpin in this.addedPushpins)
            {
                pushpin.Update(dateTime, location);
            }
        }

        private async Task initializeMapLabels(MapPage page)
        {
            if (mainPoints.Length > 0)
                await tryCreateStopLabelsAt(page, Convert(Average(mainPoints.Select(c => Convert(c)))), mainStops);
            if (isNear)
                await tryCreateStopLabelsAt(page, location ?? App.Config.CenterLocation);
        }

        private async Task tryCreateStopLabelsAt(MapPage page, GeoCoordinate currentLocation, IEnumerable<Stop> mandatoryStops = null)
        {
            if (!lowMarkedLocations.Any(loc => currentLocation.GetDistanceTo(loc) < App.Config.LowStopsRadius * 0.8))
            {
                lowMarkedLocations.Add(currentLocation);
                //highMarkedLocations.Add(currentLocation);
                await createStopLabelsAt(page, currentLocation, mandatoryStops, radius: App.Config.LowStopsRadius);
            }
        }

        private async Task tryCreateHighStopLabelsAt(MapPage page, GeoCoordinate currentLocation)
        {
            if (!highMarkedLocations.Any(loc => currentLocation.GetDistanceTo(loc) < App.Config.HighStopsRadius * 0.8))
            {
                highMarkedLocations.Add(currentLocation);
                await createStopLabelsAt(page, currentLocation, priorityLimit: 2.1, radius: App.Config.HighStopsRadius);
            }
        }

        private async Task createStopLabelsAt(MapPage page, GeoCoordinate currentLocation, IEnumerable<Stop> mandatoryStops = null, double priorityLimit = double.MaxValue, int radius = 1000)
        {
            mandatoryStops = mandatoryStops ?? new Stop[0];
            var mapPoints = await Task.Run(delegate
            {
                var stops = App.TB.Stops.Where(s => s.StraightLineDistanceTo(currentLocation) < radius);
                return mandatoryStops.Union(stops).Except(markedStops).GroupBy(s => s.Coordinate).ToList();
            });
            addStopLabels(page, mapPoints, priorityLimit);
        }

        private void addStopLabels(MapPage page, List<IGrouping<GeoCoordinate, Stop>> mapPoints, double priorityLimit = double.MaxValue)
        {
            foreach (var position in mapPoints)
            {
                var stops = position.ToList();

                bool isCurrent = this.mainStops.Intersect(stops).Count() > 0;
                var control = StopPushpin.Create(position.Key, isCurrent, stops, this.dateTime, this.location, this.sourceStop, !isCurrent);
                if (control != null && control.Priority <= priorityLimit)
                {
                    this.addedPushpins.Add(control);
                    this.markedStops.UnionWith(stops);
                    control.StopClicked += (sender, stop) =>
                    {
                        page.NavigationService.Navigate(new Uri("/StopPage.xaml?id=" + stop.Group.ID + getPostQuery((sender as StopPushpin).IsDistanceIgnored), UriKind.Relative));
                    };
                    control.TripClicked += (sender, route) =>
                    {
                        StopGroup stop = route.Stop.Group;
                        string uri = null;
                        if (route.NextTripTime != null)
                        {
                            uri = String.Format("/TripPage.xaml?tripID={0}&routeID={1}&stopID={2}&nexttrips=true{3}", 
                                route.NextTrip.ID,
                                route.NextTrip.Route.ID,
                                stop.ID,
                                getPostQuery((sender as StopPushpin).IsDistanceIgnored));
                        }
                        else
                        {
                            uri = "/TimetablePage.xaml?stopID=" + stop.ID + "&routeID=" + route.Route.ID;
                            if (dateTime != null) uri += "&selectedTime=" + dateTime.ToString();
                        }
                        page.NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                    };
                    //if (fromMainPage)
                    //{
                    //    control.Content.PlanningFromClicked += (sender, args) =>
                    //    {
                    //        MainPage.Current.SetPlanningSource(stops.First().Group);
                    //        page.NavigationService.GoBack();
                    //    };
                    //    control.Content.PlanningToClicked += (sender, args) =>
                    //    {
                    //        MainPage.Current.SetPlanningDestination(stops.First().Group);
                    //        page.NavigationService.GoBack();
                    //    };
                    //}

                    MapLayer mapLayer = new MapLayer();
                    mapLayer.Add(new MapOverlay()
                    {
                        GeoCoordinate = Convert(position.Key),
                        PositionOrigin = new Point(0.5, 0.5),
                        Content = control
                    });
                    page.Map.Layers.Insert(0, mapLayer);

                    Action currentTapAction = delegate()
                    {
                        if (!control.IsExpanded)
                        {
                            clearSelection();
                            control.ShowContent();
                            Selected = control;
                            page.Map.Layers.Remove(mapLayer);
                            page.Map.Layers.Add(mapLayer);
                        }
                    };
                    tapActions[control] = currentTapAction;
                    //control.Tap += (sender, args) =>
                    //{
                    //    currentTapAction();
                    //};
                }
            }
        }

        void clearSelection()
        {
            if (Selected != null)
            {
                Selected.HideContent();
                Selected = null;
            }
        }

        void clearMap(MapPage page, double priorityLimit = 0.0)
        {
            foreach (var layer in page.Map.Layers.ToList())
            {
                var pushpin = layer.Single().Content as StopPushpin;
                if (pushpin != null && !pushpin.IsCurrent && pushpin.Priority >= priorityLimit)
                {
                    page.Map.Layers.Remove(layer);
                    foreach (var stop in pushpin.Stops)
                        markedStops.Remove(stop);
                    addedPushpins.Remove(pushpin);
                }
            }
            lowMarkedLocations.Clear();
            if (priorityLimit == 0)
                highMarkedLocations.Clear();
        }

        private string getPostQuery(bool ignoreLocation = false)
        {
            var pq = this.postQuery;
            if (ignoreLocation)
            {
                pq = new Dictionary<string, string>(this.postQuery);
                pq.Remove("location");
            }
            return String.Join("", pq.Select(x => String.Format("&{0}={1}", x.Key, x.Value)));
        }
    }
}
