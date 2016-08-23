using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.Entities;
using CityTransitApp;

namespace CityTransitApp.Common
{
    public class CategoryTree : Dictionary<string, object> { }
    public delegate IEnumerable<RouteGroup> RouteSelector();
    public enum PlanningType { Normal = 0, FastBeforeNormal, OnlyFast };
    public enum DefaultPageType { TimetablePage, TripPage };
    public delegate double CustomTypePriorityType(RouteGroup group);

    public class Config
    {
        protected string DownloadSourceUrl;
        protected string UpdatesUrl;
        public string CheckUrl;
        public Uri DefaultDatabase = new Uri("ms-appx:///Assets/database-266.zip");
        public string StatisticsUrl = "http://jbosswildflyuseast-globaltransit.rhcloud.com/";

        public bool Advertising = false;
        public string ApplicationID;
        public string MainAdUnitID;
        public string TimetableAdUnitID;
        public bool IAPOfflinePlanning = false;
        public int TrialDurationInDays = 0;
        public bool IsFree = false;

        public int BigTableLimit;
        public double LatitudeDegreeDistance;   //calculator: http://www.csgnetwork.com/degreelenllavcalc.html
        public double LongitudeDegreeDistance;
        public GeoCoordinate CenterLocation;
        public CategoryTree CategoryTree;
        public CustomTypePriorityType CustomTypePriority;
        public DefaultPageType DefaultRouteStopPage;
        public bool CheckSameRoutes = false;
        public bool FilterTravelRouteStops = false;

        public Func<string, double> TextSizeRateCalculator = null;
        public Func<RouteGroup, bool> HasLabelPred = x => false;
        public Func<RouteGroup, string> LabelTextPred = x => "";
        public Func<RouteGroup, string> NameTextPred = x => x.Name;
        public HashSet<string> LongNameExceptions = null;

        public bool FastPlanning = false;
        public PlanningType PlanningType;
        public double HighStopsMapLevel;
        public double LowStopsMapLevel;
        public int HighStopsRadius;
        public int LowStopsRadius;  
        public bool BarrierFreeOptions = false;
        public int PlanningAspectsCount = 3;
        public string MapApplicationID = "88c7c60d-ab09-4d9c-8e1c-d3caa8ce8ae6";
        public string MapAuthenticationToken = "9A1_YZguPYboUC4DjnLzJQ";
        public string Version = "2.6.6";
        public int DBNumber = 0;
        public string CheckLine = "bpMenetrend2014GyurisBence";
        public int AppNumber;
        public int UBVersion = 2;

        public string GetDownloadSource(int updatesVersion) { return DownloadSourceUrl.Replace("{Version}", Version.Replace(".", "")).Replace("{Number}", updatesVersion.ToString()); }
        public string GetUpdatesUrl() { return UpdatesUrl.Replace("{Version}", Version.Replace(".", "")); }

        public static Config Current = null;
        //static Config()
        //{
        //    Current = Create(AppName());
        //}
        public static void Initialize()
        {
            if (Current == null)
                Current = Create(AppName());
        }

        public static Config Create(String appName)
        {
            switch (appName)
            {
                case "Bp Menetrend": return CreateBudapest();
                case "NY Transit: Offline": return CreateNewYork();
                case "Chicago Transit Offline": return CreateChicago();
                case "DC Transit: Offline": return CreateWashington();
                case "LA Transit: Offline": return CreateLosAngeles();
                default: throw new ArgumentException();
            }
        }
        private static string AppName()
        {
            //return XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Title").Value;
            return "Bp Menetrend";
        }
        private static TransitBaseComponent TB { get { return TransitBaseComponent.Current; } }

