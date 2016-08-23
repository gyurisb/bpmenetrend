#include "Planning.h"
#include "ServiceCache.h"
#include "PositionStops.h"
#include "GraphImpl.h"
#pragma once

#ifdef WP80_PROJECT
namespace PlannerComponent_WP80
#else
namespace PlannerComponent
#endif
{
	public ref class MiddleEntry sealed
	{
		Platform::Array<int>^ stops;
		int routeID;
		int tripTypeID;
		int startStopID;
		int endStopID;
		int startTimeMinutes;
		int endTimeMinutes;
		int waitMinutes;
		int walkBeforeMeters;
		int stopCount;
	public:
		property int RouteID { int get() { return routeID; } }
		property int TripTypeID { int get() { return tripTypeID; } }
		property int StartStopID { int get() { return startStopID; } }
		property int EndStopID { int get() { return endStopID; } }
		property int StartTimeMinutes { int get() { return startTimeMinutes; } }
		property int EndTimeMinutes { int get() { return endTimeMinutes; } }
		property int WaitMinutes { int get() { return waitMinutes; } }
		property int WalkBeforeMeters { int get() { return walkBeforeMeters; } }
		property int StopCount { int get() { return stopCount; } }
		property Platform::Array<int>^ Stops { Platform::Array<int>^ get() { return stops; } }
	internal:
		MiddleEntry(Planning::WayEntry* entry)
		{
			routeID = entry->Route->Id() + 1;
			tripTypeID = entry->TripType->Id() + 1;
			startStopID = entry->StartStop->Id() + 1;
			endStopID = entry->EndStop->Id() + 1;
			startTimeMinutes = (entry->StartTime).TotalMinutes();
			endTimeMinutes = (entry->EndTime).TotalMinutes();
			waitMinutes = entry->WaitMinutes;
			walkBeforeMeters = entry->WalkBeforeMeters;
			stopCount = entry->StopCount;

			int i = 0;
			stops = ref new Platform::Array<int>(entry->size());
			for (Stop* stop : *entry)
				stops[i++] = stop->Id() + 1;
		}
	};
	public ref class MiddleWay sealed
	{
		int totalTime;
		double totalWalk;
		int totalTransfers;
		int lastWalkDistance;
		Platform::Array<MiddleEntry^>^ entries;
		//Debugger informations:
		Platform::String^ message;
	public:
		property int TotalTime { int get() { return totalTime; } }
		property double TotalWalk { double get() { return totalWalk; } }
		property int TotalTransfers { int get() { return totalTransfers; } }
		property int LastWalkDistance { int get() { return lastWalkDistance; } }
		property Platform::Array<MiddleEntry^>^ Entries { Platform::Array<MiddleEntry^>^ get() { return entries; } }
		property Platform::String^ Message { Platform::String^ get() { return message; } }
	internal:
		MiddleWay(Planning::Way& way)
		{
			entries = ref new Platform::Array<MiddleEntry^>(way.size());
			int i = 0;
			for (Planning::WayEntry& wayEntry : way)
				entries[i++] = ref new MiddleEntry(&wayEntry);

			totalTime = way.TotalDist.Time;
			totalWalk = way.TotalDist.WalkDistance;
			totalTransfers = way.TotalDist.TransferCount - 1;
			lastWalkDistance = way.LastWalkDistance;

			message = std::TimeMeasResult();
		}
	};
	MiddleWay^ ConvertWay(Planning::Way& way)
	{
		return ref new MiddleWay(way);
	}


	public enum class PlanningTimeType : int { Departure = 0, Arrival };
	public enum class PlanningAspect : int { Time = 0, TransferCount, WalkDistance };

	public value class PlanningArgs
	{
	public:
		//tervezési mezők
		PlanningTimeType Type;
		//plansettings mezők
		uint8 EnabledTypes; //a sorrend a tömbben: Tram, Metro, UrbanTrain, Bus
		bool OnlyWheelchairAccessibleTrips;
		bool OnlyWheelchairAccessibleStops;
		//config mezők
		//bool UseEstimation;
		double LatitudeDegreeDistance;
		double LongitudeDegreeDistance;
		double WalkSpeedRate;
	};
	public value class TripTimePair
	{
	public:
		int TimeDifference;
		int TripID;
	};

	ref class PlannerRuntimeComponent;
	PlannerRuntimeComponent^ instance = nullptr;
	public ref class PlannerRuntimeComponent sealed
	{
		bool reservedData;
		PlanningTimeType type;
		//bool useEstimation;

		template <typename EType>
		EType* readFile(Platform::String^ fileName, int& dataLength)
		{
			FILE* file;
			_wfopen_s(&file, fileName->Data(), L"rb");
			fread(&dataLength, sizeof(int), 1, file);
			EType* data = new EType[dataLength];
			fread(data, sizeof(EType), dataLength, file);
			fclose(file);
			return data;
		}
		template <typename EType>
		EType* readFile(Platform::String^ fileName)
		{
			int dLength;
			return readFile<EType>(fileName, dLength);
		}
	public:

		PlannerRuntimeComponent(
			Platform::String^ TripFileName,
			Platform::String^ TripTypeFileName,
			Platform::String^ RouteFileName,
			Platform::String^ RouteGroupFileName,
			Platform::String^ ServiceFileName,
			Platform::String^ CalendarExceptionFileName,
			Platform::String^ StopFileName,
			Platform::String^ StopGroupFileName,
			Platform::String^ StopEntryFileName,
			Platform::String^ TTEntryFileName,
			Platform::String^ TimeEntryFileName,
			Platform::String^ TripTimeTypeFileName,
			Platform::String^ TransfersFileName
			)
		{
			if (instance != nullptr) throw "SingletonException";
			instance = this;

			reservedData = true;
			Storage::Trips = readFile<Storage::Trip>(TripFileName);
			Storage::TripTypes = readFile<Storage::TripType>(TripTypeFileName, Storage::TripTypeSize);
			Storage::Routes = readFile<Storage::Route>(RouteFileName);
			Storage::RouteGroups = readFile<Storage::RouteGroup>(RouteGroupFileName);
			Storage::Services = readFile<Storage::Service>(ServiceFileName, Storage::ServiceSize);
			Storage::CalendarExceptions = readFile<Storage::CalendarException>(CalendarExceptionFileName);
			Storage::Stops = readFile<Storage::Stop>(StopFileName, Storage::StopSize);
			Storage::StopGroups = readFile<Storage::StopGroup>(StopGroupFileName);
			Storage::StopEntries = readFile<Storage::StopEntry>(StopEntryFileName);
			Storage::TTEntries = readFile<Storage::TTEntry>(TTEntryFileName);
			Storage::TimeEntries = readFile<Storage::TimeEntry>(TimeEntryFileName);
			Storage::TripTimeTypes = readFile<Storage::TripTimeType>(TripTimeTypeFileName);
			Storage::Transfers = readFile<Storage::Transfer>(TransfersFileName);

			Storage::ServiceCache1 = new Storage::ServiceCache();
			//Storage::PositionStops1 = new Storage::PositionStops();
			Planning::Graph1 = new Planning::Graph();
		}

		Platform::Array<TripTimePair>^ GetCurrentTrips(Platform::String^ currentTimeValue, int routeId, const Platform::Array<int>^ stopIds, int prevCount, int nextCount, int timeLimit)
		{
			DateTime currentTime = currentTimeValue->Data();
			DateTime time = currentTime;
			Route* route = Storage::Routes + routeId - 1;
			list<Stop*> stops;
			for (int id : stopIds) stops.push_back(Storage::Stops + id - 1);

			Platform::Array<TripTimePair>^ arr = ref new Platform::Array<TripTimePair>(prevCount + nextCount);
			for (int i = 0; i < nextCount; i++)
			{
				list<ArrivalResult> arrivalList;
				for (Stop* stop : stops)
				{
					ArrivalResult curRes = stop->NextArrivalRoute(time, route);
					if (!curRes.IsNull())
						arrivalList.push_back(curRes);
				}
				if (!arrivalList.empty())
				{
					ArrivalResult& res = minBy<ArrivalResult>(arrivalList.begin(), arrivalList.end(), [](ArrivalResult& a, ArrivalResult& b) { return a.ArrivalTime < b.ArrivalTime; });

					TripTimePair pair;
					pair.TimeDifference = (time + res.ArrivalTime - currentTime).TotalMinutes();
					pair.TripID = res.ArrivalTrip->Id() + 1;
					arr[prevCount + i] = pair;

					time = time + res.ArrivalTime + TimeSpan::FromMinutes(1);
				}
				else break;
			}
			time = currentTime - TimeSpan::FromMinutes(1);
			for (int i = 0; i < prevCount; i++)
			{
				list<ArrivalResult> arrivalList;
				for (Stop* stop : stops)
				{
					ArrivalResult curRes = stop->PrevArrivalRoute(time, route);
					if (!curRes.IsNull())
						arrivalList.push_back(curRes);
				}
				if (!arrivalList.empty())
				{
					ArrivalResult res = minBy<ArrivalResult>(arrivalList, [](ArrivalResult a, ArrivalResult b) { return a.ArrivalTime > b.ArrivalTime; });

					TripTimePair pair;
					pair.TimeDifference = (time + res.ArrivalTime - currentTime).TotalMinutes();
					pair.TripID = res.ArrivalTrip->Id() + 1;
					arr[prevCount - i - 1] = pair;

					time = time + res.ArrivalTime - TimeSpan::FromMinutes(1);
				}
				else break;
			}

			return arr;
		}

		void SetParams(PlanningArgs args)
		{
			if ((type = args.Type) == PlanningTimeType::Departure)
				Planning::Graph1->ForwardPlanning = true;
			else
				Planning::Graph1->ForwardPlanning = false;
			Storage::RouteEnabled = args.EnabledTypes;
			Storage::NotAllRoutesEnabled = args.EnabledTypes != 255;
			Storage::OnlyBarrierFreeStop = args.OnlyWheelchairAccessibleStops;
			Storage::OnlyBarrierFreeTrip = args.OnlyWheelchairAccessibleTrips;
			//useEstimation = args.UseEstimation;
			Storage::LatitudeDegreeDistance = args.LatitudeDegreeDistance;
			Storage::LongitudeDegreeDistance = args.LongitudeDegreeDistance;
			Planning::WALK_SPEED_RATE = args.WalkSpeedRate;
		}

		MiddleWay^ CalculatePlanning(int sourceId, int targetId, Platform::String^ startTime, PlanningAspect aspect)
		{
			if (type == PlanningTimeType::Arrival)
				swap(sourceId, targetId);
			Storage::StopGroup* source = Storage::StopGroups + sourceId - 1;
			Storage::StopGroup* target = Storage::StopGroups + targetId - 1;
			Planning::Graph1->Time = (DateTime)startTime->Data();
			Storage::SetServiceCache(Planning::Graph1->Time);

			Planning::Way way;
			/*if (!useEstimation)
			way = calculatePlanning(source, target, aspect, Calculator());
			else
			way = calculatePlanning(source, target, aspect, EstimateCalculator());*/
			way = calculatePlanning(source, target, aspect, Calculator());
			if (way.empty() && !way.LastWalkDistance) return nullptr;
			return ConvertWay(way);
		}
	private:
		struct Calculator {
			template <typename Pred> Planning::Way operator()(Storage::StopGroup* source, Storage::StopGroup* target, Pred pred) {
				return Planning::Calculate(source, target, pred);
			}
		};
		/*struct EstimateCalculator {
		template <typename Pred> Planning::Way operator()(Storage::StopGroup* source, Storage::StopGroup* target, Pred pred) {
		return Planning::CalculateAStar(source, target, pred);
		}
		};*/
		template <typename CalcFunc>
		Planning::Way calculatePlanning(Storage::StopGroup* source, Storage::StopGroup* target, PlanningAspect aspect, CalcFunc calcFunc)
		{
			switch (aspect)
			{
			case PlanningAspect::Time:
				return calcFunc(source, target, Planning::Distance::CompareTime());
			case PlanningAspect::TransferCount:
				return calcFunc(source, target, Planning::Distance::CompareTransfer());
			case PlanningAspect::WalkDistance:
				return calcFunc(source, target, Planning::Distance::CompareWalk());
			default:
				return Planning::Way();
			}
		}
		~PlannerRuntimeComponent()
		{
			Close(false);
		}
	public:
		void Close(bool safe)
		{
			delete[] Storage::ServiceCache1;
			if (!safe)
			{
				delete[] Planning::Graph1;
				//delete[] Storage::PositionStops1;
			}
			else
			{
				Planning::Graph1->Delete();
			}

			if (reservedData)
			{
				delete[] Storage::Trips;
				delete[] Storage::TripTypes;
				delete[] Storage::Routes;
				delete[] Storage::RouteGroups;
				delete[] Storage::Services;
				delete[] Storage::CalendarExceptions;
				delete[] Storage::Stops;
				delete[] Storage::StopGroups;
				delete[] Storage::StopEntries;
				delete[] Storage::TTEntries;
				delete[] Storage::TimeEntries;
				delete[] Storage::TripTimeTypes;
			}

			instance = nullptr;
		}
	};
}
