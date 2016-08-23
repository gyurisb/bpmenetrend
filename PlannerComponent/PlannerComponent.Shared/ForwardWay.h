#include "Graphs.h"

#ifndef ForwardWay_H
#define ForwardWay_H

namespace Planning
{
	class ForwardWay : public Way
	{
	public:
		ForwardWay(VertexArray<Vertex*>& prev, VertexArray<Distance>& dist, Vertex* target, DateTime startTime)
		{
			TotalDist = dist[*target];

            Vertex* next = target;
            Vertex* v = prev[*target];

			Vertex* startStop = NULL;
            Vertex* endStop = NULL;
            list<Stop*> tripsStops;

            while (v != NULL)
            {
				if (v->Type() == Vertex::Vehicle && next->Type() == Vertex::Walk)
				{
					if (endStop != NULL)
					{
						WayEntry entry;
						entry.Route = startStop->Trip()->Route();
						entry.TripType = startStop->Trip();
						entry.StartStop = startStop->Stop();
						entry.EndStop = endStop->Stop();
						entry.StartTime = startTime.TimeOfDay() + TimeSpan::FromMinutes(dist[*startStop].Time);
						entry.EndTime = startTime.TimeOfDay() + TimeSpan::FromMinutes(dist[*endStop].Time);
						entry.WaitMinutes = dist[*startStop].Time - dist[*prev[*startStop]].Time;
						entry.WalkBeforeMeters = dist[*startStop].WalkDistance - dist[*next].WalkDistance;
						entry.StopCount = tripsStops.size() - 1;
						entry.insert(entry.begin(), tripsStops.begin(), tripsStops.end());

						this->push_front(entry);
					}
					else
					{
						LastWalkDistance = TotalDist.WalkDistance - dist[*v].WalkDistance;
					}
					endStop = v;
					tripsStops.clear();
				}

				if (v->Type() == Vertex::Vehicle)
				{
					tripsStops.push_front(v->Stop());
				}

				if (v->Type() == Vertex::Walk && next->Type() == Vertex::Vehicle)
				{
					startStop = next;
				}

                next = v;
                v = prev[*v];
            }
			
			if (endStop != NULL)
			{
				WayEntry entry;
				entry.Route = startStop->Trip()->Route();
				entry.TripType = startStop->Trip();
				entry.StartStop = startStop->Stop();
				entry.EndStop = endStop->Stop();
				entry.StartTime = startTime.TimeOfDay() + TimeSpan::FromMinutes(dist[*startStop].Time);
				entry.EndTime = startTime.TimeOfDay() + TimeSpan::FromMinutes(dist[*endStop].Time);
				entry.WaitMinutes = dist[*startStop].Time - dist[*prev[*startStop]].Time;
				entry.WalkBeforeMeters = dist[*startStop].WalkDistance - dist[*next].WalkDistance;
				entry.StopCount = tripsStops.size();
				entry.insert(entry.begin(), tripsStops.begin(), tripsStops.end());

				this->push_front(entry);
			}
			else
			{
				LastWalkDistance = TotalDist.WalkDistance;
			}
		}

	};
}
#endif