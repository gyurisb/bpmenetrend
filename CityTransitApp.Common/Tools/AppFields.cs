using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitServices.Tools
{
    public class AppFields
    {
        //public static async void Migrate()
        //{
        //    var oldSettingsFile = await ApplicationData.Current.LocalFolder.GetFileAsync("__ApplicationSettings");
        //    if (oldSettingsFile != null)
        //    {
        //        var storageValues = await GetIsolatedStorageValuesAsync();
        //        foreach (var value in storageValues)
        //            CommonComponent.Current.Services.Settings[value.Key] = value.Value;
        //    }
        //}

        public static void Initialize()
        {
            checkValueInitialized("UpdateAvailable", false);
            checkValueInitialized("LastDailyCheck", DateTime.Today.ToBinary());
            checkValueInitialized("OfflinePlanningPurchused", false);
            checkValueInitialized("DbUpdateVersion", Config.Current.DBNumber);
            checkValueInitialized("ApplicationStarted", DateTime.Now.ToBinary());
            //checkValueInitialized("AcquireLogProgress", -1);
            //checkValueInitialized("AppAcquired", false);
            checkValueInitialized("AcquisitionType", -1);
            checkValueInitialized("AcquisitionLoggedType", -2);
            checkValueInitialized("LastUpdateCheck", DateTime.MinValue.ToBinary());
            
            //CommonComponent.Current.Services.Settings.Save();
        }
        private static void checkValueInitialized<TVal>(string name, TVal defaultValue)
        {
            if (!CommonComponent.Current.Services.Settings.ContainsKey(name))
                CommonComponent.Current.Services.Settings[name] = defaultValue;
        }

        public static bool ForceUpdate
        {
            get { return CommonComponent.Current.Services.Settings.ContainsKey("ForceUpdate"); }
            set
            {
                if (value == true)
                {
                    if (!ForceUpdate)
                        CommonComponent.Current.Services.Settings["ForceUpdate"] = true;
                }
                else
                {
                    if (ForceUpdate)
                        CommonComponent.Current.Services.Settings.Remove("ForceUpdate");
                }
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        public static string VersionId
        {
            get
            {
                if (!CommonComponent.Current.Services.Settings.ContainsKey("VersionId"))
                    return null;
                return (string)CommonComponent.Current.Services.Settings["VersionId"];
            }
            set
            {
                CommonComponent.Current.Services.Settings["VersionId"] = value;
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        public static bool PlanningTrialExpired
        {
            get
            {
                if (!CommonComponent.Current.Services.Settings.ContainsKey("TrialStart"))
                {
                    CommonComponent.Current.Services.Settings["TrialStart"] = DateTime.Now.ToBinary();
                    CommonComponent.Current.Services.Settings["TrialExpired"] = false;
                    //CommonComponent.Current.Services.Settings.Save();
                    return false;
                }
                else
                {
                    if ((bool)CommonComponent.Current.Services.Settings["TrialExpired"])
                        return true;
                    DateTime trialStart = DateTime.FromBinary((long)CommonComponent.Current.Services.Settings["TrialStart"]);
                    if (!(trialStart + TimeSpan.FromDays(1) >= DateTime.Now && trialStart <= DateTime.Now))
                    {
                        CommonComponent.Current.Services.Settings["TrialExpired"] = true;
                        //CommonComponent.Current.Services.Settings.Save();
                        return true;
                    }
                    return false;
                }
            }
            set
            {
                if (value)
                    CommonComponent.Current.Services.Settings["TrialExpired"] = true;
                else
                {
                    CommonComponent.Current.Services.Settings["TrialExpired"] = false;
                    CommonComponent.Current.Services.Settings.Remove("TrialStart");
                }
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        public static DateTime LastDailyCheck
        {
            get
            {
                return DateTime.FromBinary((long)CommonComponent.Current.Services.Settings["LastDailyCheck"]);
            }
            set
            {
                CommonComponent.Current.Services.Settings["LastDailyCheck"] = value.ToBinary();
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        public static bool OfflinePlanningPurchused
        {
            get
            {
                return (bool)CommonComponent.Current.Services.Settings["OfflinePlanningPurchused"];
            }
            set
            {
                CommonComponent.Current.Services.Settings["OfflinePlanningPurchused"] = value;
                //CommonComponent.Current.Services.Settings.Save();
            }
        }


        public static DateTime LastUpdateCheck
        {
            get
            {
                return DateTime.FromBinary((long)CommonComponent.Current.Services.Settings["LastUpdateCheck"]);
            }
            set
            {
                CommonComponent.Current.Services.Settings["LastUpdateCheck"] = value.ToBinary();
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        public static bool UpdateAvailable
        {
            get
            {
                return (bool)CommonComponent.Current.Services.Settings["UpdateAvailable"];
            }
            set
            {
                CommonComponent.Current.Services.Settings["UpdateAvailable"] = value;
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        public static int DbUpdateVersion
        {
            get
            {
                return (int)CommonComponent.Current.Services.Settings["DbUpdateVersion"];
            }
            set
            {
                CommonComponent.Current.Services.Settings["DbUpdateVersion"] = value;
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        public static DateTime ApplicationStarted
        {
            get
            {
                DateTime startTime = DateTime.FromBinary((long)CommonComponent.Current.Services.Settings["ApplicationStarted"]);
                if (startTime > DateTime.Now)
                    ApplicationStarted = startTime = DateTime.MinValue;
                return startTime;
            }
            set
            {
                CommonComponent.Current.Services.Settings["ApplicationStarted"] = value.ToBinary();
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        //public static int AcquireLogProgress
        //{
        //    get
        //    {
        //        return (int)CommonComponent.Current.Services.Settings["AcquireLogProgress"];
        //    }
        //    set
        //    {
        //        CommonComponent.Current.Services.Settings["AcquireLogProgress"] = value;
        //        //CommonComponent.Current.Services.Settings.Save();
        //    }
        //}
        //public static bool AppAcquired
        //{
        //    get
        //    {
        //        return (bool)CommonComponent.Current.Services.Settings["AppAcquired"];
        //    }
        //    set
        //    {
        //        CommonComponent.Current.Services.Settings["AppAcquired"] = value;
        //        //CommonComponent.Current.Services.Settings.Save();
        //    }
        //}
        public static int AcquisitionType
        {
            get
            {
                return (int)CommonComponent.Current.Services.Settings["AcquisitionType"];
            }
            set
            {
                CommonComponent.Current.Services.Settings["AcquisitionType"] = value;
                //CommonComponent.Current.Services.Settings.Save();
            }
        }
        public static int AcquisitionLoggedType
        {
            get
            {
                return (int)CommonComponent.Current.Services.Settings["AcquisitionLoggedType"];
            }
            set
            {
                CommonComponent.Current.Services.Settings["AcquisitionLoggedType"] = value;
                //CommonComponent.Current.Services.Settings.Save();
            }
        }

        //public static RouteStopPair LastPage
        //{
        //    get
        //    {
        //        if (!CommonComponent.Current.Services.Settings.ContainsKey("LastPageRouteId"))
        //            return null;
        //        int routeId = (int)CommonComponent.Current.Services.Settings["LastPageRouteId"];
        //        var route = App.Model.GetRouteByID(routeId);
        //        int stopId = (int)CommonComponent.Current.Services.Settings["LastPageStopId"];
        //        var stop = App.Model.GetStopGroupByID(stopId);
        //        return new RouteStopPairImpl { Route = route, Stop = stop };
        //    }
        //    private set
        //    {
        //        CommonComponent.Current.Services.Settings["LastPageRouteId"] = value.Route.ID;
        //        CommonComponent.Current.Services.Settings["LastPageStopId"] = value.Stop.ID;
        //        //CommonComponent.Current.Services.Settings.Save();
        //    }
        //}
        //public static DateTime LastPageTime
        //{
        //    get
        //    {
        //        return (DateTime)CommonComponent.Current.Services.Settings["LastPageTime"];
        //    }
        //    private set
        //    {
        //        CommonComponent.Current.Services.Settings["LastPageTime"] = value;
        //    }
        //}
        //public static void SetLastPage(Route route, StopGroup stop)
        //{
        //    LastPage = new RouteStopPairImpl { Route = route, Stop = stop };
        //    LastPageTime = DateTime.Now;
        //}
        public static int GetAutomaticUpdateIntervalInDays()
        {
            return (int)CommonComponent.Current.Services.Settings["AutomaticUpdateInterval"];
        }

        #region updating mechanism
        //public static async Task<IEnumerable<KeyValuePair<string, object>>> GetIsolatedStorageValuesAsync()
        //{
        //    try
        //    {
        //        using (var fileStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("__ApplicationSettings"))
        //        {
        //            using (var streamReader = new StreamReader(fileStream))
        //            {
        //                var line = streamReader.ReadLine() ?? string.Empty;

        //                var knownTypes = line.Split('\0')
        //                    .Where(x => !string.IsNullOrEmpty(x))
        //                    .Select(Type.GetType)
        //                    .ToArray();

        //                fileStream.Position = line.Length + Environment.NewLine.Length;

        //                var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), knownTypes);

        //                return (Dictionary<string, object>)serializer.ReadObject(fileStream);
        //            }
        //        }
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        // ignore the FileNotFoundException, unfortunately there is no File.Exists to prevent this
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //    return new Dictionary<string, object>();
        //}
        #endregion
    }
}
