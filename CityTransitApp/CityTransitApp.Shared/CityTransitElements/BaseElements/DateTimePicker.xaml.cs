using CityTransitApp.CityTransitElements.PageParts;
using CityTransitElements.Effects;
using CityTransitServices.Tools;
using PlannerComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace CityTransitApp.CityTransitElements.BaseElements
{
    public sealed partial class DateTimePicker : UserControl
    {
        public DateTimePicker()
        {
            InitializeComponent();
            Animations.OnMouseColorChange(Root);
            setText();
        }

        public IDateTimePickerDialog CustomDialog { get; set; }

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

        private DateTime time = DateTime.Now;
        public DateTime Time
        {
            get { return time; }
            set
            {
                time = value;
                setText();
            }
        }

        async void DateTimePicker_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CustomDialog != null)
            {
                CustomDialog.DateTimeModel = new DateTimeModel
                {
                    DateTime = Time,
                    Type = TimeType,
                };
                var res = await CustomDialog.ShowAsync();
                if (res != null)
                {
                    TimeType = res.Type;
                    Time = res.DateTime;
                }
            }
        }

        private void setText()
        {
            TypeText.Text = (TimeType == PlanningTimeType.Departure ? App.Common.Services.Resources.LocalizedStringOf("DateTimeDeparture") : App.Common.Services.Resources.LocalizedStringOf("DateTimeArrive")) + ":";
            DateTimeText.Text = Time.ToRelativeString();
        }

        private Page GetPage()
        {
            FrameworkElement e = this;
            while (!(e is Page))
                e = e.Parent as FrameworkElement;
            return e as Page;
        }
    }

    public interface IDateTimePickerDialog
    {
        Task<DateTimeModel> ShowAsync();
        DateTimeModel DateTimeModel { get; set; }
    }
}
