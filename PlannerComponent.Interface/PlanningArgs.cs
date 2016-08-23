using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlannerComponent.Interface
{
    public struct PlanningArgs
	{
		//tervezési mezők
		public PlanningTimeType Type;
		//plansettings mezők
        public byte EnabledTypes; //a sorrend a tömbben: Tram, Metro, UrbanTrain, Bus
        public bool OnlyWheelchairAccessibleTrips;
        public bool OnlyWheelchairAccessibleStops;
		//config mezők
		//bool UseEstimation;
        public double LatitudeDegreeDistance;
        public double LongitudeDegreeDistance;
        public double WalkSpeedRate;
	};
}
