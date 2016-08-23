using CityTransitApp.Common.ViewModels;
using CityTransitElements.Controllers;
using CityTransitElements.Effects;
using CityTransitServices;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.Entities;
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

namespace CityTransitApp.CityTransitElements.PageElements.MapMarkers
{
    public sealed partial class StopPushpin : UserControl
    {
        private bool unset = true;
        private bool initialized = false;
        private GeoCoordinate sourceLocation;
        private Stop sourceStop;
        private DateTime startTime;
        private IEnumerable<Stop> stops;
        private bool isCurrent;
        private bool isEmpty;
        private Brush mainColor;
        private IMapControl mapParent;

        public event EventHandler<Stop> StopClicked;
        public event EventHandler<RouteModel> TripClicked;
        public bool IsDistanceIgnored { get; private set; }
        public double Priority { get; private set; }
        public StopPopup Popup { get; private set; }

        public bool IsExpanded { get { return Popup != null; } }
        public bool IsCurrent { get { return isCurrent; } }
        public IEnumerable<Stop> Stops { get { return stops; } }

        public StopPushpin(IMapControl mapParent)
        {
            InitializeComponent();
            this.mapParent = mapParent;
        }

        private void Initialize(IEnumerable<Stop> stops, bool isCurrent, DateTime startTime, GeoCoordinate sourceLocation, Stop sourceStop, bool isSmall)
        {
            this.sourceLocation = sourceLocation;
            this.sourceStop = sourceStop;
            this.startTime = startTime;
            this.stops = stops;
            this.isCurrent = isCurrent;
            initialized = true;

            var routes = stops.SelectMany(s => s.Routes).Select(r => r.RouteGroup).Distinct().ToList();
            if (routes.Count > 0)
            {
                var firstType = routes.MinBy(r => r.GetCustomTypePriority());
                this.Priority = firstType.GetCustomTypePriority();
                mainColor = firstType.GetColors().SecondaryColorBrush;
                FrontPanel.Background = mainColor;
                FrontPanel.BorderBrush = firstType.GetColors().MainColorBrush;
                HoverEffects.SetBackground(FrameBorder, mainColor);

                if (firstType.Type == RouteType.Metro)
                {
                    SubwayEllipse.Visibility = Visibility.Visible;
                    FrontIcon.FontSize = 34.0;
                    FrontIcon.Text = "" + (char)0xF239;
                }
                else if (firstType.Type == RouteType.RailRoad)
                {
                    FrontIcon.Text = "" + (char)0xF239;
                }
                else if (firstType.Type == RouteType.Tram)
                {
                    FrontIcon.Text = "" + (char)0xF238;
                }
                else if (firstType.Type == RouteType.Ferry)
                {
                    FrontIcon.Text = "" + (char)0xf21a;
                }
                //else
                //{
                //    FrontIcon.Text = "" + (char)0xF207;
                //}
            }
            else
            {
                FrontIcon.Text = "" + (char)0xF041;
                isEmpty = true;
            }

            if (isSmall)
            {
                if (SubwayEllipse.Visibility == Visibility.Visible)
                {
                    SubwayEllipse.Height = 25;
                    SubwayEllipse.Width = 25;
                    SubwayEllipse.StrokeThickness = 3;
                }
                FrontIcon.FontSize = FrontIcon.FontSize * 0.5;
            }
        }

        public async void Update(DateTime startTime, GeoCoordinate sourceLocation)
        {
            this.unset = true;
            this.startTime = startTime;
            this.sourceLocation = sourceLocation;

            if (Popup != null)
            {
                await Popup.SetContent(stops, isCurrent, startTime, sourceLocation, sourceStop);
                IsDistanceIgnored = Popup.IsDistanceIgnored;
            }
        }

        public void HideContent()
        {
            Popup = null;
        }

        public async Task<UIElement> ShowPopup()
        {
            this.Popup = new StopPopup(mapParent);
            Popup.Background = FrontPanel.Background;
            Popup.BorderBrush = FrontPanel.BorderBrush;
            await Popup.SetContent(stops, isCurrent, startTime, sourceLocation, sourceStop);
            IsDistanceIgnored = Popup.IsDistanceIgnored;
            Popup.StopClicked += (sender, stop) =>
            {
                if (StopClicked != null)
                    StopClicked(this, stop);
            };
            Popup.TripClicked += (sender, trip) =>
            {
                if (TripClicked != null)
                    TripClicked(this, trip);
            };
            return Popup;
            //FrontPanel.Visibility = Visibility.Collapsed;
            //ContentPanel.Visibility = Visibility.Visible;
            //if (unset)
            //{
            //    if (!initialized)
            //        throw new InvalidOperationException("Uninitalized pushpin showed.");
            //    unset = false;
            //    //set values
            //}
        }

        public static StopPushpin Create(IMapControl mapParent, GeoCoordinate location, bool isCurrent, IEnumerable<Stop> stops, DateTime startTime, GeoCoordinate sourceLocation, Stop sourceStop, bool isSmall)
        {
            var content = new StopPushpin(mapParent);
            content.Initialize(stops, isCurrent, startTime, sourceLocation, sourceStop, isSmall);
            if (content.isEmpty) return null;
            return content;
        }

        //private void PlanFrom_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    if (PlanningFromClicked != null)
        //        PlanningFromClicked(this, new EventArgs());
        //}

        //private void PlanTo_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    if (PlanningToClicked != null)
        //        PlanningToClicked(this, new EventArgs());
        //}
    }
}
