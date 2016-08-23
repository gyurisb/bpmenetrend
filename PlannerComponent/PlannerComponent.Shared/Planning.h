#include "Graphs.h"
#include <queue>
#include <set>
#include <vector>

#ifndef Planning_H
#define Planning_H

namespace Planning
{
	template <typename Pred>
	struct Bigger
	{
		Pred pred;
		Bigger() : pred(Pred()) {}
		template <typename Val>
		bool operator()(const Val& val1, const Val& val2) const { return pred(val1, val2) > 0; }
	};
	
	struct DistVertexPair
	{
		Distance Distance;
		Vertex* Vertex;
		DistVertexPair(Planning::Distance dist, Planning::Vertex* v) : Distance(dist), Vertex(v) {}
		bool operator<(const DistVertexPair& other) const { return Distance::CompareTime()(Distance, other.Distance) > 0; }
		bool operator>(const DistVertexPair& other) const { return Distance::CompareTime()(Distance, other.Distance) < 0; }
		DistVertexPair operator[](const unsigned int a) const { return *this; }
		operator unsigned int() { return 0; }
	};
	Distance::Distance(const DistVertexPair& pair)
	{
		*this = pair.Distance;
	}

	template <typename Pred>
	Way Calculate(StopGroup* source, StopGroup* target, Pred pred)
	{
		VertexArray<Distance> dist;
        VertexArray<Vertex*> prev;
        VertexArray<bool> complete;
		priority_queue<DistVertexPair, vector<DistVertexPair>, Bigger<Pred>> q;

        for (Stop& sourceStop : source->Stops())
        {
			Vertex& s = Graph1->Get(&sourceStop);
            dist[s] = Distance::Zero();
            prev[s] = NULL;
			q.push(DistVertexPair(Distance::Zero(), &s));
        }

		set<int> targets;
		for (Stop& targetStop : target->Stops())
			targets.insert(targetStop.Id());


        while (!q.empty())
        {
			DistVertexPair uPair = q.top();
			q.pop();
            Vertex& u = *uPair.Vertex;
            Distance distU = uPair.Distance;

            if (complete[u])
				continue;
            complete[u] = true;

            if (u.Type() == Vertex::Walk && targets.count(u.Stop()->Id()))
                return Graph1->CreateWay(prev, dist, &u);

            for (pair<Vertex*, Distance>& vPair : u.Neighbors(distU))
            {
                Vertex& v = *vPair.first;
                Distance alt = distU + vPair.second;

                if (complete[v])
					continue;

                if (pred(alt, dist[v]) < 0)
                {
                    q.push(DistVertexPair(alt, &v));
                    dist[v] = alt;
                    prev[v] = &u;
                }
            }
        }
		return Way();
	}
}

#endif