        private static Config CreateBudapest()
        {
            Func<RouteGroup, bool> isNightBus = b => b.Name.StartsWith("9") && b.Name.Cast<char>().Count(ch => Char.IsNumber(ch)) == 3;
            Func<RouteGroup, bool> isTroli = b => b.BgColor == 0xFF1609 || b.BgColor == 0xE41F18;

            return new Config
            {
                IsFree = true,
                AppNumber = 100,
                DownloadSourceUrl = "http://users.hszk.bme.hu/~gb1120/database-{Version}-{Number}.zip",
                UpdatesUrl = "http://users.hszk.bme.hu/~gb1120/updates-bp-{Version}.txt",
                CheckUrl = "http://users.hszk.bme.hu/~gb1120/check.txt",
                DBNumber = 6,
                //ApplicationID = "681330e0-9c1b-4a1c-9e89-104d4baa9dbd",
                //MainAdUnitID = "10806105",
                //TimetableAdUnitID = "10806106",
                IAPOfflinePlanning = false,
                BigTableLimit = 3,
                LatitudeDegreeDistance = 111180.5537835114, //latitude = 47.5
                LongitudeDegreeDistance = 75343.5388426138,
                BarrierFreeOptions = true,
                CategoryTree = new CategoryTree() {
                    { "Metró;Metro.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Metro)) },
                    { "Hév;UrbanTrain.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.RailRoad)) },
                    { "Villamos;Tram.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Tram)) },
                    { "Trolibusz;Troli.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(isTroli)) },
                    { "Busz;Bus.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(b => !isTroli(b) && !isNightBus(b))) },
                    { "Éjszakai;Night.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(isNightBus)) },
                },
                CustomTypePriority = rg => rg.Type == RouteType.Bus && isNightBus(rg) ? RouteType.Bus + 0.1 : rg.TypePriority,
                DefaultRouteStopPage = DefaultPageType.TimetablePage,
                CenterLocation = new GeoCoordinate(47.497853, 19.040319),
                HighStopsMapLevel = 12,
                LowStopsMapLevel = 15,
                HighStopsRadius = 2000,
                LowStopsRadius = 1000,
                CheckSameRoutes = true,
                FilterTravelRouteStops = true,
            };
        }

        private static Config CreateNewYork()
        {
            return new Config
            {
                AppNumber = 1,
                TrialDurationInDays = 3,
                DownloadSourceUrl = "http://drysj6b8xu282.cloudfront.net/database-ny-{Version}-{Number}.zip",
                UpdatesUrl = "http://drysj6b8xu282.cloudfront.net/updateslive-ny-{Version}.txt",
                MapApplicationID = "3ce44f71-e435-4684-aaf1-21f215f1ed48",
                MapAuthenticationToken = "QYYcnXzrAFnLSWWHVG3Zhg",
                FastPlanning = false,
                PlanningType = PlanningType.FastBeforeNormal,
                PlanningAspectsCount = 3,
                BigTableLimit = 2,
                LatitudeDegreeDistance = 111049.0506675487, //feet: 364333.42702809966 //latitude = 40.75
                LongitudeDegreeDistance = 84452.25134526618, //feet: 277073.76126044337,
                CategoryTree = new CategoryTree() {
                    { "Subway;subway.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Metro)) },
                    { "Railroad;train.png",  new CategoryTree() {
                            { "LIRR", (RouteSelector)( () => TB.Logic.FindAgency("LI").Routes ) },
                            { "MNR", (RouteSelector)(() => TB.Logic.GetCategory(RouteType.RailRoad).Where(r => Regex.IsMatch(r.Agency.ShortName, "[0-9]")) ) },
                            { "SIR", (RouteSelector)(() => TB.RouteGroups.Where(r => r.Name == "Sir") ) },
                    }},
                    { "Bus;bus-ny.png",  new CategoryTree() {
                            { "Bronx", (RouteSelector)( () => TB.Logic.GetCategory(RouteType.Bus).Where(r => Regex.IsMatch(r.Name, "^Bx[0-9]+")) ) },
                            { "Brooklyn", (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(r => Regex.IsMatch(r.Name, "^B[0-9]+")) ) },
                            { "Manhattan", (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(r => Regex.IsMatch(r.Name, "^M[0-9]+")) ) },
                            { "Queens", (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(r => Regex.IsMatch(r.Name, "^Q[0-9]+")) ) },
                            { "Staten Island", (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(r => Regex.IsMatch(r.Name, "^S[0-9]+")) ) },
                            { "Express", (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Bus).Where(r => Regex.IsMatch(r.Name, "^X[0-9]+")) ) },
                    }},
                    { "Bus Co.;bus-co.png",  (RouteSelector)(() => TB.Logic.FindAgency("MTABC").Routes) },
                },
                DefaultRouteStopPage = DefaultPageType.TripPage,
                CenterLocation = new GeoCoordinate(40.712705, -74.005932),
                HighStopsMapLevel = 14,
                LowStopsMapLevel = 16,
                HighStopsRadius = 1000,
                LowStopsRadius = 500,
                TextSizeRateCalculator = delegate(string text)
                {
                    int length = text.Length;
                    if (text.StartsWith("M") || text.StartsWith("Q") || text.StartsWith("BxM") || text.StartsWith("BM"))
                    {
                        if (length == 3)
                            return 22.0 / 30.0;
                        else if (length == 4)
                        {
                            if (char.IsLetter(text.Last()) || char.IsLetter(text[1]))
                                return 19.0 / 22.0;
                            return 21.0 / 22.0;
                        }
                        else if (length == 5)
                            return 20.0 / 21.0;
                    }
                    return 1.0;
                },
                HasLabelPred = r => r.Name.EndsWith("-SBS"),
                LabelTextPred = r => r.Name.EndsWith("-SBS") ? "SBS" : "",
                NameTextPred = r => r.Name.EndsWith("-SBS") ? r.Name.Substring(0, r.Name.Length - 4) : r.Name,
            };
        }

        public static Config CreateChicago()
        {
            return new Config
            {
                AppNumber = 3,
                CenterLocation = new GeoCoordinate(41.878139, -87.628631),
                LatitudeDegreeDistance = 111070.8886574498,
                LongitudeDegreeDistance = 83008.61752467098,
                TrialDurationInDays = 3,
                BigTableLimit = 2,
                DefaultRouteStopPage = DefaultPageType.TripPage,
                HighStopsMapLevel = 14,
                LowStopsMapLevel = 16,
                HighStopsRadius = 1000,
                LowStopsRadius = 500,
                CategoryTree = new CategoryTree() {
                    { "L trains;subway.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Metro)) },
                    { "Metra;train.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.RailRoad)) },
                    { "CTA Bus;bus-ny.png",  (RouteSelector)(() => TB.Logic.FindAgency("CTA").Routes.Where(r => r.Type == RouteType.Bus)) },
                    { "pace;bus-co.png",  (RouteSelector)(() => TB.Logic.FindAgency("PACE").Routes) },
                },
                DownloadSourceUrl = "http://drysj6b8xu282.cloudfront.net/database-chi-{Version}-{Number}.zip",
                UpdatesUrl = "http://drysj6b8xu282.cloudfront.net/updates-chi-{Version}.txt",
                MapApplicationID = null,
                MapAuthenticationToken = null,
                HasLabelPred = r => r.Name.Cast<char>().Contains('-'),
                LabelTextPred = r => r.Name.Cast<char>().Contains('-') ? r.Name.Split('-').Last() : "",
                NameTextPred = r => r.Name.Cast<char>().Contains('-') ? r.Name.Split('-').First() : r.Name,
                TextSizeRateCalculator = delegate(string text)
                {
                    if (text == "UP" || text == "MD")
                        return 20.0 / 30.0;
                    if (text.Length == 3 && text.Cast<char>().All(ch => char.IsUpper(ch)))
                        return 25.0 / 30.0;
                    return 1.0;
                },
            };
        }

        public static Config CreateWashington()
        {
            virginiaNames = new HashSet<string>(new string[] { "Metroway", "REX", "TAGS", "Shuttle" });
            return new Config
            {
                AppNumber = 2,
                CenterLocation = new GeoCoordinate(38.907258, -77.036526),
                LatitudeDegreeDistance = 111013.68585541553,
                LongitudeDegreeDistance = 86739.3448851524,
                IsFree = true, //JELENLEG ingyenes!!
                TrialDurationInDays = 3,
                BigTableLimit = 3,
                DefaultRouteStopPage = DefaultPageType.TripPage,
                HighStopsMapLevel = 12,
                LowStopsMapLevel = 15,
                HighStopsRadius = 2000,
                LowStopsRadius = 1000,
                DownloadSourceUrl = "http://drysj6b8xu282.cloudfront.net/database-dc-{Version}-{Number}.zip",
                UpdatesUrl = "http://drysj6b8xu282.cloudfront.net/updates-dc-{Version}.txt",
                MapApplicationID = "76bc6da6-bb12-4e10-af6a-1e5daaac0c0b",
                MapAuthenticationToken = "LxJvKsejYbBCLld-1qaSQg",
                CategoryTree = new CategoryTree() {
                    { "Metrorail;subway.png",  (RouteSelector)(() => TB.Logic.GetCategory(RouteType.Metro)) },
                    { "Metrobus;bus-co.png",  new CategoryTree() {
                            { "DC & Maryland", (RouteSelector)( () => TB.Logic.FindAgency("MET").Routes.Where(r => r.Type == RouteType.Bus && !isInVirginia(r.Name)) ) },
                            { "Virginia", (RouteSelector)( () => TB.Logic.FindAgency("MET").Routes.Where(r => r.Type == RouteType.Bus && isInVirginia(r.Name)) ) },
                    }},
                    { "DC Circulator;bus-co.png",  (RouteSelector)(() => TB.Logic.FindAgency("DC").Routes) },
                },
                TextSizeRateCalculator = delegate(string text)
                {
                    if (text.Length == 3)
                    {
                        if ((text.First() == 'W' || text.Last() == 'M' || text.Last() == 'W' || text.Last() == 'G' || text.Last() == 'N' || text.First() == 'N'))
                            return 25.0 / 30.0;
                    }
                    else if (text.Length == 4)
                    {
                        if (text == "DCWE")
                            return 18.0 / 22.0;
                        return 20.0 / 22.0;
                    }
                    return 1.0;
                },
                LongNameExceptions = new HashSet<string>(new string[] { "Blue", "Red", "TAGS", "DC98", "DCWE" }),
            };
        }
        private static HashSet<string> virginiaNames;
        private static bool isInVirginia(string route)
        {
            return virginiaNames.Contains(route) || (char.IsDigit(route.First()) && char.IsLetter(route.Last()));
        }

        public static Config CreateLosAngeles()
        {
            return new Config
            {
                AppNumber = 4,
                CenterLocation = new GeoCoordinate(34.052257, -118.243414),
                LatitudeDegreeDistance = 110923.30880197878,
                LongitudeDegreeDistance = 92328.1433370971,
                IsFree = true, //JELENLEG ingyenes!!
                TrialDurationInDays = 3,
                BigTableLimit = 3,
                DefaultRouteStopPage = DefaultPageType.TripPage,
                HighStopsMapLevel = 12,
                LowStopsMapLevel = 15,
                HighStopsRadius = 2000,
                LowStopsRadius = 1000,
                DownloadSourceUrl = "http://drysj6b8xu282.cloudfront.net/database-la-{Version}-{Number}.zip",
                UpdatesUrl = "http://drysj6b8xu282.cloudfront.net/updates-la-{Version}.txt",
                MapApplicationID = "493fdbc9-166c-40d9-80e6-ab407efe04c2",
                MapAuthenticationToken = "erl9eKF5HSV1a7evJ1jIiQ",
                CategoryTree = new CategoryTree() {
                    { "Rail;LA/rail.png",  (RouteSelector)(() => RoutesInRange(800, 899) ) },
                    { "Local;LA/bus-local.png",  new CategoryTree() {
                            { "Downtown", (RouteSelector)( () => RoutesInRange(1, 99) ) },
                            { "E/W Other", (RouteSelector)( () => RoutesInRange(100, 199) ) },
                            { "N/S Other", (RouteSelector)( () => RoutesInRange(200, 299) ) },
                    }},
                    { "Express;LA/bus-express-light.png",  new CategoryTree() {
                            { "Downtown", (RouteSelector)( () => RoutesInRange(400, 499) ) },
                            { "Other", (RouteSelector)( () => RoutesInRange(500, 599) ) },
                    }},
                    { "Rapid;LA/bus-rapid.png",  (RouteSelector)(() => RoutesInRange(700, 799)) },
                    { "Other;LA/bus1.png",  new CategoryTree() {
                            { "Metro Transitway", (RouteSelector)( () => RoutesInRange(900, 999) ) },
                            { "Limited stop service",  (RouteSelector)(() => RoutesInRange(300, 399)) },
                            { "Shuttles & Circulators", (RouteSelector)( () => RoutesInRange(600, 699) ) },
                            { "Stadium Express", (RouteSelector)( () => TB.RouteGroups.Where(r => r.Name.Cast<char>().Any(ch => !char.IsDigit(ch) && ch != '/')) ) },
                    }},
                },
                HasLabelPred = r => r.Name.Cast<char>().Contains('/'),
                LabelTextPred = r => r.Name.Cast<char>().Contains('/') ? r.Name.Substring(r.Name.IndexOf('/') + 1) : "",
                NameTextPred = r => r.Name.Cast<char>().Contains('/') ? r.Name.Substring(0, r.Name.IndexOf('/')) : r.Name,
            };
        }
        private static IEnumerable<RouteGroup> RoutesInRange(int start, int end)
        {
            return TB.RouteGroups.Where(r => r.Name.Cast<char>().All(ch => char.IsDigit(ch) || ch == '/') && r.Name.Split('/').Any(nr => start <= int.Parse(nr) && int.Parse(nr) <= end));
        }
    }
}
