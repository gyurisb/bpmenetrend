using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using TransitBase;
using CityTransitServices;
using CityTransitElements.Controllers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageElements.MapMarkers
{
    public sealed partial class TripStopPopup : UserControl
    {
        public event EventHandler<Stop> StopClick;
        private Stop stop;
        private IMapControl mapParent;

        public TripStopPopup(IMapControl mapParent)
        {
            InitializeComponent();
            this.mapParent = mapParent;
            Loaded += TripStopPopup_Loaded;
        }

        void TripStopPopup_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutRoot.MaxWidth = mapParent.Element.ActualWidth * 0.8;
        }

        public void Initialize(Trip trip, int position, TimeSpan originalTime)
        {
            var stops = trip.Stops;
            this.stop = stops[position].Item2;
            var time = stops[position].Item1;
            var transfers = stop.Group.RouteGroups
                .Except(new RouteGroup[] { trip.Route.RouteGroup })
                .OrderByText(r => r.Name)
                .OrderByWithCache(r => r.GetCustomTypePriority())
                .ToList();
            var offsetTime = time - originalTime;

            StopText.Text = stop.Name;
            TimeText.Text = String.Format(@"{0:t} ({1}{2:hh\:mm})", DateTime.Today + time, offsetTime >= TimeSpan.Zero ? "+" : "-", offsetTime);
            if (transfers.Count > 0)
                TransfersText.Text = String.Join(", ", transfers.Select(s => s.Name).Distinct());
            else TransfersTextParent.Visibility = Visibility.Collapsed;

            LayoutRoot.Background = trip.Route.RouteGroup.GetColors().SecondaryColorBrush;
            LayoutRoot.BorderBrush = trip.Route.RouteGroup.GetColors().MainColorBrush;
        }

        private void LayoutRoot_Tap(object sender, TappedRoutedEventArgs e)
        {
            if (StopClick != null)
            {
                StopClick(this, stop);
            }
        }
    }
}
