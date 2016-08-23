using PlannerComponent.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TransitBase.Entities;
using System.Globalization;
using CityTransitApp.WPSilverlight;
using PlannerComponent_WP80;

namespace CityTransitApp.PlannerComponentImplementation
{
    public class PlannerComponentWPSilverlightCpp : IPlannerComponent
    {
        private PlannerRuntimeComponent comp;
        public PlannerComponentWPSilverlightCpp(PlannerRuntimeComponent comp) { this.comp = comp; }

        public PlannerRuntimeComponent ComposedComponent { get { return comp; } }

        public IEnumerable<Tuple<DateTime, Trip>> GetCurrentTrips(DateTime currentTime, Route route, IEnumerable<Stop> stops, int prevCount, int nextCount, TimeSpan limit)
        {
            return comp.GetCurrentTrips(currentTime.ToString(CultureInfo.InvariantCulture), route.ID, stops.Select(s => s.ID).ToArray(), prevCount, nextCount, (int)limit.TotalMinutes)
                .Select(x => x.TimeDifference > limit.TotalMinutes || x.TripID == 0 ? null : Tuple.Create(currentTime + TimeSpan.FromMinutes(x.TimeDifference), App.TB.Logic.GetTripByID(x.TripID, route.ID)));
        }

        public void SetParams(PlannerComponent.Interface.PlanningArgs args)
        {
            comp.SetParams(new PlannerComponent_WP80.PlanningArgs
            {
                EnabledTypes = args.EnabledTypes,
                LatitudeDegreeDistance = args.LatitudeDegreeDistance,
                LongitudeDegreeDistance = args.LongitudeDegreeDistance,
                OnlyWheelchairAccessibleStops = args.OnlyWheelchairAccessibleStops,
                OnlyWheelchairAccessibleTrips = args.OnlyWheelchairAccessibleTrips,
                Type = Convert(args.Type),
                WalkSpeedRate = args.WalkSpeedRate
            });
        }

        public Way CalculatePlanning(StopGroup source, StopGroup target, DateTime startTime, PlannerComponent.Interface.PlanningAspect aspect)
        {
            var middleWay = comp.CalculatePlanning(source.ID, target.ID, startTime.ToString(CultureInfo.InvariantCulture), Convert(aspect));
            var way = new PlannerComponent.Interface.Way
            {
                LastWalkDistance = middleWay.LastWalkDistance,
                Message = middleWay.Message,
                TotalTime = TimeSpan.FromMinutes(middleWay.TotalTime),
                TotalTransfers = middleWay.TotalTransfers,
                TotalWalk = middleWay.TotalWalk
            };
            if (middleWay.Entries != null)
            {
                foreach (var e in middleWay.Entries)
                {
                    var entry = new PlannerComponent.Interface.WayEntry
                    {
                        Route = App.TB.Logic.GetRouteByID(e.RouteID),
                        TripType = App.TB.TripTypes[e.TripTypeID],
                        StartStop = App.TB.Logic.GetStopByID(e.StartStopID),
                        EndStop = App.TB.Logic.GetStopByID(e.EndStopID),
                        StartTime = TimeSpan.FromMinutes(e.StartTimeMinutes),
                        EndTime = TimeSpan.FromMinutes(e.EndTimeMinutes),
                        WaitMinutes = e.WaitMinutes,
                        WalkBeforeMeters = e.WalkBeforeMeters,
                        StopCount = e.Stops.Length - 1
                    };
                    entry.AddRange(e.Stops.Select(sId => App.TB.Logic.GetStopByID(sId)));
                    way.Add(entry);
                }
            }
            return way;
        }

        public void Close(bool safe)
        {
            comp.Close(safe);
        }

        private static PlannerComponent_WP80.PlanningTimeType Convert(PlannerComponent.Interface.PlanningTimeType val)
        {
            switch (val)
            {
                case PlannerComponent.Interface.PlanningTimeType.Arrival: return PlannerComponent_WP80.PlanningTimeType.Arrival;
                case PlannerComponent.Interface.PlanningTimeType.Departure: return PlannerComponent_WP80.PlanningTimeType.Departure;
                default: throw new NotImplementedException();
            }
        }

        private PlannerComponent_WP80.PlanningAspect Convert(PlannerComponent.Interface.PlanningAspect val)
        {
            switch (val)
            {
                case PlannerComponent.Interface.PlanningAspect.Time: return PlannerComponent_WP80.PlanningAspect.Time;
                case PlannerComponent.Interface.PlanningAspect.TransferCount: return PlannerComponent_WP80.PlanningAspect.TransferCount;
                case PlannerComponent.Interface.PlanningAspect.WalkDistance: return PlannerComponent_WP80.PlanningAspect.WalkDistance;
                default: throw new NotImplementedException();
            }
        }
    }
}
