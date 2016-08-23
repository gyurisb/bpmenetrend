using System;
using System.Collections.Generic;
using System.Text;
using TransitBase.Entities;

namespace CityTransitApp.Common.ViewModels
{
    public class TimetableParameter
    {
        public StopGroup Stop;
        public Route Route;
        public DateTime? SelectedTime;
    }
}
