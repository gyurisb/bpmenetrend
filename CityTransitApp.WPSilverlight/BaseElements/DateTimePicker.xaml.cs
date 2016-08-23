using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.WPSilverlight.Effects;
using CityTransitApp.WPSilverlight.Resources;
using CityTransitApp.WPSilverlight.Dialogs;
using PlannerComponent.Interface;

namespace CityTransitApp.WPSilverlight.BaseElements
{
    public partial class DateTimePicker : UserControl
    {
        public DateTimePicker()
        {
            InitializeComponent();
            Animations.OnMouseColorChange(Root);
            Time = DateTime.Now;
        }

        public PlanningTimeType timeType;
        public PlanningTimeType TimeType
        {
            get { return timeType; }
            set
            {
                timeType = value;
                setText();
            }
        }

        private DateTime time;
        public DateTime Time
        {
            get { return time; }
            set
            {
                time = value;
                setText();
            }
        }

        private void DateTimePicker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DateTimePickerDialog.Close += DateTimePickerPage_Close;
            GetPage().NavigationService.Navigate(new Uri("/Dialogs/DateTimePickerDialog.xaml?time=" + Time.ToString() + "&departure=" + (TimeType == PlanningTimeType.Departure ? "1" : "0"), UriKind.Relative));
        }

        private void DateTimePickerPage_Close(object sender, DateTimeModel model)
        {
            DateTimePickerDialog.Close -= DateTimePickerPage_Close;
            if (model != null)
            {
                TimeType = model.Type;
                Time = model.DateTime;
            }
        }

        private void setText()
        {
            TypeText.Text = (TimeType == PlanningTimeType.Departure ? AppResources.DateTimeDeparture : AppResources.DateTimeArrive) + ": ";
            DateTimeText.Text = Time.ToRelativeString();
        }

        private PhoneApplicationPage GetPage()
        {
            FrameworkElement e = this;
            while (!(e is PhoneApplicationPage))
                e = e.Parent as FrameworkElement;
            return e as PhoneApplicationPage;
        }
    }
}
