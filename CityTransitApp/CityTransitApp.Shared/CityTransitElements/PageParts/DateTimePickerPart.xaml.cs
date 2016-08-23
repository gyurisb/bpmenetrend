using PlannerComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TransitBase;
using CityTransitServices.Tools;
using System.Globalization;
using Windows.UI;
using System.Collections;
using CityTransitApp.CityTransitElements.BaseElements;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class DateTimePickerPart : UserControl
    {
        private PlanningTimeType Type;
        private bool isAmPm;

        public DateTimePickerPart()
        {
            InitializeComponent();
            //BuildLocalizedApplicationBar();
            //LayoutRoot.Background.Opacity = 0.9;
        }
        private void SetSize(bool isAmPm)
        {
            double width = App.GetAppInfo().GetScreenWidth();
            double itemHeight = width / 4;
            if (isAmPm) width -= 100;
            DaySelector.ItemSize = new Size(width / 2, itemHeight);
            HourSelector.ItemSize = new Size(width / 4, itemHeight);
            MinuteSelector.ItemSize = new Size(width / 4, itemHeight);
            AmPmSelector.ItemSize = new Size(100, itemHeight);
        }

        public DateTimeModel DateTimeModel
        {
            get
            {
                return GetCurrentResult();
            }
            set
            {
                Initialize(value);
            }
        }

        private void Initialize(DateTimeModel model)
        {
            DateTime time = model.DateTime;
            while (time.Minute % 5 != 0)
                time += TimeSpan.FromMinutes(1);
            this.Type = model.Type;
            setTypeBorders();

            isAmPm = time.ToString("t").Contains('M');

            SetSize(isAmPm);

            List<HourWrapper> hourDataSource;

            var dayDataSource = Enumerable.Range(0, 31).Select(i => DateWrapper.Create(DateTime.Today + TimeSpan.FromDays(i))).ToList();
            DaySelector.DataSource = dayDataSource;
            var minuteDataSource = Enumerable.Range(0, 60 / 5).Select(i => (i * 5).ToString("D2")).Select(x => new MinuteWrapper { Value = x }).ToList();
            MinuteSelector.DataSource = minuteDataSource;
            if (isAmPm)
            {
                //AmPmSelector.DataSource = new AmPmDataSource(time.IsPM() ? "PM" : "AM");
                var amPmDataSource = new string[] { "AM", "PM" }.Select(x => new AmPmWrapper { Value = x }).ToList();
                AmPmSelector.DataSource = amPmDataSource;
                AmPmSelector.Visibility = Visibility.Visible;
                AmPmSelector.SelectedItem = time.IsPM() ? amPmDataSource[1] : amPmDataSource[0];

                hourDataSource = new int[] { 12 }.Concat(Enumerable.Range(1, 11)).Select(x => new HourWrapper { Value = x }).ToList();
                HourSelector.DataSource = hourDataSource;
            }
            else
            {
                hourDataSource = Enumerable.Range(0, 24).Select(x => new HourWrapper { Value = x }).ToList();
                HourSelector.DataSource = hourDataSource;
            }
            DaySelector.SelectedItem = dayDataSource.First(x => x.Value.Date == time.Date);
            MinuteSelector.SelectedItem = minuteDataSource.ElementAt(time.Minute / 5);
            HourSelector.SelectedItem = hourDataSource.First(h => h.Value.ToString() == time.HourString());

            //DaySelector.SelectionChanged += (sender, a) => DaySelector.IsExpanded = false;
            //HourSelector.SelectionChanged += (sender, a) => HourSelector.IsExpanded = false;
            //MinuteSelector.SelectionChanged += (sender, a) => MinuteSelector.IsExpanded = false;
            //if (AmPmSelector.DataSource != null)
            //    AmPmSelector.SelectionChanged += (sender, a) => AmPmSelector.IsExpanded = false;
        }

        private DateTimeModel GetCurrentResult()
        {
            DateTime date = (DaySelector.SelectedItem as DateWrapper).Value.Date;
            int hour = ((HourWrapper)HourSelector.SelectedItem).Value;
            int min = int.Parse(((MinuteWrapper)MinuteSelector.SelectedItem).Value);
            if (isAmPm)
            {
                hour %= 12;
                if (((AmPmWrapper)AmPmSelector.SelectedItem).Value == "PM")
                    hour += 12;
            }
            return new DateTimeModel { DateTime = date + new TimeSpan(hour, min, 0), Type = this.Type };
        }

        private void DepartureBorder_Tap(object sender, TappedRoutedEventArgs e)
        {
            LoopingSelector.PerformOtherClick(sender);
            Type = PlanningTimeType.Departure;
            setTypeBorders();
        }

        private void ArriveBorder_Tap(object sender, TappedRoutedEventArgs e)
        {
            LoopingSelector.PerformOtherClick(sender);
            Type = PlanningTimeType.Arrival;
            setTypeBorders();
        }

        private void setTypeBorders()
        {
            if (Type == PlanningTimeType.Departure)
            {
                DepartureBorder.Background = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
                ArriveBorder.Background = (SolidColorBrush)App.Current.Resources["PhoneChromeBrush"];
            }
            else
            {
                DepartureBorder.Background = (SolidColorBrush)App.Current.Resources["PhoneChromeBrush"];
                ArriveBorder.Background = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            }
        }

        //private void BuildLocalizedApplicationBar()
        //{
        //    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = "OK";
        //    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = App.Common.Services.Localization.StringOf("RouteMenuCancel");
        //}
    }
    public class DateTimeModel
    {
        public DateTime DateTime { get; set; }
        public PlanningTimeType Type { get; set; }
    }

    #region Looping selector datasources
    public class DateWrapper : LoopingSelectorItemBase
    {
        public static DateWrapper Create(DateTime date) { return new DateWrapper { Value = date.Date }; }
        public DateTime Value { get; set; }
        public string Header
        {
            get
            {
                if (Value == DateTime.Today) return App.Common.Services.Resources.LocalizedStringOf("DateTimeToday");
                else if (Value == DateTime.Today + TimeSpan.FromDays(1))
                    return App.Common.Services.Resources.LocalizedStringOf("DateTimeTomorrow");
                return Value.ToString("dddd", CultureInfo.CurrentCulture);
            }
        }
        public string Body { get { return Value.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM"), CultureInfo.CurrentUICulture); } } //MMM d.
    }

    public class HourWrapper : LoopingSelectorItemBase
    {
        public int Value { get; set; }
    }

    public class MinuteWrapper : LoopingSelectorItemBase
    {
        public string Value { get; set; }
    }

    public class AmPmWrapper : LoopingSelectorItemBase
    {
        public string Value { get; set; }
    }

    //public class NextDatesDataSource
    //{
    //    public NextDatesDataSource(DateTime date) { }
    //    protected DateWrapper GetNext(DateWrapper relativeTo)
    //    {
    //        return DateWrapper.Create(relativeTo.Value + TimeSpan.FromDays(1));
    //    }
    //    protected DateWrapper GetPrevious(DateWrapper relativeTo)
    //    {
    //        //nem működik, végtelen ciklusba kerül ettől a UI, ha kiválasztjuk az elemet
    //        if (relativeTo.Value == DateTime.Today)
    //            return null;
    //        return DateWrapper.Create(relativeTo.Value - TimeSpan.FromDays(1));
    //    }
    //}

    //public class HourWrapper
    //{
    //    DateTime time;
    //    public HourWrapper(DateTime time) { this.time = time; }
    //    public HourWrapper Increment() { return new HourWrapper(time + TimeSpan.FromHours(1)); }
    //    public HourWrapper Decrement() { return new HourWrapper(time - TimeSpan.FromHours(1)); }
    //    public string ToString()
    //    {
    //        return time.HourString();
    //    }
    //    public int Hour { get { return time.Hour; } }
    //}
    //public class HourDataSource
    //{
    //    public HourDataSource(DateTime time) { }
    //    protected HourWrapper GetNext(HourWrapper relativeTo)
    //    {
    //        return relativeTo.Increment();
    //    }
    //    protected HourWrapper GetPrevious(HourWrapper relativeTo)
    //    {
    //        return relativeTo.Decrement();
    //    }
    //}

    //public class MinuteWrapper
    //{
    //    public int Value { get; private set; }
    //    public static MinuteWrapper Create(int value) { return new MinuteWrapper { Value = value }; }
    //    public string ToString()
    //    {
    //        return Value.ToString("D2");
    //    }
    //}
    //public class MinuteDataSource
    //{
    //    public MinuteDataSource(int minute) { }
    //    protected MinuteWrapper GetNext(MinuteWrapper relativeTo)
    //    {
    //        if (relativeTo.Value >= 55) return MinuteWrapper.Create(0);
    //        else return MinuteWrapper.Create(relativeTo.Value + 5);
    //    }
    //    protected MinuteWrapper GetPrevious(MinuteWrapper relativeTo)
    //    {
    //        if (relativeTo.Value == 0) return MinuteWrapper.Create(55);
    //        else return MinuteWrapper.Create(relativeTo.Value - 5);
    //    }
    //}
    //public class AmPmDataSource
    //{
    //    public AmPmDataSource(string value) { }
    //    protected string GetNext(string relativeTo)
    //    {
    //        if (relativeTo == "AM")
    //            return "PM";
    //        else if (relativeTo == "PM")
    //            return null;
    //        else throw new InvalidOperationException();
    //    }
    //    protected string GetPrevious(string relativeTo)
    //    {
    //        if (relativeTo == "AM")
    //            return null;
    //        else if (relativeTo == "PM")
    //            return "AM";
    //        else throw new InvalidOperationException();
    //    }
    //}
    #endregion
}
