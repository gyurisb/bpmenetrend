#include "Graphs.h"

#ifndef STOPVERTEX_H
#define STOPVERTEX_H

using namespace Storage;

namespace Planning
{
	double WALK_SPEED_RATE;

	class StopVertex : public Vertex
	{
	protected:
		StopVertex(){}
	public:
			
		static list<pair<Vertex*, Distance>> Neighbors(Vertex& vertex, Distance dist)
		{
			//SStop.Start();
            DateTime time = Graph1->Time + TimeSpan::FromMinutes(dist.Time);
			list<pair<Vertex*, Distance>> list;
            for (ArrivalResult& arrival : vertex.stop->NextArrivalTripTypes(time))
            {
				Vertex* tripVertex = &Graph1->Get(arrival.Trip, arrival.Position);
				tripVertex->hint_trip = arrival.ArrivalTrip;
				tripVertex->hint_type = arrival.Type;
                list.push_back(make_pair(
                    tripVertex,
                    Distance((int)arrival.ArrivalTime.TotalMinutes(), 0.0, 1)
                ));
            }
            /*for (Storage::Stop* s0 : Union<Storage::Stop*>(vertex.stop->NearStops(), vertex.stop->Group()->Stops()))
                if (s0 != vertex.stop && Storage::StopEnabled(s0))
                    list.push_back(make_pair(&Graph1->Get(s0), walkToDistance(vertex.stop->Distance(s0))));*/
            for (Storage::Transfer& transfer : vertex.stop->Transfers())
                if (Storage::StopEnabled(transfer.Target()))
                    list.push_back(make_pair(&Graph1->Get(transfer.Target()), walkToDistance(transfer.Distance)));
			//SStop.Stop();
			return list;
		}

	protected:
		/*template <class Key, class T, class Func>
        static int binarySearchBy(ArrayPt<T>& list, Key value, Func selector)
        {
            int lower = 0;
            int upper = list.size() - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
				Key middleVal = selector(list[middle]);
                if (value == middleVal)
                    return middle;
                else if (value < middleVal)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            throw "ArgumentException('Element not found in list!')";
        }
        static int correctPosition(Storage::Stop* stop, short time, ArrayPt<TimeEntry1>& timeEntries, int startIndex)
        {
            int im = startIndex - 1, ip = startIndex + 1;
            while (true)
            {
                bool quit = true;
                if (im >= 0 && timeEntries[im].Time == time)
                {
                    quit = false;
                    if (timeEntries[im].Stop() == stop) return im;
                    im--;
                }
                if (ip < timeEntries.size() && timeEntries[ip].Time == time)
                {
                    quit = false;
                    if (timeEntries[ip].Stop() == stop) return ip;
                    ip++;
                }
                if (quit) throw "ArgumentException('Stop not found in list')";
            }
        }
        static int findPosition(Storage::TripType* tt, Storage::Stop* stop, unsigned short time)
        {
            ArrayPt<TimeEntry1> timeEntries = tt->TimeEntries();
			int i = binarySearchBy<unsigned short>(timeEntries, time, [](TimeEntry1 e) { return e.Time; });
            if (timeEntries[i].Stop() != stop)
                return correctPosition(stop, time, timeEntries, i);
            return i;
        }*/
        static Distance walkToDistance(short walkDistance)
        {
            return Distance(ceil(walkDistance / WALK_SPEED_RATE), walkDistance, 0);
        }
	};
}

#endif