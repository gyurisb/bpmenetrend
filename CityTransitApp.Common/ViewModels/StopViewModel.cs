using CityTransitApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.BusinessLogic.Helpers;
using TransitBase.Entities;
using CityTransitServices.Tools;
using System.Threading.Tasks;
using CityTransitApp.Common;

namespace CityTransitApp.Common.ViewModels
{
    public class StopViewModel : ViewModel<StopParameter>
    {
        public static readonly int MinShowDistance = 0;

        #region ViewModel Data

        public StopParameter OriginalParameter;
        public StopGroup Stop { get; set; }
        public DateTime StartTime = DateTime.Now;
        public bool CurrentTime = true;
        public bool Near;
        public Stop SourceStop;
        public GeoCoordinate Location;
        public ObservableCollection<StopGroupModel> ItemsSource { get; private set; }
        public object ScrollPosition; //ListViewPositionResult

        public bool BottomSpacing { get { return Get<bool>(); } set { Set(value); } }
        public bool NoTransfer { get { return Get<bool>(); } set { Set(value); } }
        public bool InProgress { get { return Get<bool>(); } set { Set(value); } }
        public double ContentListBottomMargin { get { return Get<double>(); } set { Set(value); } }
        //public Thickness ContentListMargin { get { return Get<Thickness>(); } set { Set(value); } }

        #endregion

        protected bool ShowTransfers { get { return !CurrentTime; } }

        private bool addFooter;
        public StopViewModel(bool addFooter = true)
        {
            this.ItemsSource = new ObservableCollection<StopGroupModel>();
            this.addFooter = addFooter;
        }

        public override void Initialize(StopParameter initialData)
        {
            this.OriginalParameter = initialData;
            this.Stop = initialData.StopGroup;

            CommonComponent.Current.UB.History.AddStopHistory(Stop);

            if (initialData.DateTime != null)
            {
                this.StartTime = initialData.DateTime.Value;
                this.CurrentTime = false;
            }

            if (initialData.Location != null)
            {
                if (initialData.IsNear)
                {
                    this.Near = true;
                    this.Location = CurrentLocation.Last;
                }
                else
                {
                    this.SourceStop = initialData.SourceStop;
                    this.Location = SourceStop.Coordinate;
                }
            }

            if (ShowTransfers)
                this.BottomSpacing = true;
            else
                this.ContentListBottomMargin = -39;

            if (CurrentTime)
            {
                AddTaskToSchedule(() => this.SetContentAsync());
            }
            else
            {
                setContentAsync(); //await
            }
        }

        public async Task SetContentAsync()
        {
            await setContentAsync();
        }

        private List<StopGroup> nearStops = null;
        private HashSet<Route> allRoutes = null;
        private GeoCoordinate stopLoc = null;
        private bool stopAdded = false;
        public async Task<bool> AddNearStopToItemSource()
        {

            if (nearStops == null)
            {
                nearStops = Stop.TransferTargets(400).ToList();
                allRoutes = new HashSet<Route>(ItemsSource.SelectMany(x => x.Items.Select(y => y.Route)));
                stopLoc = Location ?? new GeoCoordinate(Stop.Stops.Average(s => s.Latitude), Stop.Stops.Average(s => s.Longitude));
                if (nearStops.Count == 0)
                    this.NoTransfer = true;
            }

            if (nearStops.Any())
            {
                var nearStop = nearStops.First();
                nearStops.RemoveAt(0);
                var nearItems = (await calculateRoutes(nearStop)).Where(x => !allRoutes.Contains(x.Route)).ToList();
                if (nearItems.Count > 0)
                {
                    ItemsSource.Add(createStopHeader(nearStop, nearItems, false, true, stopLoc, showSeparator: true));
                    allRoutes.UnionWith(nearItems.Select(x => x.Route));
                    stopAdded = true;
                }
            }
            if (nearStops.Count == 0)
            {
                if (!stopAdded)
                {
                    this.NoTransfer = true;
                }
                else
                {
                    this.BottomSpacing = false;
                    this.ContentListBottomMargin = -60;
                }
                return false;
            }
            return true;
        }

        private async Task setContentAsync()
        {
            ItemsSource.Clear();
            this.InProgress = true;
            if (Near)
            {
                this.Location = await CurrentLocation.Get();
            }
            var stopItems = await calculateRoutes(Stop);
            ItemsSource.Add(createStopHeader(Stop, stopItems, !CurrentTime, Near, Location, ShowTransfers));
            this.InProgress = false;
        }

        private StopGroupModel createStopHeader(StopGroup stop, IList<RouteModel> items, bool showTime, bool showDistance, GeoCoordinate location = null, bool showBtn = false, bool showSeparator = false)
        {
            var model = new StopGroupModel(items, addFooter)
            {
                Stop = stop,
                IsWheelchairVisible = stop.Stops.Any(s => s.WheelchairBoardingAvailable),
                TimeText = StartTime.ToRelativeString(),
                IsTransferVisible = showTime,
                IsDistanceVisible = false,
                IsBtnVisible = showBtn,
                IsSeparatorVisible = showSeparator,
            };

            if (showDistance && items.Count > 0)
            {
                double dist = items.Select(i => i.Distance).Average();
                if (dist > MinShowDistance)
                {
                    double walkTime = items.Select(i => i.WalkTime.TotalMinutes).Average();
                    model.NearDistance = StringFactory.LocalizeDistance(dist);
                    model.NearDirection = StringFactory.CardinalToString(stop.InverseDirection(location.Latitude, location.Longitude));
                    model.NearWalkingtime = (int)Math.Ceiling(walkTime);
                    model.IsDistanceVisible = true;
                }
            }
            return model;
        }

        private async Task<List<RouteModel>> calculateRoutes(StopGroup stop)
        {
            if (CurrentTime)
                this.StartTime = DateTime.Now;
            return await ArrivalTrips.CalculateRoutes(stop.Stops, StartTime, Location, SourceStop);
        }
    }
}
