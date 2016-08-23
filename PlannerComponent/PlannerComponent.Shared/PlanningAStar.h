/*
#include "Planning.h"
#include "boost\pending\mutable_queue.hpp"
#include "boost\heap\fibonacci_heap.hpp"

#ifndef PLANNINGASTAR_H
#define PLANNINGASTAR_H

#define FILL_AT_START 0


namespace Planning
{
	struct DVPair
	{
		DVPair(Planning::Distance dist, Planning::Vertex* v) {}
	};

	typedef boost::typed_identity_property_map<DistVertexPair> iden;
	typedef greater<DistVertexPair> greater_t;
	typedef vector<DistVertexPair> vector_t;
	typedef boost::mutable_queue<DistVertexPair, vector_t, greater_t, iden> mutable_queue_t;


	template <typename Pred>
	Way CalculateAStar(StopGroup* source, StopGroup* target, Pred pred)
	{
		typedef boost::heap::fibonacci_heap<DistVertexPair, boost::heap::compare<Bigger<Pred>>> fib_heap;
		fib_heap::handle_type nullHandler = fib_heap::handle_type();

		VertexArray<Distance> dist;
        VertexArray<Vertex*> prev;
		int size = sizeof(fib_heap::handle_type);
		VertexArray<fib_heap::handle_type> handler;
		fib_heap q;

        //VertexArray<bool> complete;
		////priority_queue<DistVertexPair, vector<DistVertexPair>, Bigger<Pred>> q;
		//mutable_queue_t q(1024, greater_t(), iden());

		Vertex t(&target->Stops()[0]);

#if FILL_AT_START
		OutputDebugString(L"\nStarting filling heap!\n");
		int totalSize = Graph1->Size();
		int curSize = 0;
		int lastPercent;
		Graph1->ForEach([&q, &handler, &totalSize, &curSize, &lastPercent](Vertex& v)
		{
			handler[v] = q.push(DistVertexPair(Distance(), &v));
			curSize++;
			int percent = curSize*100 / totalSize;
			if (percent != lastPercent)
			{
				lastPercent = percent;
				OutputDebugString((L" " + percent + L"%\n")->Data());
			}
		});
		OutputDebugString(L"Done filling heap!\n");
#endif
        for (Stop& sourceStop : source->Stops())
        {
			Vertex& s = Graph1->Get(&sourceStop);
            dist[s] = Distance::Zero();
            prev[s] = NULL;
			
#if FILL_AT_START
			*handler[s] = DistVertexPair(heuristicCostEstimate(&s, &t), &s);
			q.increase(handler[s]);
#else
			handler[s] = q.push(DistVertexPair(heuristicCostEstimate(&s, &t), &s));
#endif
        }

		set<int> targets;
		for (Stop& targetStop : target->Stops())
			targets.insert(targetStop.Id());
		
		OutputDebugString(L"Start planning!\n");
        while (!q.empty())
        {
			DistVertexPair uPair = q.top();
			q.pop();
            Vertex& u = *uPair.Vertex;
			handler[u] = nullHandler;

            //if (complete[u])
			//	continue;
            //complete[u] = true;

            if (u.Type() == Vertex::Walk && targets.count(u.Stop()->Id()))
			{
				OutputDebugString(L"\nWay found!\n");
                return Graph1->CreateWay(prev, dist, &u);
			}

            for (pair<Vertex*, Distance>& vPair : u.Neighbors(dist[u]))
            {
                Vertex& v = *vPair.first;
                Distance alt = dist[u] + vPair.second;

                //if (complete[v])
					continue;

                if (pred(alt, dist[v]) < 0)
                {
					fib_heap::handle_type& handler_v = handler[v];
					if (handler_v == nullHandler)
						handler_v = q.push(DistVertexPair(alt + heuristicCostEstimate(&v, &t), &v));
					else
					{
						*handler_v = DistVertexPair(alt + heuristicCostEstimate(&v, &t), &v);
						q.update(handler_v);
					}
					//q.push(DistVertexPair(alt + heuristicCostEstimate(&v, &t), &v));

                    dist[v] = alt;
                    prev[v] = &u;
                }
            }
        }
		OutputDebugString(L"\nWay not found!\n");
		return Way();
	}
	
	
    const double EstimationWeight = 1.0;
    const int EstimationCap = 15;

    Distance heuristicCostEstimate(Vertex* a, Vertex* b)
    {
        double dist = a->Stop()->Distance(b->Stop());
        int time = (int)dist * 60 / 10000;
        if (time < EstimationCap)
            time = 0;
        else
            time = (int)(time * EstimationWeight);
		return Distance(time, 0, 0);
    }
}

#endif
*/