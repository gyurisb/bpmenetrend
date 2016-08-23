#include <iostream>
#include <list>
#include "VertexArray.h"
#include "Way.h"
#include "Distance.h"

#ifndef GRAPHS_H
#define GRAPHS_H

using namespace std;

namespace Planning
{
	class Graph;

	class Vertex
	{
	protected:
		Storage::Stop* stop;
		Storage::TripType* trip;
		int pos;

		TripTimeType* hint_type;
		Trip* hint_trip;

		Vertex() : hint_type(0), hint_trip(0) {}
		void Set(Stop* stop, TripType* tt=NULL, int pos=0)
		{
			this->stop = stop;
			this->trip = tt;
			this->pos = pos;
		}
	public:
		Vertex(Stop* stop, TripType* tt=NULL, int pos=0) : stop(stop), trip(tt), pos(pos), hint_type(0), hint_trip(0) {}
		enum TravelType { Walk, Vehicle };

		list<pair<Vertex*, Distance>> Neighbors(Distance dist);

		inline Storage::Stop* Stop() const { return stop; }
		inline Storage::TripType* Trip() const { return trip; }
		inline int TripPos() const { return pos; }
		TravelType Type() const { return trip==NULL ? Walk : Vehicle; }

		friend class StopVertex;
		friend class TripVertex;
		friend class ReverseStopVertex;
		friend class ReverseTripVertex;
		friend class VertexArray<Vertex>;
		friend class Graph;

	};

	class Graph
	{
		VertexArray<Vertex> data;
		int size;
	public:
		DateTime Time;
		bool ForwardPlanning;

		Graph()
		{
			int stopSize = Storage::StopSize;
			Stop* stops = Storage::Stops;
			size = stopSize;
			for (int i=0; i<stopSize; i++)
			{
				Stop* curStop = stops + i;
				data.Get(curStop).Set(curStop);
			}
			int ttSize = Storage::TripTypeSize;
			TripType* tts = Storage::TripTypes;
			for (int i=0; i<ttSize; i++)
			{
				TripType* tt = tts + i;
				size += tt->StopEntriesSize;
				int pos = 0;
				for (StopEntry& entry : tt->StopEntries())
					data.Get(tt, pos).Set(entry.Stop(), tt, pos++);
			}
		}
		template <typename Action>
		void ForEach(Action action)
		{
			int stopSize = Storage::StopSize;
			Stop* stops = Storage::Stops;
			for (int i=0; i<stopSize; i++)
				action(data.Get(stops + i));
			int ttSize = Storage::TripTypeSize;
			TripType* tts = Storage::TripTypes;
			for (int i=0; i<ttSize; i++)
			{
				TripType* tt = tts + i;
				int maxPos = tt->StopEntriesSize;
				for (int pos=0; pos<maxPos; pos++)
					action(data.Get(tt, pos++));
			}
		}

		void Delete()
		{
			data.Delete();
		}

		Way CreateWay(VertexArray<Vertex*>& prev, VertexArray<Distance>& dist, Vertex* target);

		int Size() const { return size; }

		inline Vertex& Get(Storage::Stop* stop)
		{
			return data.Get(stop);
		}

		inline Vertex& Get(Storage::TripType* tt, int pos)
		{
			return data.Get(tt, pos);
		}
	};
	Graph* Graph1 = NULL;

	template <class CType>
	CType& VertexArray<CType>::operator[](const Vertex& v)
	{
		if (v.Type() == Vertex::Walk)
			return Get(v.Stop());
		else return Get(v.Trip(), v.TripPos());
	}
}

#endif