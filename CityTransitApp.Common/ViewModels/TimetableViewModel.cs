using CityTransitApp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using TransitBase.BusinessLogic.Helpers;
using TransitBase.Entities;
using CityTransitApp.Common;
using CityTransitServices.Tools;

namespace CityTransitApp.Common.ViewModels
{
    public class TimetableViewModel : ViewModel<TimetableParameter>
    {
        #region View state

        public Route Route { get { return Get<Route>(); } set { Set(value); } }
        public StopGroup Stop { get { return Get<StopGroup>(); } set { Set(value); } }
        public DateTime SelectedDay { get { return Get<DateTime>(); } set { Set(value); } }
        public TimeTableBodySource BodySource { get { return Get<TimeTableBodySource>(); } set { Set(value); } }
        public bool InProgress { get { return Get<bool>(); } set { Set(value); } }
        public object FavoriteIcon { get { return Get<object>(); } set { Set(value); } }

        public DateTime? SelectedTime;
        public List<Route> Routes;
        public double BodyVerticalOffset;
#if WINDOWS_PHONE_APP
        public bool ShowNeighborDays = true;
#else
        public bool ShowNeighborDays = false;
#endif

        #endregion

        public override void Initialize(TimetableParameter initialData)
        {
            nextTimeBg = Services.Resources.ColorOf("NextItemBacgroundBrush");
            transparentBg = Services.Resources.ColorOf("TransparentBrush");
            beforeTextFg = Services.Resources.ColorOf("BeforeForegroundBrush");
            appForegroundBrush = Services.Resources.ColorOf("AppForegroundBrush");

            this.SelectedDay = DateTime.Today;
            this.InProgress = true;
            this.Route = initialData.Route;
            this.Stop = initialData.Stop;
            this.FavoriteIcon = Services.Resources.IconOf("Favorite");
            this.SelectedTime = initialData.SelectedTime;

            if (this.Stop != null)
            {
                CommonComponent.Current.UB.History.AddTimetableHistory(Route, Stop, 5);
                routeStopChanged();

                if (this.SelectedTime == null)
                {
                    AddTaskToSchedule(() => SetBodyContentAsync());
                }
                else
                {
                    this.SelectedDay = SelectedTime.Value.Date;
                    setBodyContentAsync();
                }

                //if (CommonComponent.Current.Config.Advertising)
                //{
                //    AdControl.Latitude = stop.Stops.First().Latitude;
                //    AdControl.Longitude = stop.Stops.First().Longitude;
                //}
            }

            this.Routes = Route.RouteGroup.Routes.ToList();
        }
        public void SetRouteStopValue()
        {
            routeStopChanged();
        }
        public async Task SetBodyContentAsync()
        {
            await setBodyContentAsync();
        }

        public async Task ReverseDirection(Route targetRoute = null)
        {
            //this.BodySource = null;
            this.InProgress = true;

            if (targetRoute == null)
            {
                int i = Routes.IndexOf(Route);
                i = (i + 1) % Routes.Count;
                this.Route = Routes[i];
            }
            else
                this.Route = targetRoute;

            this.Stop = await Task.Run(() => Stop.OppositeOn(this.Route));
            if (Route.TravelRoute.Last().Stop == Stop)
                this.Stop = Route.TravelRoute.First().Stop;

            CommonComponent.Current.UB.History.AddTimetableHistory(Route, Stop, 2);
            routeStopChanged();
            await setBodyContentAsync();
        }

        public void ToggleFavorite()
        {
            if (!CommonComponent.Current.UB.Favorites.Contains(this.Route, this.Stop))
            {
                CommonComponent.Current.UB.Favorites.Add(this.Route, this.Stop);
                this.FavoriteIcon = Services.Resources.IconOf("UnFavorite");
            }
            else
            {
                CommonComponent.Current.UB.Favorites.Remove(this.Route, this.Stop);
                this.FavoriteIcon = Services.Resources.IconOf("Favorite");
            }
        }

        //private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (this.Controller != this)
        //        this.PropertyChanged -= ViewModel_PropertyChanged;
        //    if (e.PropertyName == "Route" || e.PropertyName == "Stop" || e.PropertyName == "SelectedDay")
        //        setBodyContentAsync();
        //}


        private void routeStopChanged()
        {
            CommonComponent.Current.UB.History.SetRecentStopAtRoute(Route, Stop);

            setFavoriteIcon();
            BeginnerTips.CheckFavoriteTip();
        }

        private void setFavoriteIcon()
        {
            if (Route == null || Stop == null)
                this.FavoriteIcon = Services.Resources.IconOf("Favorite");
            else this.FavoriteIcon = CommonComponent.Current.UB.Favorites.Contains(Route, Stop) ? Services.Resources.IconOf("UnFavorite") : Services.Resources.IconOf("Favorite");
        }

        #region body content creation
        private object nextTimeBg, transparentBg, beforeTextFg, appForegroundBrush;

