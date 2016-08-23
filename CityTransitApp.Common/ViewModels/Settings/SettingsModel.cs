using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.Common.ViewModels.Settings
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public static SettingsModel Current { get; private set; }
        static SettingsModel() { Current = new SettingsModel(); }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void propertyChanged(params string[] names)
        {
            if (Current != null && Current.PropertyChanged != null)
                foreach (string name in names)
                    Current.PropertyChanged(Current, new PropertyChangedEventArgs(name));
        }

        //public static ThemeContrainer[] ThemeValues { get { return ThemeContrainer.All; } }
        public static DayInterval[] IntervalValues { get { return DayInterval.IntervalValues; } }

        public class DayInterval
        {
            private static DayInterval[] intervalValues = new DayInterval[] { new DayInterval(1), new DayInterval(3), new DayInterval(7) };
            public static DayInterval[] IntervalValues { get { return intervalValues; } }

            public int Value;
            public DayInterval(int val) { Value = val; }
            public static DayInterval Get(int val) { return intervalValues.First(x => x.Value == val); }

            public override string ToString()
            {
                switch (Value)
                {
                    case 1: return CommonComponent.Current.Services.Resources.LocalizedStringOf("SettingsPerDay");
                    case 7: return CommonComponent.Current.Services.Resources.LocalizedStringOf("SettingsPerWeek");
                    default: return StringFactory.Format(CommonComponent.Current.Services.Resources.LocalizedStringOf("SettingsPerXDay"), Value);
                }
            }
        }

        //public static bool IsSettingsInitialized { get { return CommonComponent.Current.Services.Settings.Contains("SettingsInitialized"); } }

        public static void InitializeSettings()
        {
            checkValueInitialized("SettingsInitialized", true);
            checkValueInitialized("Theme", "Light");
            checkValueInitialized("AutomaticUpdateCheck", true);
            checkValueInitialized("AutomaticUpdateInterval", 1);
            checkValueInitialized("LocationConsent", false);
            checkValueInitialized("AutomaticNearSearch", true);
            checkValueInitialized("WheelchairUnderlined", false);
        }

        private static void checkValueInitialized<TVal>(string name, TVal defaultValue)
        {
            if (!CommonComponent.Current.Services.Settings.ContainsKey(name))
                CommonComponent.Current.Services.Settings[name] = defaultValue;
        }

        //public static ThemeContrainer Theme
        //{
        //    get
        //    {
        //        string storedValue = (string)CommonComponent.Current.Services.Settings["Theme"];
        //        return ThemeContrainer.Get(storedValue);
        //    }
        //    set
        //    {
        //        CommonComponent.Current.Services.Settings["Theme"] = value.ToString();
        //        propertyChanged("Theme");
        //        SettingsModelCurrent.ThemeMsg.Visibility = Visibility.Visible;
        //    }
        //}

        public static bool AutomaticUpdateCheck
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["AutomaticUpdateCheck"];
                //trySetLocalizedSwitchText(value, () => Current.ToggleSwitch);
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["AutomaticUpdateCheck"] = value;
                propertyChanged("AutomaticUpdateCheck");
            }
        }

        public static bool WheelchairUnderlined
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["WheelchairUnderlined"];
                //trySetLocalizedSwitchText(value, () => Current.WheelchairSwitch);
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["WheelchairUnderlined"] = value;
                propertyChanged("WheelchairUnderlined");
            }
        }

        public static DayInterval AutomaticUpdateInterval
        {
            get
            {
                return DayInterval.Get(AppFields.GetAutomaticUpdateIntervalInDays());
            }
            set
            {
                CommonComponent.Current.Services.Settings["AutomaticUpdateInterval"] = (value as DayInterval).Value;
                propertyChanged("AutomaticUpdateInterval");
            }
        }


        public static bool LocationServices
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["LocationConsent"];
                //trySetLocalizedSwitchText(value, () => Current.LocationSwitch);
                return value;
            }
            set
            {
                CommonComponent.Current.Services.Settings["LocationConsent"] = value;
                propertyChanged("LocationServices", "AutomaticNearSearch");
            }
        }

        public static bool AutomaticNearSearch
        {
            get
            {
                bool value = (bool)CommonComponent.Current.Services.Settings["AutomaticNearSearch"];
                //trySetLocalizedSwitchText(value && LocationServices, () => Current.NearSearchSwitch);
                return value && LocationServices;
            }
            set
            {
                CommonComponent.Current.Services.Settings["AutomaticNearSearch"] = value;
                propertyChanged("AutomaticNearSearch");
            }
        }
    }
}
