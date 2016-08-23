using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.Common.ViewModels.Settings
{
    public class PlanSettingsModel : INotifyPropertyChanged
    {
        public static PlanSettingsModel Current { get; private set; }
        static PlanSettingsModel() { Current = new PlanSettingsModel(); }

        public static Dictionary<double, double> WalkSpeedTranslation = new Dictionary<double, double>()
        {
            { 1.4, 3.0 },
            { 5.0, 5.0 },
            { 8.5, 10.0 }
        };

        public static void InitializeSettings()
        {
            checkValueInitialized("WalkingSpeed", 5.0);
            checkValueInitialized("MetroAllowed", true);
            checkValueInitialized("UrbanTrainAllowed", true);
            checkValueInitialized("TramAllowed", true);
            checkValueInitialized("BusAllowed", true);
            checkValueInitialized("WheelchairAccessibleTrips", false);
            checkValueInitialized("WheelchairAccessibleStops", false);
        }

        private static void checkValueInitialized<TVal>(string name, TVal defaultValue)
        {
            if (!CommonComponent.Current.Services.Settings.ContainsKey(name))
                CommonComponent.Current.Services.Settings[name] = defaultValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void propertyChanged(params string[] names)
        {
            if (Current.PropertyChanged != null)
                foreach (string name in names)
                    Current.PropertyChanged(Current, new PropertyChangedEventArgs(name));
        }

        public static double WalkingSpeed
        {
            get
            {
                return (double)CommonComponent.Current.Services.Settings["WalkingSpeed"];
            }
            set
            {
                CommonComponent.Current.Services.Settings["WalkingSpeed"] = value;
                propertyChanged("WalkingSpeed");
            }
        }
        public static double WalkingSpeedSlider
        {
            get
            {
                double walkSpeed = WalkingSpeed;
                return WalkSpeedTranslation.Single(x => x.Value == walkSpeed).Key;
            }
            set
            {
                if (WalkSpeedTranslation.ContainsKey(value))
                {
                    WalkingSpeed = WalkSpeedTranslation[value];
                    propertyChanged("WalkingSpeedSlider");
                }
            }
        }

        public static bool MetroAllowed
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["MetroAllowed"];
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["MetroAllowed"] = value;
                propertyChanged("MetroAllowed");
            }
        }
        public static bool UrbanTrainAllowed
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["UrbanTrainAllowed"];
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["UrbanTrainAllowed"] = value;
                propertyChanged("UrbanTrainAllowed");
            }
        }
        public static bool TramAllowed
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["TramAllowed"];
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["TramAllowed"] = value;
                propertyChanged("TramAllowed");
            }
        }
        public static bool BusAllowed
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["BusAllowed"];
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["BusAllowed"] = value;
                propertyChanged("BusAllowed");
            }
        }


        public static bool WheelchairAccessibleTrips
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["WheelchairAccessibleTrips"];
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["WheelchairAccessibleTrips"] = value;
                propertyChanged("WheelchairAccessibleTrips");
            }
        }

        public static bool WheelchairAccessibleStops
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["WheelchairAccessibleStops"];
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["WheelchairAccessibleStops"] = value;
                propertyChanged("WheelchairAccessibleStops");
            }
        }

        //private void Check_Click(object sender, EventArgs e)
        //{
        //    NavigationService.GoBack();
        //}


        //private void OrderHelp_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    new MessageDialog("App.Common.Services.Localization.StringOf("PlanSettingsHelp")").ShowAsync();
        //}

        //private void BuildLocalizedApplicationBar()
        //{
        //    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = App.Common.Services.Localization.StringOf("RouteMenuOK");
        //}

        public static double WalkingSpeedInMps { get { return WalkingSpeed / 3.6; } }
    }
}
