#include "MyStdTypes.h"
#include "Storage.h"

#ifndef SERVICE_CACHE_H
#define SERVICE_CACHE_H

namespace Storage
{
	uint16 shift_left(uint16 number, int count)
	{
		if (count >= 0)
			return number << count;
		else return number >> -count;
	}

	class ServiceCache
	{
		uint16* data;
		Date setDate;
		Date beginDate;
		Date endDate;
	public:
		ServiceCache(Date date = Date::Today())
		{
			int serviceSize = Storage::ServiceSize;
			data = new uint16[serviceSize];
			for (int i = 0; i<serviceSize; i++)
			{
				Service* service = Services + i;
				int idays = service->IDays;
				int wday = date.WeekDay();
				uint16 d = shift_left(idays, 8 - wday);
				d |= shift_left(idays, 15 - wday);
				d |= shift_left(idays, 1 - wday);
				d |= shift_left(idays, -6 - wday);

				setDate = date;
				beginDate = date - 8;
				endDate = date + 7;

				if (beginDate < service->StartDate)
					d &= ((uint16)~0) << ((Date)service->StartDate - beginDate);
				if (endDate > service->EndDate)
					d &= ((uint16)~0) >> (endDate - (Date)service->EndDate);

				for (CalendarException exc : service->Exceptions())
				{
					if ((Date)exc.Date >= beginDate && (Date)exc.Date <= endDate)
					{
						if (exc.Type == 1)
							d |= shift_left(1, (Date)exc.Date - date + 8);
						else
							d &= ~shift_left(1, (Date)exc.Date - date + 8);
					}
				}

				data[i] = d;
			}
		}

		bool IsInRange(Date date)
		{
			return date >= beginDate && date <= endDate;
		}

		bool IsActive(Service* service, Date date)
		{
			return data[service->Id()] & (1 << (date - setDate + 8));
		}

		friend void SetServiceCache(Date date);
	};

	ServiceCache* ServiceCache1 = NULL;

	void SetServiceCache(Date date)
	{
		if (ServiceCache1 == NULL || ServiceCache1->beginDate + 3 > date || ServiceCache1->endDate - 3 < date)
		{
			delete[] ServiceCache1;
			ServiceCache1 = new ServiceCache(date);
		}
	}

	bool Service::IsActive(Date date)
	{
		if (ServiceCache1->IsInRange(date))
			return ServiceCache1->IsActive(this, date);
		else return
			(
				date >= (Date)this->StartDate && date <= (Date)this->EndDate && (this->IDays & (1 << date.WeekDay()))
				&& !any(this->Exceptions(), [date](CalendarException exc) { return (Date)exc.Date == date && exc.Type == 2; })
			)
			|| any(this->Exceptions(), [date](CalendarException exc) { return (Date)exc.Date == date && exc.Type == 1; });
	}
	bool Service::IsActiveFast(Date date)
	{
		return ServiceCache1->IsActive(this, date);
	}
}

#endif