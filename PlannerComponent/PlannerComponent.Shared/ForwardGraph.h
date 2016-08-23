#include "Graphs.h"
#include "ForwardWay.h"
#include "StopVertex.h"
#include "TripVertex.h"
#include "MyStdTypes.h"

#ifndef ForwardGraph_H
#define ForwardGraph_H

using namespace std;
using namespace Storage;

namespace Planning
{
	class ForwardGraph
	{
		/*virtual Way CreateWay(VertexArray<Vertex*>& prev, VertexArray<Distance>& dist, Vertex* target)
		{
            return ForwardWay(prev, dist, target, this->Time);
		}

		virtual Vertex* CreateStopVertex(Storage::Stop* stop)
		{
			return new StopVertex(stop);
		}
		virtual Vertex* CreateTripVertex(Storage::TripType* tt, int pos)
		{
            int i = 0;
            Vertex* ret = NULL;
            for (TimeEntry1& stop : tt->TimeEntries())
            {
                Vertex* v = new TripVertex(stop.Stop(), tt, i);
                if (i == pos) ret = v;
				data.Get(tt, i) = v;
                i++;
            }
            return ret;
		}*/
	};
}

#endif