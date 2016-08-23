using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace PlannerComponent.Interface
{
    public class Way : List<WayEntry>
    {
		public TimeSpan TotalTime;
        public double TotalWalk;
        public int TotalTransfers;
        public int LastWalkDistance;
        public string Message;
    }
    public class WayEntry : List<Stop>
    {
        public Route Route;
        public TripType TripType;
        public Stop StartStop;
        public Stop EndStop;
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public int WaitMinutes;
        public int WalkBeforeMeters;
        public int StopCount;
    }
}
