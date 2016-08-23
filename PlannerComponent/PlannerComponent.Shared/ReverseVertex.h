#include "Graphs.h"
#include "StopVertex.h"
#include "TripVertex.h"

#ifndef REVERSE_VERTEX_H
#define REVERSE_VERTEX_H

namespace Planning
{
	class ReverseStopVertex : public StopVertex
	{
		ReverseStopVertex() {}
	public:
			
		static list<pair<Vertex*, Distance>> Neighbors(Vertex& vertex, Distance dist)
		{
            DateTime time = Graph1->Time - TimeSpan::FromMinutes(dist.Time);
			list<pair<Vertex*, Distance>> list;
            for (ArrivalResult& arrival : vertex.stop->PrevArrivalTripTypes(time))
            {
				Vertex* tripVertex = &Graph1->Get(arrival.Trip, arrival.Position);
				tripVertex->hint_trip = arrival.ArrivalTrip;
				tripVertex->hint_type = arrival.Type;
                list.push_back(make_pair(
                    tripVertex,
                    Distance((int)-arrival.ArrivalTime.TotalMinutes(), 0.0, 0)
                ));
            }
            for (Storage::Transfer& transfer : vertex.stop->Transfers())
                if (Storage::StopEnabled(transfer.Target()))
                    list.push_back(make_pair(&Graph1->Get(transfer.Target()), walkToDistance(transfer.Distance)));
			return list;
		}
	};

	class ReverseTripVertex : public TripVertex
	{
		ReverseTripVertex() {}
	public:
		static list<pair<Vertex*, Distance>> Neighbors(Vertex& vertex, Distance dist)
		{
			list<pair<Vertex*, Distance>> list;
            if (Storage::StopEnabled(vertex.stop))
				list.push_back(std::make_pair(&Graph1->Get(vertex.stop), Distance(0, 0, 1)));

            if (vertex.pos > 0)
			{
				DateTime time = Graph1->Time - TimeSpan::FromMinutes(dist.Time);
				TripTimeType* type = NULL;
				Vertex* prevVertex = &Graph1->Get(vertex.trip, vertex.pos - 1);
				if (checkHint(vertex, time))
				{
					//HintSuccess++;
					type = vertex.hint_type;
					prevVertex->hint_trip = vertex.hint_trip;
					prevVertex->hint_type = vertex.hint_type;
				}
				else
				{
					//HintFailed++;
					ArrivalResult res = vertex.stop->FindPrevTrip(time, vertex.trip, vertex.pos);
					type = res.Type;
					prevVertex->hint_trip = res.ArrivalTrip;
					prevVertex->hint_type = res.Type;
				}
				short travelTime = type->TimeEntries()[vertex.pos].Time - type->TimeEntries()[vertex.pos - 1].Time;
                list.push_back(make_pair(
                        prevVertex,
                        Distance(travelTime, 0, 0)
                    ));
			}
			return list;
		}
	};
}

#endif