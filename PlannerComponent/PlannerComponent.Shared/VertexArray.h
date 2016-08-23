#include "Storage.h"

#ifndef VERTEXARRAY_H
#define VERTEXARRAY_H

namespace Planning
{
	class Vertex;

	template<class CType>
	class VertexArray
	{
		CType* stopData;
		CType** tripData;

		VertexArray(const VertexArray& other) {}
		VertexArray& operator=(const VertexArray& other) {}
	public:
		VertexArray()
		{
			stopData = new CType[Storage::StopSize]();
			int ttSize = Storage::TripTypeSize;
			tripData = new CType*[ttSize];
			for (int i=0; i<ttSize; i++)
				tripData[i] = new CType[(Storage::TripTypes + i)->StopEntriesSize]();
		}
		~VertexArray()
		{
			delete[] stopData;
			int ttSize = Storage::TripTypeSize;
			for (int i=0; i<ttSize; i++)
				delete[] tripData[i];
			delete[] tripData;
		}
		void Delete()
		{
			delete[] stopData;
			int ttSize = Storage::TripTypeSize;
			for (int i=0; i<ttSize; i++)
				delete[] tripData[i];
			delete[] tripData;
		}

		CType& operator [](const Vertex& v); //defined in Graphs.h

		inline CType& Get(Storage::Stop* stop)
		{
			return stopData[stop->Id()];
		}
		inline CType& Get(Storage::TripType* trip, int pos)
		{
			return tripData[trip->Id()][pos];
		}
	};
}

#endif