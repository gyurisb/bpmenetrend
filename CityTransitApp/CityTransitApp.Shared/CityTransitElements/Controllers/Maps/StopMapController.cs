using CityTransitApp;
using CityTransitApp.CityTransitElements.PageElements.MapMarkers;
using CityTransitApp.Common.ViewModels;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.Entities;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace CityTransitElements.Controllers
{
    class StopMapController : MapController
    {
        public event EventHandler<TimetableParameter> TimeTableSelected;
        public event EventHandler<TripParameter> TripSelected;
        public event EventHandler<StopParameter> StopGroupSelected;

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
        bool fromMainPage = false;
        StopParameter stopParam;

        public override void Bind(IMapControl page, object parameter)
        {
            base.Bind(page, parameter);
            base.RegisterElementTypes(typeof(StopPopup), typeof(StopPushpin));
            stopParam = (StopParameter)parameter;

            if (stopParam.StopGroup != null)
            {
                this.stopGroup = stopParam.StopGroup;
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

            if (stopParam.Location != null)
            {
                //this.postQuery.Add("location", locationStr);
                if (!stopParam.Location.IsNear)
                {
                    this.sourceStop = stopParam.Location.Stop;
                    this.location = sourceStop.Coordinate;
                }
                else
                {
                    this.location = CurrentLocation.Last;
                    isNear = true;
                }
            }
            if (stopParam.DateTime != null)
            {
                //this.postQuery.Add("dateTime", dateTimeStr);
                this.dateTime = stopParam.DateTime.Value;
                isNow = false;
            }

            if (isNow)
            {
                timeUpdaterTask = new PeriodicTask(DoTimeUpdate);
                timeUpdaterTask.RunEveryMinute();
            }
            if (isNear)
            {
                locationUpdaterTask = new PeriodicTask(10000, DoLocationUpdate);
                locationUpdaterTask.Run(delay: 1000);
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
            this.MapElementTap += (sender, element) =>
            {
                if (element is StopPushpin)
                    tapActions[element].Invoke();
            };

            var boundAddition = isNear ? new GeoCoordinate[] { CurrentLocation.Last ?? App.Config.CenterLocation } : new GeoCoordinate[0];
            var boundaries = calculateBoundaries(mainPoints.Concat(boundAddition));

            SetBoundaries(page, boundaries, stopParam.StopGroup == null);
        }

        private async void SetBoundaries(IMapControl map, LocationRectangle boundaries, bool noStopInParameter)
        {
            await Task.Delay(250);
            await initializeMapLabels(map);
            await map.TrySetViewBoundsAsync(boundaries, false);
            //while (page.ZoomLevel < 15)
            //{
            //    page.SetView(boundaries, MapAnimationKind.None);
            //    await Task.Delay(100);
            //}
            //while (page.IsMapEmpty)
            //{
            //    initializeMapLabels(page);
            //    await Task.Delay(100);
            //}

            map.CenterChanged += async (sender, args) =>
            {
                await mapFillingLock.WaitAsync();
                if (map.ZoomLevel >= App.Config.LowStopsMapLevel)
                {
                    await tryCreateStopLabelsAt(map, map.Center);
                }
                else if (map.ZoomLevel >= App.Config.HighStopsMapLevel)
                {
                    await tryCreateHighStopLabelsAt(map, map.Center);
                    clearMap(map, 2.1);
                }
                else
                {
                    clearMap(map);
                }
                mapFillingLock.Release();
            };

            if (isNear && noStopInParameter && mainPoints.Length == 0)
            {
                var nearestResult = await StopTransfers.GetNearestStop(CurrentLocation.Last);
                if (nearestResult != null)
                {
                    this.location = CurrentLocation.Last;
                    this.mainPoints = nearestResult.Stop.Group.Stops.Select(s => s.Coordinate).ToArray();
                    var boundAddition = new GeoCoordinate[] { CurrentLocation.Last ?? App.Config.CenterLocation };

                    boundaries = calculateBoundaries(mainPoints.Concat(boundAddition));

                    await map.TrySetViewBoundsAsync(boundaries, false);
                    await initializeMapLabels(map);
                }
            }
        }

        private LocationRectangle calculateBoundaries(IEnumerable<GeoCoordinate> points)
        {
            var boundaries = GetBoundaries(points);
            boundaries = WidenBoundaries(boundaries);
            boundaries = SetCenter(boundaries, Average(points));
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

        private async Task initializeMapLabels(IMapControl page)
        {
            if (mainPoints.Length > 0)
                await tryCreateStopLabelsAt(page, Average(mainPoints), mainStops);
            if (isNear)
                await tryCreateStopLabelsAt(page, location ?? App.Config.CenterLocation);
        }

        private async Task tryCreateStopLabelsAt(IMapControl page, GeoCoordinate currentLocation, IEnumerable<Stop> mandatoryStops = null)
        {
            if (!lowMarkedLocations.Any(loc => currentLocation.GetDistanceTo(loc) < App.Config.LowStopsRadius * 0.8))
            {
                lowMarkedLocations.Add(currentLocation);
                //highMarkedLocations.Add(currentLocation);
                await createStopLabelsAt(page, currentLocation, mandatoryStops, radius: App.Config.LowStopsRadius);
            }
        }

        private async Task tryCreateHighStopLabelsAt(IMapControl page, GeoCoordinate currentLocation)
        {
            if (!highMarkedLocations.Any(loc => currentLocation.GetDistanceTo(loc) < App.Config.HighStopsRadius * 0.8))
            {
                highMarkedLocations.Add(currentLocation);
                await createStopLabelsAt(page, currentLocation, priorityLimit: 2.1, radius: App.Config.HighStopsRadius);
            }
        }

        private async Task createStopLabelsAt(IMapControl page, GeoCoordinate currentLocation, IEnumerable<Stop> mandatoryStops = null, double priorityLimit = double.MaxValue, int radius = 1000)
        {
            mandatoryStops = mandatoryStops ?? new Stop[0];
            var mapPoints = await Task.Run(delegate
            {
                var stops = App.TB.Stops.Where(s => s.StraightLineDistanceTo(currentLocation) < radius);
                return mandatoryStops.Union(stops).Except(markedStops).GroupBy(s => s.Coordinate).ToList();
            });
            addStopLabels(page, mapPoints, priorityLimit);
        }

        private void addStopLabels(IMapControl page, IList<IGrouping<GeoCoordinate, Stop>> mapPoints, double priorityLimit = double.MaxValue)
        {
            foreach (var position in mapPoints.Reverse())
            {
                var stops = position.ToList();

                bool isCurrent = this.mainStops.Intersect(stops).Count() > 0;
                var control = StopPushpin.Create(page, position.Key, isCurrent, stops, this.dateTime, this.location, this.sourceStop, !isCurrent);
                if (control != null && control.Priority <= priorityLimit)
                {
                    this.addedPushpins.Add(control);
                    this.markedStops.UnionWith(stops);
                    control.StopClicked += (sender, stop) =>
                    {
                        if (StopGroupSelected != null)
                            StopGroupSelected(this, createStopGroupParam(stop.Group, (sender as StopPushpin).IsDistanceIgnored));
                    };
                    control.TripClicked += (sender, route) =>
                    {
                        StopGroup stop = route.Stop.Group;
                        if (route.NextTripTime != null)
                        {
                            if (TripSelected != null)
                                TripSelected(this, createTripParam(route.NextTrip, route.Stop, (sender as StopPushpin).IsDistanceIgnored));
                        }
                        else
                        {
                            if (TimeTableSelected != null)
                                TimeTableSelected(this, new TimetableParameter { Stop = stop, Route = route.Route, SelectedTime = dateTime });
                        }
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

                    //AddControlToMapAt(0, control, position.Key, new Point(0.5, 0.5));
                    AddControlToMap(control, position.Key, new Point(0.5, 0.5));

                    Action currentTappedAction = async delegate()
                    {
                        if (!control.IsExpanded)
                        {
                            clearSelection();
                            var popup = await control.ShowPopup();
                            AddControlToMap(popup, position.Key, new Point(0.5, 1));
                            Selected = control;
                            //BringControlToForeground(control);
                        }
                    };
                    tapActions[control] = currentTappedAction;
                    //control.Tapped += (sender, args) =>
                    //{
                    //    currentTappedAction();
                    //};
                }
            }
        }

        void clearSelection()
        {
            if (Selected != null)
            {
                RemoveControlFromMap(Selected.Popup);
                Selected.HideContent();
                Selected = null;
            }
        }

        void clearMap(IMapControl page, double priorityLimit = 0.0)
        {
            foreach (var element in page.Children.ToList())
            {
                var pushpin = element as StopPushpin;
                if (pushpin != null && !pushpin.IsCurrent && pushpin.Priority >= priorityLimit)
                {
                    RemoveControlFromMap(pushpin);
                    foreach (var stop in pushpin.Stops)
                        markedStops.Remove(stop);
                    addedPushpins.Remove(pushpin);
                }
            }
            lowMarkedLocations.Clear();
            if (priorityLimit == 0)
                highMarkedLocations.Clear();
        }

        private StopParameter createStopGroupParam(StopGroup newStop, bool ignoreLocation = false)
        {
            return new StopParameter
            {
                StopGroup = newStop,
                DateTime = stopParam.DateTime,
                Location = ignoreLocation ? null : stopParam.Location
            };
        }
        private TripParameter createTripParam(Trip trip, Stop stop, bool ignoreLocation = false)
        {
            return new TripParameter
            {
                Trip = trip,
                DateTime = stopParam.DateTime,
                Location = ignoreLocation ? null : stopParam.Location,
                CurrentStop = stop,
                NextTrips = true
            };
        }

        public override void Dispose()
        {
            base.Dispose();
            if (locationUpdaterTask != null)
                locationUpdaterTask.Cancel();
            if (timeUpdaterTask != null)
                timeUpdaterTask.Cancel();
        }
    }
}
