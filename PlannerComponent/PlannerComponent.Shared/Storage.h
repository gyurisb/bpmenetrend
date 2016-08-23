#include "MyStdTypes.h"

#ifndef STORAGE_H
#define STORAGE_H

using namespace std;

namespace Storage
{	
	struct Trip;
	struct TripType;
	struct Route;
	struct RouteGroup;
	struct Service;
	struct CalendarException;
	struct Stop;
	struct Transfer;
	struct StopGroup;
	struct StopEntry;
	struct TTEntry;
	struct TimeEntry;
	struct TripTimeType;
	struct TripResult;

	struct ArrivalResult;

	Trip* Trips;
	TripType* TripTypes;
	Route* Routes;
	RouteGroup* RouteGroups;
	Service* Services;
	CalendarException* CalendarExceptions;
	Stop* Stops;
	Transfer* Transfers;
	StopGroup* StopGroups;
	StopEntry* StopEntries;
	TTEntry* TTEntries;
	TimeEntry* TimeEntries;
	TripTimeType* TripTimeTypes;

	int StopSize = 0;
	int TripTypeSize = 0;
	int ServiceSize = 0;

	double LatitudeDegreeDistance = 101000.0;
	double LongitudeDegreeDistance = 101000.0;
	
	#pragma pack(push, 1)
	struct Service
	{
		int IDays;
		uint32 StartDate;
		uint32 EndDate;
		int ExceptionsId;
		size_t ExceptionsSize;

		ArrayPt<CalendarException> Exceptions();
		int Id() { return this - Services; }
		bool IsActive(Date date);
		bool IsActiveFast(Date date);
		static const int SIZE = 20;
	};
	struct CalendarException
	{
		uint32 Date;
		int Type;
	};

	struct RouteGroup
	{
		int NameStrId;
		int DescriptionStrId;
		uint8 Type;
		uint32 BgColor;
		uint32 FontColor;
		int AgencyId;
		int RoutesId;
		size_t RoutesSize;
	};
	struct Route
	{
		int NameStrId;
		int RouteGroupId2;
		int RouteGroupId;
		int TripTypesId;
		size_t TripTypesSize;
		RouteGroup* RouteGroup() { return RouteGroups + RouteGroupId - 1; }
		int Id() { return this - Routes; }
	};
	struct TripType
	{
		int NameStrId;
		int RouteId;
		int ShapeId;
		int TripTimeTypesId;
		size_t TripTimeTypesSize;
		int StopEntriesId;
		size_t StopEntriesSize;
		//int HeadsignsId;
		//size_t HeadsignsSize;

		ArrayPt<StopEntry> StopEntries();
		ArrayPt<TripTimeType> TripTimeTypes();
		Route* Route() { return Routes + RouteId - 1; }
		int Id() { return this - TripTypes; }
	};
	struct Trip
	{
		uint16 WheelchairAndStartTime;
		int ServiceId;

		Service* Service() { return Services + ServiceId - 1; }
		int Id() { return this - Trips; }

		bool WheelchairAccessible()
		{
			if (WheelchairAndStartTime & 0x0040)
				return false;
			if (WheelchairAndStartTime & 0x0080)
				return true;
			return false;
		}

		TimeSpan StartTime()
		{
			uint16 time = WheelchairAndStartTime & 0xFF3F;
			return TimeSpan(time & 0x00ff, time >> 8);
		}
	};

	struct StopGroup
	{
		int NameStrId;
		int StopsId;
		size_t StopsSize;
		ArrayPt<Stop> Stops();
	};
	struct Stop
	{
		int NameStrId;
		double Latitude;
		double Longitude;
		uint8 WheelchairBoarding;
		int StopGroupId;
		int TTEntriesId; //TimeEntry2
		size_t TTEntriesSize;
		int TransfersId;
		size_t TransfersSize;

