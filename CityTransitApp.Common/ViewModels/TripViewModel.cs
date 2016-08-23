using CityTransitApp;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using TransitBase;
using TransitBase.BusinessLogic.Helpers;
using TransitBase.Entities;
using TransitBase.BusinessLogic;
using System.ComponentModel;
using CityTransitApp.Common;

namespace CityTransitApp.Common.ViewModels
{
    public class TripViewModel : ViewModel<TripParameter>
    {
        public EventHandler<object> ScrollIntoViewRequired;

        #region ViewModel state

        public Trip Trip;
        public StopGroup Stop;
        public TimeSpan[] Times;
        public DateTime DateTime;
        public DateTime NextTripTime;
        public int CurPos;

        public GeoCoordinate Location = null;
        public bool Near;
        public bool IsTimeSet;
        public Stop SourceStop;

        public bool ShowAmPm;
        public bool ShowDistance;

        public List<TimeStopListModel<Stop>> ItemsSource { get { return Get<List<TimeStopListModel<Stop>>>(); } set { Set(value); } }
        public string HeadsignText { get { return Get<string>(); } set { Set(value); } }
        public bool IsBarrierFree { get { return Get<bool>(); } set { Set(value); } }
        public bool IsTimeStripVisible { get { return Get<bool>(); } set { Set(value); } }
        public bool HeadsignAnimates { get { return Get<bool>(); } set { Set(value); } }
        public string AmPmText { get { return Get<string>(); } set { Set(value); } }
        public string DistanceText { get { return Get<string>(); } set { Set(value); } }

        public TabHeaderText[] HeaderSource { get { return Get<TabHeaderText[]>(); } set { Set(value); } }
        public int HeaderSelectedIndex { get { return Get<int>(); } set { Set(value); } }

        public bool usePivotPageing = false;
        public int CurrentPageIndex = 0;
        public object ScrollPosition;//ListViewPositionResult

        public bool IsDistanceVisible { get { return ShowAmPm || ShowDistance; } }
        public void NotifyDistanceChanged()
        {
            this.OnPropertyChanged("IsDistanceVisible");
        }
        public Route Route { get { return Trip.Route; } }
        public RouteGroup RouteGroup { get { return Trip.Route.RouteGroup; } }
        public string RouteName { get { return Trip.Route.RouteGroup.Name; } }

        private Stop CurrentStop { get { return Trip.Stops.First(x => x.Item2.Group == Stop).Item2; } }

        #endregion

        public override void Initialize(TripParameter param)
        {
            this.Trip = param.Trip;
            this.Stop = param.Stop;

            //optional parameters
            if (this.IsTimeSet = (param.DateTime != null))
                this.DateTime = param.DateTime.Value;
            else this.DateTime = DateTime.Now;
            if (param.Location != null)
            {
                if (param.Location.IsNear)
                {
                    this.Near = true;
                    this.Location = CurrentLocation.Last;
                }
                else
                {
                    this.SourceStop = param.Location.Stop;
                    this.Location = SourceStop.Coordinate;
                }
            }
            //boolean parameters
            bool nextTripStrip = param.NextTrips;
            setAmPmText();

            //TODO a trip.Route nem biztos, hogy az a route ami kell
            CommonComponent.Current.UB.History.AddTimetableHistory(Trip.Route, Stop, 1);

            this.HeadsignText = Trip.GetNameAt(Stop, DateTime);
            this.IsBarrierFree = Trip.WheelchairAccessible ?? false;

            //this.ViewModel = this.viewModel;

            if (nextTripStrip)
            {
                this.IsTimeStripVisible = true;
                this.HeadsignAnimates = true;
                setNextTrips(DateTime);
                this.HeaderSelectedIndex = 1;
            }
            this.ItemsSource = getTripStopsList(DateTime);
            //scrollToCurrentStop();

            if (!IsTimeSet && nextTripStrip)
            {
                AddTaskToSchedule(SetNextTripsPeriodically);
            }
        }

        public void PostInizialize()
        {
            scrollToCurrentStop();
        }

        public async Task ReverseDirection(Route targetRoute = null)
        {
            //if (TimeStripGrid.Visibility == Visibility.Visible)
            var routes = Trip.Route.RouteGroup.Routes;
            if (routes.Count == 1)
                return;
            var nextRoute = targetRoute ?? routes[(routes.IndexOf(Trip.Route) + 1) % routes.Count];
            var nextStop = Stop.OppositeOn(nextRoute);
            var tripTime = TransitProvider.GetCurrentTrips(DateTime, nextRoute, nextStop, 0, 1).Single();
            if (tripTime == null)
                return;
            this.Trip = tripTime.Item2;
            this.Stop = nextStop;
            if (Trip.Stops.Last().Item2.Group == Stop)
                this.Stop = Trip.Stops.First().Item2.Group;

            await setNextTrips(DateTime);
            this.HeaderSelectedIndex = 1;
            this.Trip = (this.HeaderSource[1].Tag as Tuple<DateTime, Trip>).Item2;
            this.IsBarrierFree = Trip.WheelchairAccessible ?? false;
            setAmPmText();
            setItemsSource(getTripStopsList(DateTime));
            this.HeadsignText = Trip.GetNameAt(Stop, NextTripTime);
            //TODO továbbfejlesztési lehetőség: A visszafele irányú járat a mostani irányba közlekedő járathoz időben legközelebb eső járat legyen és az a header listának ne a második eleme legyen..  
        }

