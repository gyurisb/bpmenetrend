#include "ForwardGraph.h"
#include "ReverseVertex.h"
#include "StopVertex.h"
#include "TripVertex.h"
#include "ForwardWay.h"
#include "ReverseWay.h"

#ifndef GRAPH_IMPL_H
#define GRAPH_IMPL_H

namespace Planning
{
	//definitions:
	list<pair<Vertex*, Distance>> Vertex::Neighbors(Distance dist)
	{
		if (Graph1->ForwardPlanning)
		{
			if (trip == NULL)
				return StopVertex::Neighbors(*this, dist);
			else
				return TripVertex::Neighbors(*this, dist);
		}
		else
		{
			if (trip == NULL)
				return ReverseStopVertex::Neighbors(*this, dist);
			else
				return ReverseTripVertex::Neighbors(*this, dist);
		}
	}

	Way Graph::CreateWay(VertexArray<Vertex*>& prev, VertexArray<Distance>& dist, Vertex* target)
	{
		if (ForwardPlanning)
			return ForwardWay(prev, dist, target, this->Time);
		else return ReverseWay(prev, dist, target, this->Time);
	}
}

#endif