		int Id() { return this - Stops; }
		StopGroup* Group() { return StopGroups + StopGroupId - 1; }
		list<Stop*> NearStops();
		ArrayPt<TTEntry> TTEntries();
		ArrayPt<Transfer> Transfers();
		list<ArrivalResult> NextArrivalTripTypes(DateTime time);
		list<ArrivalResult> PrevArrivalTripTypes(DateTime time);
		ArrivalResult NextArrivalRoute(DateTime time, Route* route);
		ArrivalResult PrevArrivalRoute(DateTime time, Route* route);
		ArrivalResult FindNextTrip(DateTime time, TripType* tt, int pos);
		ArrivalResult FindPrevTrip(DateTime time, TripType* tt, int pos);
		double Distance(Stop* stop);
	private:
		TripResult findNextTrip(ArrayPt<Trip> trips, std::Date date, TimeSpan time);
		TripResult nextArrival(TripTimeType* tt, short travelTime, DateTime time);
		TripResult findPrevTrip(ArrayPt<Trip> trips, std::Date date, TimeSpan time);
		TripResult prevArrival(TripTimeType* tt, short travelTime, DateTime time);
		template <class Func> ArrivalResult arrivalRoute(DateTime time, Route* route, Func nextPrevArrival);
	};
	struct Transfer
	{
		short Distance;
		int TargeId;
		int TransferPointsId;
		size_t TransferPointsSize;

		Stop* Target() { return Stops + TargeId - 1; }
	};

	struct StopEntry
	{
		int StopId;
		Stop* Stop() { return Stops + StopId - 1; }
	};
	struct TTEntry
	{
		int Position;
		int TripTypeId;
		TripType* TripType() { return TripTypes + TripTypeId - 1; }
	};
	struct TimeEntry
	{
		short Time;
	};
	struct TripTimeType
	{
		int TimeEntriesId;
		size_t TimeEntriesSize;
		int TripsId;
		size_t TripsSize;
		ArrayPt<TimeEntry> TimeEntries() { return ArrayPt<TimeEntry>(Storage::TimeEntries + TimeEntriesId - 1, TimeEntriesSize); }
		ArrayPt<Trip> Trips() { return ArrayPt<Trip>(Storage::Trips + TripsId - 1, TripsSize); }
	};
	#pragma pack(pop)

	//enabling for planning:

	uint8 RouteEnabled = 255;
	bool NotAllRoutesEnabled = false;
	bool OnlyBarrierFreeTrip = false;
	bool OnlyBarrierFreeStop = false;

	inline bool TripEnabled(Trip* trip)
	{
		return !OnlyBarrierFreeTrip || trip->WheelchairAccessible();
	}
	inline bool TripTypeEnabled(TripType* tt)
	{
		if (NotAllRoutesEnabled)
			return RouteEnabled & (1 << tt->Route()->RouteGroup()->Type);
		return true;
	}
	inline bool StopEnabled(Stop* stop)
	{
		return !OnlyBarrierFreeStop || stop->WheelchairBoarding == 1;
	}


	//definitions:
	
	ArrayPt<StopEntry> TripType::StopEntries()
	{
		return ArrayPt<StopEntry>(Storage::StopEntries + StopEntriesId - 1, StopEntriesSize);
	}
	ArrayPt<TripTimeType> TripType::TripTimeTypes()
	{
		return ArrayPt<TripTimeType>(Storage::TripTimeTypes + TripTimeTypesId - 1, TripTimeTypesSize);
	}
	ArrayPt<TTEntry> Stop::TTEntries()
	{
		return ArrayPt<TTEntry>(Storage::TTEntries + TTEntriesId - 1, TTEntriesSize);
	}
	ArrayPt<Transfer> Stop::Transfers()
	{
		return ArrayPt<Transfer>(Storage::Transfers + TransfersId - 1, TransfersSize);
	}
	ArrayPt<CalendarException> Service::Exceptions()
	{
		return ArrayPt<CalendarException>(CalendarExceptions + ExceptionsId - 1, ExceptionsSize);
	}
	ArrayPt<Stop> StopGroup::Stops()
	{
		return ArrayPt<Stop>(Storage::Stops + this->StopsId - 1, this->StopsSize);
	}
	
