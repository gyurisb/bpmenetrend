#include "Storage.h"
#include "MyStdTypes.h"

#ifndef POSITIONSTOPS_H
#define POSITIONSTOPS_H

namespace Storage
{
	class PositionStops
	{
		Array2d<list<Stop*>*>* data;
		list<Stop*> empty;
        double minLon, minLat;
        double Res;

	public:
        PositionStops(double Res = 200.0) : Res(Res)
        {
            double minLon = numeric_limits<double>::max(), maxLon = -numeric_limits<double>::max();
            double minLat = numeric_limits<double>::max(), maxLat = -numeric_limits<double>::max();
			
            for (int i = 0; i < StopSize; i++)
            {
                if (Stops[i].Latitude < minLat)
                    minLat = Stops[i].Latitude;
                if (Stops[i].Longitude < minLon)
                    minLon = Stops[i].Longitude;
                if (Stops[i].Latitude > maxLat)
                    maxLat = Stops[i].Latitude;
                if (Stops[i].Longitude > maxLon)
                    maxLon = Stops[i].Longitude;
            }
            this->minLat = minLat;
            this->minLon = minLon;

            //double dLon = maxLon - minLon;
            //double dLat = maxLat - minLat;
            double distLon = Storage::Distance(0, minLon, 0, maxLon);
            double distLat = Storage::Distance(minLat, 0, maxLat, 0);

            int nLat = ceil(distLat / Res);
            int nLon = ceil(distLon / Res);

			data = new Array2d<list<Stop*>*>(nLat, nLon);
			
            for (int i = 0; i < StopSize; i++)
            {
                int j = (int)(d2mLat(Stops[i].Latitude - minLat) / Res);
                int k = (int)(d2mLon(Stops[i].Longitude - minLon) / Res);

				list<Stop*>*& curList = (*data)[j][k];
				if (curList == NULL) curList = new list<Stop*>();
                curList->push_back(Stops + i);
            }

        }
		~PositionStops()
		{
			for (list<Stop*>* cur : *data)
				delete[] cur;
			delete[] data;
		}



        list<Stop*> StopsAt(double latitude, double longitude)
        {
            int i = d2mLat(latitude - minLat) / Res;
            int k = d2mLon(longitude - minLon) / Res;

			list<Stop*> list(Data(i, k));
			std::list<Stop*> newList(Data(i + 1, k));
			list.insert(list.begin(), newList.begin(), newList.end());
			newList = Data(i - 1, k);
			list.insert(list.begin(), newList.begin(), newList.end());
			newList = Data(i, k + 1);
			list.insert(list.begin(), newList.begin(), newList.end());
			newList = Data(i, k - 1);
			list.insert(list.begin(), newList.begin(), newList.end());
			newList = Data(i + 1, k + 1);
			list.insert(list.begin(), newList.begin(), newList.end());
			newList = Data(i - 1, k - 1);
			list.insert(list.begin(), newList.begin(), newList.end());
			newList = Data(i + 1, k - 1);
			list.insert(list.begin(), newList.begin(), newList.end());
			newList = Data(i - 1, k + 1);
			list.insert(list.begin(), newList.begin(), newList.end());

			return list;
        }

        list<Stop*> StopsNear(Stop* stop)
        {
			double res = Res;
			return filter<Stop*>(StopsAt(stop->Latitude, stop->Longitude), [stop, res](Stop* s) { return stop->Distance(s) <= res; });
        }
		
	private:
        list<Stop*>& Data(int i, int k)
        {
            if (i < 0 || i >= data->GetLength(0) || k < 0 || k >= data->GetLength(1))
                return empty;
			list<Stop*>*& cur = (*data)[i][k];
			if (cur == NULL) return empty;
            return *cur;
        }
        static double d2mLat(double degree)
        {
            return Storage::Distance(0, 0, degree, 0);
        }
        static double d2mLon(double degree)
        {
            return Storage::Distance(0, 0, 0, degree);
        }
	};
	PositionStops* PositionStops1;

	list<Stop*> Stop::NearStops()
	{
		return PositionStops1->StopsNear(this);
	}
}

#endif