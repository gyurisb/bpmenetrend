using System;
using System.Collections.Generic;
using System.Text;
using TransitBase;
using TransitBase.Entities;

namespace CityTransitApp.Common.ViewModels
{
    public class StopParameter
    {
        public StopGroup StopGroup;
        public DateTime? DateTime;
        public ParameterLocation Location;

        public Stop SourceStop
        {
            set
            {
                if (Location == null) Location = new ParameterLocation();
                Location.Stop = value;
            }
            get { return Location.Stop; }
        }
        public bool IsNear
        {
            set
            {
                if (Location == null) Location = new ParameterLocation();
                Location.IsNear = value;
            }
            get { return Location.IsNear; }
        }
    }

    public class ParameterLocation
    {
        public Stop Stop;
        public bool IsNear;
    }
}