        public void ChangeToHeader(int headerIndex, bool fromHeaderSelected)
        {
            var curBtn = this.HeaderSource[headerIndex];
            if (curBtn.Tag != null)
            {
                //ChangeToTrip((curBtn.Tag as Tuple<DateTime, Trip>).Item1, (curBtn.Tag as Tuple<DateTime, Trip>).Item2);
                this.Trip = (curBtn.Tag as Tuple<DateTime, Trip>).Item2;
                var dateTime = (curBtn.Tag as Tuple<DateTime, Trip>).Item1;

                if (!fromHeaderSelected)
                    this.HeaderSelectedIndex = headerIndex;
                this.IsBarrierFree = Trip.WheelchairAccessible ?? false;
                setAmPmText();
                setItemsSource(getTripStopsList(dateTime));
                this.HeadsignText = Trip.GetNameAt(Stop, NextTripTime);
            }
        }

        public void ChangeToTrip(DateTime dateTime, Trip trip)
        {
            this.Trip = trip;

            this.IsBarrierFree = Trip.WheelchairAccessible ?? false;
            setAmPmText();
            setItemsSource(getTripStopsList(dateTime));
            this.HeadsignText = Trip.GetNameAt(Stop, NextTripTime);
        }

        public bool ChangeStop(StopGroup stop)
        {
            var currentItem = this.ItemsSource.FirstOrDefault(x => x.Stop.Group == stop);
            if (currentItem != null)
            {
                this.Stop = stop;
                setItemsSource(getTripStopsList(GetTimeOf(currentItem)));
                return true;
            }
            return false;
        }

        public DateTime GetTimeOf(TimeStopListModel<Stop> currentItem)
        {
            return this.NextTripTime + this.Times[this.ItemsSource.IndexOf(currentItem)];
        }
        public DateTime GetTimeOfCurrentStop()
        {
            return this.NextTripTime + this.Times[CurPos];
        }

        public void SetNextTripsPeriodically()
        {
            setNextTripsPeriodically();
        }


        private void setAmPmText()
        {
            string time = (DateTime.Today + Trip.StartTime).ToString("t");
            this.AmPmText = "";
            if (time.Contains("AM"))
            {
                this.ShowAmPm = true;
                this.AmPmText = "AM";
            }
            else if (time.Contains("PM"))
            {
                this.ShowAmPm = true;
                this.AmPmText = "PM";
            }
        }

        private async void setNextTripsPeriodically()
        {
            this.DateTime = DateTime.Now;
            await setNextTrips(DateTime);
            this.HeaderSelectedIndex = 1;
            //TabHeader.SelectedIndex = 1;

            //if (btn == null)
            //{
            //    btn = tripControls.First();
            //    trip = (btn.Tag as Tuple<DateTime, Trip>).Item2;
            //}
            //NextTrip_Click(btn, null);
        }
        private async Task setNextTrips(DateTime time)
        {
            if (Location != null)
            {
                if (Near)
                {
                    var dist = await StopTransfers.WalkDistance(Location, CurrentStop.Coordinate);
                    setDistanceText(Services.Resources.LocalizedStringOf("TripStopDistance"), Location, dist.DistanceInMeters);
                    setNextTripsAtTime(time + dist.EstimatedDuration, time);

                    var newLocation = await CurrentLocation.Get();
                    if (newLocation != null && newLocation != Location)
                    {
                        this.Location = newLocation;
                        var dist1 = await StopTransfers.WalkDistance(Location, CurrentStop.Coordinate);
                        setDistanceText(Services.Resources.LocalizedStringOf("TripStopDistance"), Location, dist1.DistanceInMeters);
                        setNextTripsAtTime(time + dist1.EstimatedDuration, time);
                    }
                }
                else
                {
                    var dist = await StopTransfers.WalkDistance(SourceStop.Coordinate, CurrentStop.Coordinate);
                    setDistanceText(Services.Resources.LocalizedStringOf("TripTransferDistance"), Location, dist.DistanceInMeters);
                    setNextTripsAtTime(time + dist.EstimatedDuration, time);
                }
            }
            else
            {
                setNextTripsAtTime(time, time);
            }
        }
        private void setNextTripsAtTime(DateTime time, DateTime referenceTime)
        {
            var trips = TransitProvider.GetCurrentTrips(time, Trip.Route, Stop, 1, 3, limit: TimeSpan.FromDays(1));
            var newHeaderSource = new TabHeaderText[4];
            for (int i = 0; i < newHeaderSource.Length; i++)
            {
                newHeaderSource[i] = new TabHeaderText();
                if (trips[i] != null)
                {
                    DateTime time0 = trips[i].Item1;
                    double mins = Math.Ceiling((trips[i].Item1 - referenceTime).TotalMinutes);
                    newHeaderSource[i].PrimaryLine = time0.ShortTimeString();
                    newHeaderSource[i].SecondaryLine = ((mins >= 0) ? "+" : "") + mins + "'";
                    newHeaderSource[i].Tag = trips[i];
                }
                else
                {
                    newHeaderSource[i].PrimaryLine = "";
                    newHeaderSource[i].SecondaryLine = "";
                    newHeaderSource[i].Tag = null;
                }
            }
            this.HeaderSource = newHeaderSource;
        }