        private async Task setBodyContentAsync()
        {
            if (SelectedTime != null)
                nextTimeBg = Services.Resources.ColorOf("NextBackgroundBrush");

            //this.BodySource = null;
            this.InProgress = true;

            DateTime selectedDate = SelectedDay;
            var bodySource = await Task.Run(() => calculateBodyContent(selectedDate));

            this.InProgress = false;
            this.BodySource = bodySource;
        }
        private TimeTableBodySource calculateBodyContent(DateTime selectedDate)
        {
            DateTime dateNow = SelectedTime ?? DateTime.Now;

            var nextArrival = (TransitProvider.GetCurrentTrips(dateNow, Route, Stop, 0, 1).SingleOrDefault() ?? Tuple.Create(DateTime.Now, (Trip)null)).Item1;

            var list = new List<TimeTableBodyHourGroup>();
            if (ShowNeighborDays)
            {
                //tegnapi járatok:
                if (DateTime.Now.Hour < 2 && selectedDate == DateTime.Today || DateTime.Now.Hour >= 22 && selectedDate == DateTime.Today + TimeSpan.FromDays(1))
                {
                    var yesterdayLast2Hour = CommonComponent.Current.TB.Logic.GetTimetable(Route, Stop, selectedDate - TimeSpan.FromDays(1)).Where(t => t.Item1.Hour >= 22);
                    if (yesterdayLast2Hour.Count() > 0)
                    {
                        list.Add(new TimeTableBodyLabelGroup { Label = (selectedDate == DateTime.Today ? Services.Resources.LocalizedStringOf("TimeTableYesterday") : Services.Resources.LocalizedStringOf("TimeTablePreviousDay")) + Services.Resources.LocalizedStringOf("TimeTableLabelPost") });
                        list.AddRange(createListFromTimetable(yesterdayLast2Hour, nextArrival));
                        list.Add(new TimeTableBodyLabelGroup { Label = (selectedDate == DateTime.Today ? Services.Resources.LocalizedStringOf("TimeTableToday") : Services.Resources.LocalizedStringOf("TimeTableThisDay")) + Services.Resources.LocalizedStringOf("TimeTableLabelPost") });
                    }
                }
            }
            //mai járatok:
            var timeTable = CommonComponent.Current.TB.Logic.GetTimetable(Route, Stop, selectedDate);
            list.AddRange(createListFromTimetable(timeTable, nextArrival));
            if (ShowNeighborDays)
            {
                //holnapi járatok:
                if (DateTime.Now.Hour >= 22 && selectedDate == DateTime.Today || DateTime.Now.Hour < 2 && selectedDate == DateTime.Today - TimeSpan.FromDays(1))
                {
                    var tomorrowFirst2Hour = CommonComponent.Current.TB.Logic.GetTimetable(Route, Stop, selectedDate + TimeSpan.FromDays(1)).Where(t => t.Item1.Hour < 2);
                    if (tomorrowFirst2Hour.Count() > 0)
                    {
                        list.Add(new TimeTableBodyLabelGroup { Label = (selectedDate == DateTime.Today ? Services.Resources.LocalizedStringOf("TimeTableTomorrow") : Services.Resources.LocalizedStringOf("TimeTableNextDay")) + Services.Resources.LocalizedStringOf("TimeTableLabelPost") });
                        list.AddRange(createListFromTimetable(tomorrowFirst2Hour, nextArrival));
                    }
                }
            }
            //görgessünk a következő indulás járata fölé
            var nextHour = list.SingleOrDefault(hour => hour.Trips.Any(t => t.Time == nextArrival));
            TimeTableBodyHourGroup scrollTarget = null;
            if (nextHour != null)
            {
                int index = Math.Max(0, list.IndexOf(nextHour) - 1);
                while (index > 0 && list[index] is TimeTableBodyLabelGroup)
                    index--;
                scrollTarget = list[index];
            }
            else
                scrollTarget = null;
            return new TimeTableBodySource { HourList = list, ScrollTarget = scrollTarget };
        }

        private List<TimeTableBodyHourGroup> createListFromTimetable(IEnumerable<Tuple<DateTime, Trip>> timeTable, DateTime nextArrival)
        {
            return timeTable.GroupBy(x => x.Item1.Hour).Select(hour => new TimeTableBodyHourGroup
            {
                Hour = hour.Key,
                Trips = hour.Select(x => new TimeTableBodyMinute
                {
                    Time = x.Item1,
                    Trip = x.Item2,
                    BorderColor = x.Item1 == nextArrival ? nextTimeBg : transparentBg,
                    TextColor = x.Item1 < nextArrival ? beforeTextFg : appForegroundBrush
                }).ToList()
            }).ToList();
        }
        #endregion
    }

    
    public class TimeTableBodyMinute : Bindable
    {
        public object BorderColor { get; set; }
        public object TextColor { get; set; }
        public Trip Trip { get; set; }
        public DateTime Time { get; set; }
        public bool IsSelected { get { return Get<bool>(); } set { Set(value); } }

        public int TimeMin { get { return Time.Minute; } }
    }

    public class TimeTableBodyHourGroup
    {
        public int Hour { get; set; }
        public List<TimeTableBodyMinute> Trips { get; set; }

        public object BackgroundColor
        {
            get { return Hour % 2 == 0 ? CommonComponent.Current.Services.Resources.ColorOf("AppBackgroundBrush") : CommonComponent.Current.Services.Resources.ColorOf("AppSecondaryBackgroundBrush"); }
        }
    }

    public class TimeTableBodyLabelGroup : TimeTableBodyHourGroup
    {
        public string Label { get; set; }

        public TimeTableBodyLabelGroup() { Hour = -1; Trips = new List<TimeTableBodyMinute>(); }
    }

    public class TimeTableBodySource : List<TimeTableBodyHourGroup>
    {
        public List<TimeTableBodyHourGroup> HourList
        {
            get { return this; }
            set { Clear(); AddRange(value); }
        }
        public TimeTableBodyHourGroup ScrollTarget;
    }
}
