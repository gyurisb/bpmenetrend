using System;
using System.Collections.Generic;
using System.Text;
using TransitBase.Entities;

namespace CityTransitApp.Common.ViewModels
{
    public class TripParameter
    {
        public Trip Trip;
        public DateTime? DateTime;
        public ParameterLocation Location;
        public bool NextTrips;
        public Stop CurrentStop;

        private StopGroup stop;
        public StopGroup Stop
        {
            get
            {
                if (CurrentStop == null)
                    return stop;
                return CurrentStop.Group;
            }
            set
            {
                stop = value;
            }
        }
    }
}
