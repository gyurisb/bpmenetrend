using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace PlannerComponent.Interface
{
	public enum PlanningTimeType { Departure = 0, Arrival };
	public enum PlanningAspect { Time = 0, TransferCount, WalkDistance };

    public interface IPlannerComponent
    {
        IEnumerable<Tuple<DateTime, Trip>> GetCurrentTrips(DateTime currentTime, Route route, IEnumerable<Stop> stops, int prevCount, int nextCount, TimeSpan limit);
        void SetParams(PlanningArgs args);
        Way CalculatePlanning(StopGroup source, StopGroup target, DateTime startTime, PlanningAspect aspect);
        void Close(bool safe);
    }
}