	struct ArrivalResult
	{
		TimeSpan ArrivalTime;
		TripType* Trip;
		TripTimeType* Type;
		short Position;
		Storage::Trip* ArrivalTrip;
		ArrivalResult(TimeSpan time, TripType* trip, TripTimeType* type, short pos, Storage::Trip* arrivalTrip=NULL) : ArrivalTime(time), Trip(trip), Type(type), Position(pos), ArrivalTrip(arrivalTrip) {}
		static ArrivalResult Null() { return ArrivalResult(TimeSpan::MaxValue(), NULL, NULL, 0); } 
		//bool IsNull() { return ArrivalTime == TimeSpan::MaxValue(); }
		inline bool IsNull() const { return Trip == NULL; }
	};
	struct TripResult
	{
		TimeSpan Time;
		Trip* Trip;
		TripResult(TimeSpan time, Storage::Trip* trip) : Time(time), Trip(trip) {}
		static TripResult Null() { return TripResult(TimeSpan::MaxValue(), NULL); } 
		bool operator==(const TripResult& res) const { return Time == res.Time && Trip == res.Trip; }
		bool operator!=(const TripResult& res) const { return Time != res.Time || Trip != res.Trip; }
		inline bool operator<(const TripResult& res) const { return Time < res.Time; }
		inline bool operator>(const TripResult& res) const { return Time > res.Time; }
		inline bool IsNull() const { return Trip == NULL; }
	};
	
	ArrivalResult Stop::NextArrivalRoute(DateTime time, Route* route)
	{
		return arrivalRoute(time, route, [this](TripTimeType* tt, short travelTime, DateTime time) { return nextArrival(tt, travelTime, time); });
	}
	ArrivalResult Stop::PrevArrivalRoute(DateTime time, Route* route)
	{
		return arrivalRoute(time, route, [this](TripTimeType* tt, short travelTime, DateTime time) { return prevArrival(tt, travelTime, time); });
	}
	template <class Func>
	ArrivalResult Stop::arrivalRoute(DateTime time, Route* route, Func nextPrevArrival)
	{
		list<ArrivalResult> list;
		for (TTEntry& entry : TTEntries())
        {
			TripType* tt = entry.TripType();
			if (tt->Route() == route)
            {
				for (TripTimeType& timeType : tt->TripTimeTypes())
				{
					short travelTime = timeType.TimeEntries()[entry.Position].Time;
					TripResult nextArrivalTime = nextPrevArrival(&timeType, travelTime, time);
					if (!nextArrivalTime.IsNull())
						list.push_back(ArrivalResult(
							nextArrivalTime.Time,
							tt,
							&timeType,
							entry.Position,
							nextArrivalTime.Trip
						));
				}
            }
        }
		if (list.size() == 0) return ArrivalResult::Null();
		return minBy<ArrivalResult>(list.begin(), list.end(), [](ArrivalResult& a, ArrivalResult& b) { return abs(a.ArrivalTime.TotalMinutes()) < abs(b.ArrivalTime.TotalMinutes()); });
	}

	ArrivalResult Stop::FindNextTrip(DateTime time, TripType* tt, int pos)
	{
		for (TripTimeType& timeType : tt->TripTimeTypes())
		{
			short travelTime = timeType.TimeEntries()[pos].Time;
			TripResult nextArrivalTime = nextArrival(&timeType, travelTime, time);
			if (nextArrivalTime.Time == TimeSpan::Zero())
				return ArrivalResult(
					nextArrivalTime.Time,
					tt,
					&timeType,
					pos,
					nextArrivalTime.Trip
				);
		}
		throw "InvalidOperationException";
	}
	ArrivalResult Stop::FindPrevTrip(DateTime time, TripType* tt, int pos)
	{
		for (TripTimeType& timeType : tt->TripTimeTypes())
		{
			short travelTime = timeType.TimeEntries()[pos].Time;
			TripResult nextArrivalTime = prevArrival(&timeType, travelTime, time);
			if (nextArrivalTime.Time == TimeSpan::Zero())
				return ArrivalResult(
					nextArrivalTime.Time,
					tt,
					&timeType,
					pos,
					nextArrivalTime.Trip
				);
		}
		throw "InvalidOperationException";
	}
	list<ArrivalResult> Stop::NextArrivalTripTypes(DateTime time)
	{
		list<ArrivalResult> retList;
		for (TTEntry& entry : TTEntries())
        {
			TripType* tt = entry.TripType();
			if (TripTypeEnabled(tt))
            {
				list<ArrivalResult> timeList;
				for (TripTimeType& timeType : tt->TripTimeTypes())
				{
					short travelTime = timeType.TimeEntries()[entry.Position].Time;
					TripResult nextArrivalTime = nextArrival(&timeType, travelTime, time);
					if (!nextArrivalTime.IsNull())
						timeList.push_back(ArrivalResult(
							nextArrivalTime.Time,
							tt,
							&timeType,
							entry.Position,
							nextArrivalTime.Trip
						));
				}
				if (!timeList.empty())
					retList.push_back(
						minBy<ArrivalResult>(timeList.begin(), timeList.end(), [](ArrivalResult& a, ArrivalResult& b) { return a.ArrivalTime < b.ArrivalTime; })
					);
            }
        }
		return retList;
	}

