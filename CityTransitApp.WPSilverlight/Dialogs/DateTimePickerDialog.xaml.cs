using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.WPSilverlight.Resources;
using System.Globalization;
using CityTransitApp.WPSilverlight.Tools;
using TransitBase;
using System.Windows.Media;
using PlannerComponent.Interface;

namespace CityTransitApp.WPSilverlight.Dialogs
{
    public partial class DateTimePickerDialog : PhoneApplicationPage
    {
        private PlanningTimeType Type;
        private bool isAmPm;

        public DateTimePickerDialog()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            //LayoutRoot.Background.Opacity = 0.9;
        }
        private void SetSize(bool isAmPm)
        {
            double width = Application.Current.Host.Content.ActualWidth;
            double itemHeight = width / 4;
            if (isAmPm) width -= 100;
            DaySelector.ItemSize = new Size(width / 2, itemHeight);
            HourSelector.ItemSize = new Size(width / 4, itemHeight);
            MinuteSelector.ItemSize = new Size(width / 4, itemHeight);
            AmPmSelector.ItemSize = new Size(100, itemHeight);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string param = "";
            NavigationContext.QueryString.TryGetValue("time", out param);
            DateTime time = DateTime.Parse(param);
            while (time.Minute % 5 != 0)
                time += TimeSpan.FromMinutes(1);
            NavigationContext.QueryString.TryGetValue("departure", out param);
            Type = param == "1" ? PlanningTimeType.Departure : PlanningTimeType.Arrival;
            setTypeBorders();

            isAmPm = time.ToString("t").Contains('M');
            DaySelector.DataSource = new NextDatesDataSource(time);
            HourSelector.DataSource = new HourDataSource(time);
            MinuteSelector.DataSource = new MinuteDataSource(time.Minute);
            if (isAmPm)
            {
                AmPmSelector.DataSource = new AmPmDataSource(time.IsPM() ? "PM" : "AM");
                AmPmSelector.Visibility = Visibility.Visible;
            }
            SetSize(isAmPm);

            DaySelector.DataSource.SelectionChanged += (sender, args) => DaySelector.IsExpanded = false;
            HourSelector.DataSource.SelectionChanged += (sender, args) => HourSelector.IsExpanded = false;
            MinuteSelector.DataSource.SelectionChanged += (sender, args) => MinuteSelector.IsExpanded = false;
            if (AmPmSelector.DataSource != null)
                AmPmSelector.DataSource.SelectionChanged += (sender, args) => AmPmSelector.IsExpanded = false;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (Close != null)
                Close(this, null);
        }

        public static event EventHandler<DateTimeModel> Close;

        private void Ok_Click(object sender, EventArgs e)
        {
            if (Close != null)
            {
                DateTime date = (DaySelector.DataSource.SelectedItem as DateWrapper).Value.Date;
                int hour = ((HourWrapper)HourSelector.DataSource.SelectedItem).Hour;
                int min = (MinuteSelector.DataSource.SelectedItem as MinuteWrapper).Value;
                if (isAmPm)
                {
                    hour %= 12;
                    if (((string)AmPmSelector.DataSource.SelectedItem) == "PM")
                        hour += 12;
                }
                Close(this, new DateTimeModel { DateTime = date + new TimeSpan(hour, min, 0), Type = this.Type });
            }
            NavigationService.GoBack();
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            if (Close != null)
                Close(this, null);
            NavigationService.GoBack();
        }

