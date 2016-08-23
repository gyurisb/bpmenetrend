using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.BusinessLogic.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.BaseElements
{
    public sealed partial class Calendar : UserControl
    {
        public event EventHandler<DateTime> DateSelected;

        private CalendarViewModel viewModel;
        private CalendarViewModel ViewModel
        {
            get { return viewModel; }
            set { this.DataContext = viewModel = value; }
        }

        public Calendar()
        {
            this.InitializeComponent();
            ViewModel = new CalendarViewModel(this);
            SelectedDay = DateTime.Today;
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //void SetValueDp(DependencyProperty property, object value, [System.Runtime.CompilerServices.CallerMemberName] String p = null)
        //{
        //    SetValue(property, value);
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(p));
        //}

        public static readonly DependencyProperty SelectedDayProperty = DependencyProperty.RegisterAttached(
              "SelectedDay",
              typeof(DateTime),
              typeof(Calendar),
              new PropertyMetadata(null, selectedDayValueChanged)
          );

        private static void selectedDayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                ((Calendar)d).setSelectedDay((DateTime)e.NewValue);
        }

        private void setSelectedDay(DateTime dateTime)
        {
            ViewModel.SelectedDay = dateTime;
        }

        public DateTime SelectedDay
        {
            get { return (DateTime)this.GetValue(SelectedDayProperty); }
            set { this.SetValue(SelectedDayProperty, value); }
        }

        private class CalendarViewModel : Bindable
        {
            public Calendar Parent { get; private set; }
            public DateTime SelectedDay
            {
                get { return Get<DateTime>(); }
                set
                {
                    DateTime old = SelectedDay;
                    if (old.Month != value.Month || old.Year != value.Year)
                    {
                        Set(value);
                        SelectedMonth = value;
                    }
                    else Set(value);

                    if (old != value)
                    {
                        var oldItem = days.SelectMany(x => x).FirstOrDefault(x => x.Date == old);
                        var newItem = days.SelectMany(x => x).FirstOrDefault(x => x.Date == value);
                        if (oldItem != null)
                            oldItem.SetStyles();
                        if (newItem != null)
                            newItem.SetStyles();
                    }
                }
            }
            public DateTime SelectedMonth
            {
                get { return Get<DateTime>(); }
                set
                {
                    Set(value);
                    OnPropertyChanged("Days");
                }
            }

            private CalendarItemViewModel[][] days;
            public CalendarItemViewModel[][] Days { get { return this.days = getDays(); } }

            public IList<CalendarHeaderViewModel> Header
            {
                get
                {
                    DateTime firstDayOfWeek = DateTime.Now.Date - TimeSpan.FromDays(DateTime.Now.DayOfWeek - CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek);
                    return Enumerable.Range(0, 7).Select(i => new CalendarHeaderViewModel(firstDayOfWeek + TimeSpan.FromDays(i))).ToList();
                }
            }

            public CalendarViewModel(Calendar calendar)
            {
                this.Parent = calendar;
            }

            private CalendarItemViewModel[][]  getDays()
            {
                var firstDayOfMonth = SelectedMonth - TimeSpan.FromDays(SelectedMonth.Day - 1);
                var firstDayOfFirstWeek = firstDayOfMonth - TimeSpan.FromDays((firstDayOfMonth.DayOfWeek - CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek + 7) % 7);
                var days = Enumerable.Range(0, 6 * 7).Select(i => new CalendarItemViewModel(this, firstDayOfFirstWeek + TimeSpan.FromDays(i))).ToList();
                return Enumerable.Range(0, 6).Select(i => days.GetRange(i * 7, 7).ToArray()).ToArray();
            }
        }
        private class CalendarItemViewModel : Bindable
        {
            private CalendarViewModel outer;
            public CalendarItemViewModel(CalendarViewModel outer, DateTime date)
            {
                this.Date = date;
                this.outer = outer;
                SetStyles();
            }
            public DateTime Date { get; set; }
            public Style ItemContainerBorderStyle { get { return Get<Style>(); } set { Set(value); } }
            public Style ItemContainerTextStyle { get { return Get<Style>(); } set { Set(value); } }

            public void SetStyles()
            {
                string stylePrefix = getStylePrefix();
                ItemContainerBorderStyle = (Style)outer.Parent.Resources[stylePrefix + "BorderStyle"];
                ItemContainerTextStyle = (Style)outer.Parent.Resources[stylePrefix + "TextStyle"];
            }

            private string getStylePrefix()
            {
                if (Date.Month != outer.SelectedMonth.Month || Date.Year != outer.SelectedMonth.Year)
                    return "OtherMonth";
                if (Date == DateTime.Today)
                    return "Today";
                if (Date == outer.SelectedDay)
                    return "Selected";
                return "Default";
            }
        }
        private class CalendarHeaderViewModel
        {
            public CalendarHeaderViewModel(DateTime dateTime) { DayOfWeek = dateTime; }
            public DateTime DayOfWeek { get; set; }
        }

        private void DateBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = (CalendarItemViewModel)((FrameworkElement)sender).DataContext;
            SelectedDay = item.Date;
            if (DateSelected != null)
                DateSelected(this, item.Date);
        }

        private void DecrementBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.SelectedMonth = ViewModel.SelectedMonth.AddMonths(-1);
        }

        private void IncerementBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.SelectedMonth = ViewModel.SelectedMonth.AddMonths(1);
        }

        private void TodayText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectedDay = DateTime.Today;
            if (DateSelected != null)
                DateSelected(this, SelectedDay);
        }
    }
}