        //private async Task<DateTime> addWalkTime(DateTime time, GeoCoordinate location)
        //{
        //    if (currentStop.StraightLineDistanceTo(location.Latitude, location.Longitude) > StopPage.MinShowDistance)
        //        time += (await StopTransfers.WalkDistance(currentStop.Coordinate, location)).EstimatedDuration;
        //    return time;
        //}

        private void setDistanceText(string head, GeoCoordinate loc, double distance)
        {
            if (CurrentStop.StraightLineDistanceTo(loc.Latitude, loc.Longitude) > StopViewModel.MinShowDistance)
            {
                //int walkMinutes = (int)Math.Ceiling(distance.TotalMinutes);
                //DistanceText.Text = String.Format("{0}: {1} m ({2})",
                //    head,
                //    Math.Round(currentStop.Distance(loc.Latitude, loc.Longitude)),
                //    StringFactory.XMinutes(walkMinutes)
                //);
                this.DistanceText = StringFactory.LocalizeDistanceWithUnit(distance) + " " +
                                    StringFactory.CardinalToString(CurrentStop.InverseDirectionTo(loc.Latitude, loc.Longitude));
                this.ShowDistance = true;
            }
            else
            {
                this.ShowDistance = false;
            }
            this.NotifyDistanceChanged();
        }

        private List<TimeStopListModel<Stop>> getTripStopsList(DateTime dateTime)
        {
            //finding the current stop of the list (one stop can occur multiple times)
            this.CurPos = Trip.IndexAt(Stop, dateTime);
            if (Trip.Stops[CurPos].Item2.Group != Stop)
                throw new InvalidOperationException("Stop not found in trip stops list!");

            List<TimeStopListModel<Stop>> list = new List<TimeStopListModel<Stop>>();
            int i = 0;
            foreach (var time in Trip.Stops)
            {
                list.Add(new TimeStopListModel<Stop>
                {
                    Time = (dateTime.Date + time.Item1).ShortTimeString(),
                    Stop = time.Item2,
                    Position = i - CurPos
                });
                i++;
            }

            TimeSpan thisTime = Trip.Stops[CurPos].Item1;
            this.Times = Trip.Stops.Select(t => t.Item1 - thisTime).ToArray();
            this.NextTripTime = dateTime.NextDateTimeAt(thisTime);
            return list;
        }

        private void scrollToCurrentStop()
        {
            var item = this.ItemsSource.Single(m => m.Position == 0);
            int i = this.ItemsSource.IndexOf(item);
            var prevItem = this.ItemsSource[Math.Max(0, i - 2)];
            if (ScrollIntoViewRequired != null)
                ScrollIntoViewRequired(this, prevItem);
        }
        private void setItemsSource(IEnumerable<TimeStopListModel<Stop>> source)
        {
            this.ItemsSource = source.ToList();
            scrollToCurrentStop();
        }
    }

    public class TabHeaderText
    {
        public string PrimaryLine;
        public string SecondaryLine;
        public object Tag;
    }

    public interface ITimeStopListModel { bool Disabled { get; } }

    public class TimeStopListModel<TStop> : INotifyPropertyChanged, ITimeStopListModel
    {
        private string time;
        public String Time
        {
            get { return time; }
            set
            {
                time = value;
                notifyPropertyChanged("Time");
            }
        }
        private int position;
        public int Position
        {
            get { return position; }
            set
            {
                position = value;
                notifyPropertyChanged("Position");
                notifyPropertyChanged("ForegroundBrush");
                notifyPropertyChanged("BackgroundBrush");
            }
        }

        public TStop Stop { get; set; }
        public bool Disabled { get; set; }

        public object ForegroundBrush
        {
            get
            {
                if (Position < 0 || Disabled)
                    return CommonComponent.Current.Services.Resources.ColorOf("BeforeForegroundBrush");
                else
                    return CommonComponent.Current.Services.Resources.ColorOf("AppForegroundBrush");
            }
        }

        public object BackgroundBrush
        {
            get
            {
                if (Position == 0) return CommonComponent.Current.Services.Resources.ColorOf("AppHeaderBackgroundBrush");
                return Position % 2 == 0 ? CommonComponent.Current.Services.Resources.ColorOf("AppSecondaryBackgroundBrush") : CommonComponent.Current.Services.Resources.ColorOf("AppBackgroundBrush");
            }
        }


        private void notifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
