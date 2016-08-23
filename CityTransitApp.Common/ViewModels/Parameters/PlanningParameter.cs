using PlannerComponent;
using PlannerComponent.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using TransitBase.Entities;

namespace CityTransitApp.Common.ViewModels
{
    public class PlanningParameter
    {
        public StopGroup SourceStop;
        public StopGroup DestStop;
        public DateTime DateTime;
        public PlanningTimeType PlanningType;
    }
}
