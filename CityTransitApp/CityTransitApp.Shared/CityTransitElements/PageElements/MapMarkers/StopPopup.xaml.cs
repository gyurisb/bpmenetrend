using CityTransitApp.Common.ViewModels;
using CityTransitElements.Controllers;
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
    public sealed partial class StopPopup : UserControl
    {
        public event EventHandler<Stop> StopClicked;
        public event EventHandler<RouteModel> TripClicked;

        public bool IsDistanceIgnored { get; private set; }
        public Brush BorderBrush { set { ContentPanel.BorderBrush = value; } }
        public Brush Background { set { ContentPanel.Background = value; } }

        public IMapControl MapParent { get; set; }

        public StopPopup(IMapControl mapParent)
        {
            this.InitializeComponent();
            MapParent = mapParent;
            Loaded += StopPopup_Loaded;
        }

        void StopPopup_Loaded(object sender, RoutedEventArgs e)
        {
            ContentPanel.MaxWidth = MapParent.Element.ActualWidth - 40;
            //ContentPanel.Width = MapParent.Element.ActualWidth * 0.7;
        }

        private static readonly double MaximumDistance = 1000.0;
        public async Task SetContent(IEnumerable<Stop> stops, bool isCurrent, DateTime startTime, GeoCoordinate sourceLocation, Stop sourceStop)
        {
            NameList.ItemsSource = stops.Select(s => s.Group).Distinct().Select(s => new HeaderModel { Stop = s.Stops.First(), IsLink = !isCurrent }).ToList();
            var model = await ArrivalTrips.CalculateRoutes(stops, startTime, sourceLocation, sourceStop, MaximumDistance);
            var lines = new List<string>();
            if (model.Count > 0)
            {
                double dist = model.First().Distance;
                if (sourceStop != null && stops.Contains(sourceStop))
                {
                    lines.Add(App.Common.Services.Resources.LocalizedStringOf("StopMapPosition"));
                }
                else if (dist > 10.0 && dist < MaximumDistance)
                {
                    int minutes = (int)Math.Round(model.First().WalkTime.TotalMinutes);
                    lines.Add(StringFactory.Format(App.Common.Services.Resources.LocalizedStringOf("StopMapDistance"), minutes > 1, StringFactory.LocalizeDistanceWithUnit(dist), minutes));
                }
                if (model.First().IsFar)
                    IsDistanceIgnored = true;
                RouteList.ItemsSource = model;
                lines.Add(StringFactory.Format(App.Common.Services.Resources.LocalizedStringOf("StopMapDepartures"), false, startTime.ToString("t")));
            }
            else
            {
                RouteList.Visibility = Visibility.Collapsed;
                lines.Add(App.Common.Services.Resources.LocalizedStringOf("RoutePanelOutOfService"));
            }
            DetailsText.Text = String.Join("\n", lines);
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

    }
}
