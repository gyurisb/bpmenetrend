#include "Graphs.h"

#ifndef REVERSE_WAY_H
#define REVERSE_WAY_H

namespace Planning
{
	class ReverseWay : public Way
	{
	public:
		ReverseWay(VertexArray<Vertex*>& next, VertexArray<Distance>& dist, Vertex* target, DateTime startTime)
        {
			TotalDist = dist[*target];

            Vertex* previous = target;
            Vertex* v = next[*target];
            Vertex* startStop = NULL;

            double walkDistance = 0;
            int nextWaitMinutes = 0;
            list<Stop*> tripsStops;
            while (v != NULL)
            {
                if (v->Type() == Vertex::Walk && previous->Type() == Vertex::Vehicle)
                {
					WayEntry entry;
                    entry.Route = previous->Trip()->Route();
					entry.TripType = previous->Trip();
                    entry.StartStop = startStop->Stop();
                    entry.EndStop = v->Stop();
                    entry.StartTime = startTime.TimeOfDay() - TimeSpan::FromMinutes(dist[*startStop].Time);
                    entry.EndTime = startTime.TimeOfDay() - TimeSpan::FromMinutes(dist[*previous].Time),
                    entry.WaitMinutes = nextWaitMinutes;
                    entry.WalkBeforeMeters = (int)walkDistance;
                    entry.StopCount = tripsStops.size() - 1;
					this->push_back(entry);
					this->back().insert(this->back().begin(), tripsStops.begin(), tripsStops.end());
                    nextWaitMinutes = dist[*previous].Time - dist[*v].Time;
                    walkDistance = 0;
                }
                else if (v->Type() == Vertex::Vehicle && previous->Type() == Vertex::Walk)
                {
                    tripsStops.clear();
                    startStop = previous;
                }
                else if (v->Type() == Vertex::Walk && previous->Type() == Vertex::Walk)
                {
                    walkDistance += dist[*previous].WalkDistance - dist[*v].WalkDistance;
                }

                if (v->Type() == Vertex::Vehicle)
                    tripsStops.push_back(v->Stop());

                previous = v;
                v = next[*v];
            }
			LastWalkDistance = walkDistance;
        }
	};
}

#endif