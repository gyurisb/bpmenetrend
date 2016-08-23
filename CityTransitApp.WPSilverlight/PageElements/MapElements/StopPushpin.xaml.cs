using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TransitBase;
using TransitBase.Entities;
using CityTransitApp.Common.ViewModels;
using System.Windows.Media;
using TransitBase.BusinessLogic;
using CityTransitApp.WPSilverlight.Resources;
using CityTransitServices.Tools;

namespace CityTransitApp.WPSilverlight.PageElements.MapElements
{
    public partial class StopPushpin : UserControl
    {
        private bool unset = true;
        private bool initialized = false;
        private GeoCoordinate sourceLocation;
        private Stop sourceStop;
        private DateTime startTime;
        private IEnumerable<Stop> stops;
        private bool isCurrent;

        public event EventHandler<Stop> StopClicked;
        public event EventHandler<RouteModel> TripClicked;
        public bool IsExpanded { get; private set; }
        public bool IsDistanceIgnored { get; private set; }
        public bool IsCurrent { get { return isCurrent; } }
        public IEnumerable<Stop> Stops { get { return stops; } }
        public double Priority { get; private set; }

        private bool IsEmpty { get; set; }
        private Brush MainColor { get; set; }

        public StopPushpin()
        {
            InitializeComponent();
            ContentPanel.MaxWidth = Application.Current.Host.Content.ActualWidth - 40;
            ContentPanel.Width = Application.Current.Host.Content.ActualWidth * 0.7;
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
                MainColor = firstType.GetColors().SecondaryColorBrush;
                FrontPanel.Background = ContentPanel.Background = MainColor;
                FrontPanel.BorderBrush = ContentPanel.BorderBrush = firstType.GetColors().MainColorBrush;

                if (firstType.Type == RouteType.Metro)
                {
                    SubwayEllipse.Visibility = Visibility.Visible;
                    FrontIcon.FontSize = 40.0;
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
                IsEmpty = true;
            }

            if (isSmall)
            {
                if (SubwayEllipse.Visibility == Visibility.Visible)
                {
                    SubwayEllipse.Height = 30;
                    SubwayEllipse.Width = 30;
                    SubwayEllipse.StrokeThickness = 3;
                }
                FrontIcon.FontSize = FrontIcon.FontSize * 0.5;
            }
        }

        public void Update(DateTime startTime, GeoCoordinate sourceLocation)
        {
            this.unset = true;
            this.startTime = startTime;
            this.sourceLocation = sourceLocation;

            IsDistanceIgnored = false;
            RouteList.Visibility = Visibility.Visible;
            if (IsExpanded)
                ShowContent();
        }

        public void HideContent()
        {
            IsExpanded = false;
            FrontPanel.Visibility = Visibility.Visible;
            ContentPanel.Visibility = Visibility.Collapsed;
        }

        private static readonly double MaximumDistance = 1000.0;
        public async void ShowContent()
        {
            IsExpanded = true;
            FrontPanel.Visibility = Visibility.Collapsed;
            ContentPanel.Visibility = Visibility.Visible;

            if (unset)
            {
                if (!initialized)
                    throw new InvalidOperationException("Uninitalized pushpin showed.");
                unset = false;

                NameList.ItemsSource = stops.Select(s => s.Group).Distinct().Select(s => new HeaderModel { Stop = s.Stops.First(), IsLink = !isCurrent }).ToList();
                var model = await ArrivalTrips.CalculateRoutes(stops, startTime, sourceLocation, sourceStop, MaximumDistance);
                if (model.Count > 0)
                {
                    double dist = model.First().Distance;
                    DetailsText.Text = "";
                    if (sourceStop != null && stops.Contains(sourceStop))
                    {
                        DetailsText.Text += AppResources.StopMapPosition + "\n";
                    }
                    else if (dist > 10.0 && dist < MaximumDistance)
                    {
                        int minutes = (int)Math.Round(model.First().WalkTime.TotalMinutes);
                        DetailsText.Text += StringFactory.Format(AppResources.StopMapDistance, minutes > 1, StringFactory.LocalizeDistanceWithUnit(dist), minutes);
                    }
                    if (model.First().IsFar)
                        IsDistanceIgnored = true;
                    RouteList.ItemsSource = model;
                    DetailsText.Text += StringFactory.Format(AppResources.StopMapDepartures, false, startTime.ToString("t"));
                }
                else
                {
                    RouteList.Visibility = Visibility.Collapsed;
                    DetailsText.Text = AppResources.RoutePanelOutOfService;
                }
            }
        }

        public static StopPushpin Create(GeoCoordinate location, bool isCurrent, IEnumerable<Stop> stops, DateTime startTime, GeoCoordinate sourceLocation, Stop sourceStop, bool isSmall)
        {
            var content = new StopPushpin { };
            content.Initialize(stops, isCurrent, startTime, sourceLocation, sourceStop, isSmall);
            if (content.IsEmpty) return null;
            return content;
        }

        private void NameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NameList.SelectedItem != null)
            {
                var selected = (HeaderModel)NameList.SelectedItem;
                if (selected.IsLink && StopClicked != null)
                    StopClicked(this, selected.Stop);
                NameList.SelectedItem = null;
            }
        }

        private void RouteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RouteList.SelectedItem != null)
            {
                if (TripClicked != null)
                    TripClicked(this, (RouteModel)RouteList.SelectedItem);
                RouteList.SelectedItem = null;
            }
        }

        private class HeaderModel
        {
            public Stop Stop;
            public bool IsLink;
            public Visibility IsLinkVisibility { get { return IsLink ? Visibility.Visible : Visibility.Collapsed; } }
            public String Name { get { return Stop.Name; } }
        }

        //private void PlanFrom_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    if (PlanningFromClicked != null)
        //        PlanningFromClicked(this, new EventArgs());
        //}

        //private void PlanTo_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    if (PlanningToClicked != null)
        //        PlanningToClicked(this, new EventArgs());
        //}
    }
}
