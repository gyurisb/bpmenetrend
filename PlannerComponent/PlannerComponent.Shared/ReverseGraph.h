#include "Graphs.h"
#include "ReverseVertex.h"
#include "ReverseWay.h"

#ifndef REVERSE_GRAPH_H
#define REVERSE_GRAPH_H

namespace Planning
{
	class ReverseGraph
	{
		/*virtual Way CreateWay(VertexArray<Vertex*>& prev, VertexArray<Distance>& dist, Vertex* target)
		{
            return ReverseWay(prev, dist, target, this->Time);
		}

		virtual Vertex* CreateStopVertex(Storage::Stop* stop)
		{
			return new ReverseStopVertex(stop);
		}
		virtual Vertex* CreateTripVertex(Storage::TripType* tt, int pos)
		{
            int i = 0;
            Vertex* ret = NULL;
            for (TimeEntry1& stop : tt->TimeEntries())
            {
                Vertex* v = new ReverseTripVertex(stop.Stop(), tt, i);
                if (i == pos) ret = v;
				data.Get(tt, i) = v;
                i++;
            }
            return ret;
		}*/
	};
}

#endif