	list<ArrivalResult> Stop::PrevArrivalTripTypes(DateTime time)
    {
		/*for (TimeEntry2& ttAndTime : TimeEntries())
        {
			if (TripTypeEnabled(ttAndTime.TripType()))
            {
                TimeSpan prevArrivalTime = prevArrival(ttAndTime.TripType(), ttAndTime.Time, time);
                if (prevArrivalTime != TimeSpan::MaxValue())
					list.push_back(ArrivalResult(
                        prevArrivalTime,
                        ttAndTime.TripType(),
                        ttAndTime.Time
					));
            }
        }*/
		list<ArrivalResult> retList;
		for (TTEntry& entry : TTEntries())
        {
			TripType* tt = entry.TripType();
			if (TripTypeEnabled(tt))
            {
				list<ArrivalResult> timeList;
				for (TripTimeType& timeType : tt->TripTimeTypes())
				{
					short travelTime = timeType.TimeEntries()[entry.Position].Time;
					TripResult prevArrivalTime = prevArrival(&timeType, travelTime, time);
					if (!prevArrivalTime.IsNull())
						timeList.push_back(ArrivalResult(
							prevArrivalTime.Time,
							tt,
							&timeType,
							entry.Position,
							prevArrivalTime.Trip
						));
				}
				if (!timeList.empty())
					retList.push_back(
						minBy<ArrivalResult>(timeList.begin(), timeList.end(), [](ArrivalResult& a, ArrivalResult& b) { return a.ArrivalTime > b.ArrivalTime; })
					);
            }
        }
		return retList;
    }

	TripResult Stop::findNextTrip(ArrayPt<Trip> trips, Date date, TimeSpan time)
    {
		if (time > trips[trips.size() - 1].StartTime())
			return TripResult::Null();
		int index = 0;
		if (time > trips[0].StartTime()) 
			index = binarySearch(trips.begin(), trips.size(), time, [](Trip trip) { return trip.StartTime(); });
        if (index > 0)
        {
            for (int i = index - 1; i >= 0 && trips[i].StartTime() == trips[index].StartTime(); i--)
                if (trips[i].Service()->IsActiveFast(date) && TripEnabled(&trips[i]))
                    return TripResult(trips[i].StartTime() - time, &trips[i]);
        }
        for (int i = index < 0 ? -index - 1 : index; i < trips.size(); i++)
            if (trips[i].Service()->IsActiveFast(date) && TripEnabled(&trips[i]))
				return TripResult(trips[i].StartTime() - time, &trips[i]);
        return TripResult::Null();
    }
	TripResult Stop::findPrevTrip(ArrayPt<Trip> trips, Date date, TimeSpan time)
    {
		if (time < trips[0].StartTime())
			return TripResult::Null();
		int index = trips.size() - 1;
		if (time < trips[trips.size() - 1].StartTime()) 
			index = binarySearch(trips.begin(), trips.size(), time, [](Trip trip) { return trip.StartTime(); });
        if (index >= 0)
        {
			//javaslat: index => i++, javítva
            for (int i = index; i < trips.size() && trips[i].StartTime() == trips[index].StartTime(); i++)
                if (trips[i].Service()->IsActiveFast(date) && TripEnabled(&trips[i]))
                    return TripResult(trips[i].StartTime() - time, &trips[i]);
        }
        for (int i = (index < 0 ? -index - 1 : index) - 1; i >= 0; i--)
            if (trips[i].Service()->IsActive(date) && TripEnabled(&trips[i]))
                return TripResult(trips[i].StartTime() - time, &trips[i]);
		return TripResult::Null();
    }

