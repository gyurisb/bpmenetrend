#include "MyStdTypes.h"
#include "Distance.h"
#ifndef WAY_H
#define WAY_H

using namespace std;
using namespace Storage;

namespace Planning
{
	struct WayEntry : list<Stop*>
	{
		Route* Route;
		TripType* TripType;
		Stop* StartStop;
		Stop* EndStop;
		TimeSpan StartTime;
		TimeSpan EndTime;
		int WaitMinutes;
		int WalkBeforeMeters;
		int StopCount;
	};

	class Way : public std::list<WayEntry>
	{
	public:
		Distance TotalDist;
		int LastWalkDistance;
		std::TimeSpan TotalTime()
		{
			return sum<TimeSpan>(begin(), end(), [](WayEntry& e) { return e.StartTime - e.EndTime; });
		}
	};
}

#endif