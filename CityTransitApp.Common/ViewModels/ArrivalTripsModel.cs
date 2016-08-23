using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;
using CityTransitApp;
using TransitBase.BusinessLogic;
using TransitBase;

namespace CityTransitApp.Common.ViewModels
{
    public class ArrivalTrips
    {
        public static async Task<List<RouteModel>> CalculateRoutes(IEnumerable<Stop> stops, DateTime startTime, GeoCoordinate location = null, Stop sourceStop = null, double distanceLimit = double.MaxValue)
        {
            //Kiszámolom az indulási időket az egyes stop-triptype párosokhoz
            bool isFar = false;
            var targetWalkStops = stops.Select(s => new StopDistanceResult { Stop = s });
            if (location != null)
            {
                if (location.GetDistanceTo(stops.First().Coordinate) < distanceLimit)
                {
                    if (sourceStop != null)
                        targetWalkStops = await StopTransfers.WalkDistances(sourceStop, stops);
                    else
                        targetWalkStops = await StopTransfers.WalkDistances(location, stops);
                }
                else
                {
                    isFar = true;
                }
            }

            return await Task.Run(() =>
            {
                //if (currentTime) startTime = DateTime.Now;

                List<RouteModel> routes = new List<RouteModel>();
                int i = 0;
                foreach (var curWalkStop in targetWalkStops)
                {
                    TimeSpan walkTime = location != null && curWalkStop.DistanceInMeters < distanceLimit ? curWalkStop.EstimatedDuration : TimeSpan.Zero;
                    foreach (Route route in curWalkStop.Stop.Routes.Where(r => r.TravelRoute.Last().Stop != curWalkStop.Stop.Group))
                    //foreach (var routeAndIndex in curWalkStop.Stop.GetRoutes())
                    {
                        var model = new RouteModel
                        {
                            //Route = routeAndIndex.Item1,
                            //Position = routeAndIndex.Item2,
                            Route = route,
                            Stop = curWalkStop.Stop,
                            WalkTime = walkTime,
                            Distance = curWalkStop.DistanceInMeters,
                            IsFar = isFar || curWalkStop.DistanceInMeters >= distanceLimit,
                            NoLocation = (location == null)
                        };
                        model.SetNextArrive(startTime);
                        if (model.HasAnyTrip)
                            routes.Add(model);
                    }
                    i++;
                }

                //összerántom az azonos és azonos célú route-okat
                var ret = routes
                    .GroupBy(x => x.Route).Select(x => x.MinBy(e => e.NextTripTime ?? DateTime.MaxValue))
                    .OrderBy(r => r.NextTripTime ?? DateTime.MaxValue)
                    .Distinct(new RouteEqualityComparer());

                //Ha szentendrei a következő, akkor a következő békásit is ki kell írni (szentendreire kattintva nem látszanának)
                //ezért a szentendrére menő békási HÉV-et léptetem amíg békási nem lesz
                var grouped = ret.Where(x => x.NextTrip != null).GroupBy(x => x.NextTrip).Where(x => x.Count() > 1);
                foreach (var groupedRoute in grouped.SelectMany(x => x).Where(x => x.NextTrip.Route != x.Route))
                    while (groupedRoute.HasAnyTrip && groupedRoute.NextTrip != null && groupedRoute.Route != groupedRoute.NextTrip.Route)
                        groupedRoute.SetNextArrive(startTime);

                return ret
                    .Where(r => r.HasAnyTrip)
                    .OrderByText(r => r.RouteGroup.Name + " " + r.Name)
                    .OrderBy(r => r.RouteGroup.GetCustomTypePriority())
                    .OrderBy(r => r.NextTripTime ?? DateTime.MaxValue)
                    .ToList();
            });
        }
    }
    public class StopGroupModel : List<object>, INotifyPropertyChanged
    {
        public StopGroup Stop { get; set; }
        public string TimeText { get; set; }
        public int NearDistance { get; set; }
        public string NearDirection { get; set; }
        public int NearWalkingtime { get; set; }
        public bool IsWheelchairVisible { get; set; }
        public bool IsTransferVisible { get; set; }
        public bool IsDistanceVisible { get; set; }
        public bool IsSeparatorVisible { get; set; }
        public bool IsBtnVisible { get; set; }
        private bool btnEnabled = true;
        public bool BtnEnabled
        {
            get { return btnEnabled; }
            set
            {
                btnEnabled = value;
                propertyChanged("BtnEnabled");
            }
        }

        public IEnumerable<RouteModel> Items { get { return this.Take(this.Count - 1).Cast<RouteModel>(); } }

        public StopGroupModel(IEnumerable<RouteModel> items, bool addFooter) : base(items)
        {
            if (addFooter)
                Add(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void propertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class RouteModel : INotifyPropertyChanged
    {
        //mandatory properties
        public Route Route { get; set; }
        public Stop Stop { get; set; }
        public int Position { get; set; }
        public TimeSpan WalkTime { get; set; }
        public double Distance { get; set; }
        public bool IsFar { get; set; }
        public bool NoLocation { get; set; }

        //derived properties
        public int? NextTime { get; private set; }
        public Trip NextTrip { get; private set; }
        public DateTime? NextTripTime { get; private set; }
        public bool HasAnyTrip { get; private set; }

        //virtual properties
        public RouteGroup RouteGroup { get { return Route.RouteGroup; } }
        public string NextTimeText { get { return NextTime != null ? NextTime + "'" : "- "; } }
        public string NextTimeTextWithPlus { get { return NextTime != null ? "+" + NextTime + "'" : ""; } }
        public string Name { get { return NextTripTime == null ? Route.Name : NextTrip.GetNameAt(Stop.Group, NextTripTime.Value); } }
        public string DistanceText { get { return NoLocation ? ""  : StringFactory.LocalizeDistanceWithUnit(Distance); } }

        //flashing time
        private bool isTimeVisible = true;
        public bool IsTimeVisible
        {
            get { return isTimeVisible; }
            set
            {
                isTimeVisible = value;
                propertyChanged("IsTimeVisible");
            }
        }

        public void SetNextArrive(DateTime startTime)
        {
            DateTime nextStartTime = NextTripTime != null ? NextTripTime.Value + TimeSpan.FromMinutes(1) : startTime + WalkTime;
            var firstTrip = TransitProvider.GetCurrentTrips(nextStartTime, Route, Stop, 0, 1).Single();
            if (firstTrip != null)
            {
                DateTime nextTime = firstTrip.Item1;
                if (nextTime - startTime < TimeSpan.FromMinutes(100))
                {
                    NextTrip = firstTrip.Item2;
                    NextTripTime = nextTime;
                    NextTime = (int)Math.Ceiling((nextTime - startTime).TotalMinutes);
                }
                else
                {
                    NextTrip = null; NextTripTime = null; NextTime = null;
                }
                HasAnyTrip = true;
            }
            else
            {
                HasAnyTrip = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void propertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class RouteEqualityComparer : IEqualityComparer<RouteModel>
    {
        public bool Equals(RouteModel x, RouteModel y) { return x.RouteGroup.Name == y.RouteGroup.Name && x.Route.Name == y.Route.Name; }
        public int GetHashCode(RouteModel obj) { return (obj.RouteGroup.Name + ";" + obj.Route.Name).GetHashCode(); }
    }
}
