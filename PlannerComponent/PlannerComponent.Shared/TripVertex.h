 #include "Graphs.h"

#ifndef TRIPVERTEX_H
#define TRIPVERTEX_H


namespace Planning
{
	//long HintSuccess;
	//long HintFailed;

	class TripVertex : public Vertex
	{
	protected:
		TripVertex() {}
	public:			
		static list<pair<Vertex*, Distance>> Neighbors(Vertex& vertex, Distance dist)
		{
			//STrip.Start();
			list<pair<Vertex*, Distance>> list;
            if (Storage::StopEnabled(vertex.stop))
				list.push_back(std::make_pair(&Graph1->Get(vertex.stop), Distance::Zero()));

            if (vertex.pos < vertex.trip->StopEntriesSize - 1)
			{
				DateTime time = Graph1->Time + TimeSpan::FromMinutes(dist.Time);
				TripTimeType* type = NULL;
				Vertex* nextVertex = &Graph1->Get(vertex.trip, vertex.pos + 1);
				if (checkHint(vertex, time))
				{
					//HintSuccess++;
					type = vertex.hint_type;
					nextVertex->hint_trip = vertex.hint_trip;
					nextVertex->hint_type = vertex.hint_type;
				}
				else
				{
					//HintFailed++;
					ArrivalResult res = vertex.stop->FindNextTrip(time, vertex.trip, vertex.pos);
					type = res.Type;
					nextVertex->hint_trip = res.ArrivalTrip;
					nextVertex->hint_type = res.Type;
				}
				short travelTime = type->TimeEntries()[vertex.pos + 1].Time - type->TimeEntries()[vertex.pos].Time;
                list.push_back(make_pair(
                        nextVertex,
                        Distance(travelTime, 0, 0)
                    ));
			}
			//STrip.Stop();
			return list;
		}

		static bool checkHint(Vertex& v, std::DateTime time)
		{
			if (v.hint_trip == NULL) return false;
			/*return	v.hint_trip->Service()->IsActive(time) && 
					v.hint_trip->StartTime() + TimeSpan::FromMinutes(v.hint_type->TimeEntries()[v.pos].Time) == time.TimeOfDay();*/
			TimeSpan arriveTime = v.hint_trip->StartTime() + TimeSpan::FromMinutes(v.hint_type->TimeEntries()[v.pos].Time);
			if (arriveTime < TimeSpan::FromDays(1))
				return v.hint_trip->Service()->IsActive(time) && arriveTime == time.TimeOfDay();
			else
				return v.hint_trip->Service()->IsActive(time - TimeSpan::FromDays(1)) && arriveTime == time.TimeOfDay() + TimeSpan::FromDays(1);
		}
	};
}

#endif