	TripResult Stop::nextArrival(TripTimeType* tt, short travelTime, DateTime time)
    {
        ArrayPt<Trip> trips = tt->Trips();
        if (trips.size() == 0) return TripResult::Null();
        vector<TripResult> list;
		list.reserve(2);

        DateTime startTime = time - TimeSpan::FromMinutes(travelTime);

        //A mai járat keresése
        TripResult trip0 = findNextTrip(trips, startTime, startTime.TimeOfDay());
        if (!trip0.IsNull())
            list.push_back(trip0);

        //A tegnapi éjszakai járat keresése (amely átnyúlik mába)
        TimeSpan startTimePlus24Hour = startTime.TimeOfDay() + TimeSpan::FromDays(1);
        TripResult trip1 = findNextTrip(trips, (Date)startTime - 1, startTimePlus24Hour);
        if (!trip1.IsNull())
            list.push_back(trip1);

        if (list.size() == 1)
		{
            return list.front();
		}
        else if (list.size() == 2)
        {
            return min(list.front(), list.back());
        }
        else
        {
            //Ha nem találtunk járatot, a holnapi elsõt választjuk ki
            TripResult trip2 = findNextTrip(trips, (Date)startTime + 1, TimeSpan::Zero());
            if (!trip2.IsNull())
                return TripResult(TimeSpan::FromDays(1) - startTime.TimeOfDay() + trip2.Time, trip2.Trip);
			else
				return TripResult::Null();

        }
    }
	TripResult Stop::prevArrival(TripTimeType* tt, short travelTime, DateTime time)
    {
		ArrayPt<Trip> trips = tt->Trips();
        if (trips.size() == 0) return TripResult::Null();
        list<TripResult> list;

        DateTime startTime = time - TimeSpan::FromMinutes(travelTime);

        //A mai járat keresése
        TripResult trip0 = findPrevTrip(trips, startTime, startTime.TimeOfDay());
        if (!trip0.IsNull())
            list.push_back(trip0);

        //A tegnapi éjszakai járat keresése (amely átnyúlik mába)
        TimeSpan startTimePlus24Hour = startTime.TimeOfDay() + TimeSpan::FromDays(1);
        TripResult trip1 = findPrevTrip(trips, (Date)startTime - 1, startTimePlus24Hour);
        if (!trip1.IsNull())
            list.push_back(trip1);
		
        if (list.size() == 1)
            return list.front();
        else if (list.size() == 2)
        {
            return max(list.front(), list.back());
        }
        else
        {
            //Ha nem találtunk járatot, akkor a tegnapi utolsót választjuk ki
            TripResult trip2 = findPrevTrip(trips, (Date)startTime - 1, TimeSpan::FromDays(1));
            if (!trip2.IsNull())
                //return TimeSpan.FromDays(1) - startTime.TimeOfDay + trip2.Value;
                return TripResult(trip2.Time - startTime.TimeOfDay(), trip2.Trip);
			else
				return TripResult::Null();
        }
    }

	
    //the distance from the other stop in meters
	double Stop::Distance(Stop* other)
    {
		double dlat = (other->Latitude - Latitude)*LatitudeDegreeDistance;
		double dlon = (other->Longitude - Longitude)*LongitudeDegreeDistance;
        return sqrt(dlat*dlat + dlon*dlon);
    }

	double Distance(double lat1, double lon1, double lat2, double lon2)
    {
		double dlat = (lat1 - lat2)*LatitudeDegreeDistance;
		double dlon = (lon1 - lon2)*LongitudeDegreeDistance;
        return sqrt(dlat*dlat + dlon*dlon);
    }
}

#endif