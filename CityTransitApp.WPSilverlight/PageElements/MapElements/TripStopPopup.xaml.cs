using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TransitBase.Entities;
using TransitBase;

namespace CityTransitApp.WPSilverlight.PageElements.MapElements
{
    public partial class TripStopPopup : UserControl
    {
        public event EventHandler<Stop> StopClick;
        private Stop stop;

        public TripStopPopup()
        {
            InitializeComponent();
            LayoutRoot.MaxWidth = Application.Current.Host.Content.ActualWidth * 0.8;
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

        private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (StopClick != null)
            {
                StopClick(this, stop);
            }
        }
    }
}