        private void DepartureBorder_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Type = PlanningTimeType.Departure;
            setTypeBorders();
        }

        private void ArriveBorder_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Type = PlanningTimeType.Arrival;
            setTypeBorders();
        }

        private void setTypeBorders()
        {
            if (Type == PlanningTimeType.Departure)
            {
                DepartureBorder.Background = (Brush)App.Current.Resources["PhoneAccentBrush"];
                ArriveBorder.Background = (Brush)App.Current.Resources["PhoneChromeBrush"];
            }
            else
            {
                DepartureBorder.Background = (Brush)App.Current.Resources["PhoneChromeBrush"];
                ArriveBorder.Background = (Brush)App.Current.Resources["PhoneAccentBrush"];
            }
        }

        private void BuildLocalizedApplicationBar()
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = "OK";
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = AppResources.RouteMenuCancel;
        }
    }

    public class DateTimeModel
    {
        public DateTime DateTime { get; set; }
        public PlanningTimeType Type { get; set; }
    }

    #region Looping selector datasources
    public class DateWrapper
    {
        public static DateWrapper Create(DateTime date) { return new DateWrapper { Value = date.Date }; }
        public DateTime Value { get; set; }
        public string Header {
            get
            {
                if (Value == DateTime.Today) return AppResources.DateTimeToday;
                else if (Value == DateTime.Today + TimeSpan.FromDays(1))
                    return AppResources.DateTimeTomorrow;
                return Value.ToString("dddd", CultureInfo.CurrentCulture);
            }
        }
        public string Body { get { return Value.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM"), CultureInfo.CurrentUICulture); } } //MMM d.
    }
    public class NextDatesDataSource : LoopingDataSource<DateWrapper>
    {
        public NextDatesDataSource(DateTime date) : base(DateWrapper.Create(date)) { }
        protected override DateWrapper GetNext(DateWrapper relativeTo)
        {
            return DateWrapper.Create(relativeTo.Value + TimeSpan.FromDays(1));
        }
        protected override DateWrapper GetPrevious(DateWrapper relativeTo)
        {
            //nem működik, végtelen ciklusba kerül ettől a UI, ha kiválasztjuk az elemet
            if (relativeTo.Value == DateTime.Today)
                return null;
            return DateWrapper.Create(relativeTo.Value - TimeSpan.FromDays(1));
        }
    }

    public class HourWrapper
    {
        DateTime time;
        public HourWrapper(DateTime time) { this.time = time; }
        public HourWrapper Increment() { return new HourWrapper(time + TimeSpan.FromHours(1)); }
        public HourWrapper Decrement() { return new HourWrapper(time - TimeSpan.FromHours(1)); }
        public override string ToString()
        {
            return time.HourString();
        }
        public int Hour { get { return time.Hour; } }
    }
    public class HourDataSource : LoopingDataSource<HourWrapper>
    {
        public HourDataSource(DateTime time) : base(new HourWrapper(time)) { }
        protected override HourWrapper GetNext(HourWrapper relativeTo)
        {
            return relativeTo.Increment();
        }
        protected override HourWrapper GetPrevious(HourWrapper relativeTo)
        {
            return relativeTo.Decrement();
        }
    }

    public class MinuteWrapper
    {
        public int Value { get; private set; }
        public static MinuteWrapper Create(int value) { return new MinuteWrapper { Value = value }; }
        public override string ToString()
        {
            return Value.ToString("D2");
        }
    }
    public class MinuteDataSource : LoopingDataSource<MinuteWrapper>
    {
        public MinuteDataSource(int minute) : base(MinuteWrapper.Create(minute)) { }
        protected override MinuteWrapper GetNext(MinuteWrapper relativeTo)
        {
            if (relativeTo.Value >= 55) return MinuteWrapper.Create(0);
            else return MinuteWrapper.Create(relativeTo.Value + 5);
        }
        protected override MinuteWrapper GetPrevious(MinuteWrapper relativeTo)
        {
            if (relativeTo.Value == 0) return MinuteWrapper.Create(55);
            else return MinuteWrapper.Create(relativeTo.Value - 5);
        }
    }
    public class AmPmDataSource : LoopingDataSource<string>
    {
        public AmPmDataSource(string value) : base(value) { }
        protected override string GetNext(string relativeTo)
        {
            if (relativeTo == "AM")
                return "PM";
            else if (relativeTo == "PM")
                return null;
            else throw new InvalidOperationException();
        }
        protected override string GetPrevious(string relativeTo)
        {
            if (relativeTo == "AM")
                return null;
            else if (relativeTo == "PM")
                return "AM";
            else throw new InvalidOperationException();
        }
    }
    #endregion
}