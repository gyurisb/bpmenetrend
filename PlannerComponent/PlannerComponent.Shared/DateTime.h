#include <time.h>
#include <stdlib.h>
#include <string>
#include "TimeSpan.h"

#ifndef DATETIME_H
#define DATETIME_H

namespace std
{
	class DateTime;

	int TimeZoneShift = 1;
	
	class Date
	{
		uint16 date;
		Date(uint16 date, bool val) : date(date) {}
	public:
		Date()
		{
			createFrom(time(0));
		}
		Date(uint32 data)
		{
			uint16 year = *((uint16*)&data);
			uint8 month = *(((uint8*)&data) + 2);
			uint8 day = *(((uint8*)&data) + 3);
			createFrom(year, month, day);
		}
		Date(int year, int month, int day) { createFrom(year, month, day); }
		Date(const DateTime& dateTime);
		/*int Year() const { return date >> 16; }
		int Month() const { return (date & 0x0000ff00) >> 8; }
		int Day() const { return date & 0xff; }*/
		int WeekDay() const;

		int operator-(const Date& other) const { return date - other.date; }
		Date operator+(int days) const { return Date(date + days, true); }
		Date operator-(int days) const { return Date(date - days, true); }
		inline int operator<=(const Date& other) const { return date <= other.date; }
		inline int operator>=(const Date& other) const { return date >= other.date; }
		inline int operator<(const Date& other) const { return date < other.date; }
		inline int operator>(const Date& other) const { return date > other.date; }
		inline bool operator==(const Date& other) const { return date == other.date; }

		static Date Today() { return Date(); }
		friend class DateTime;
	private:
		void createFrom(int year, int month, int day)
		{
			tm tm;
			tm.tm_year = year - 1900; tm.tm_mon = month - 1; tm.tm_mday = day;
			tm.tm_hour = 12;
			tm.tm_sec = tm.tm_min = 0;
			createFrom(mktime(&tm));
		}
		inline void createFrom(time_t time) { date = time / (60*60*24); }
	};

	class DateTime
	{
		time_t dateTime;
		DateTime(time_t time) : dateTime(time) {}
	public:
		DateTime() : dateTime(time(0)) {}
		DateTime(int year, int month, int day) { createFrom(year, month, day); }
		DateTime(Date date) { dateTime = date.date * (60*60*24); }
		DateTime(const wchar_t* stdtime)
		{
			wchar_t ampm[64];
			tm tm;
			swscanf_s(stdtime, L"%d/%d/%d %d:%d:%d",
				&tm.tm_mon, &tm.tm_mday, &tm.tm_year, &tm.tm_hour, &tm.tm_min, &tm.tm_sec);
			int tmHour = tm.tm_hour;
			tm.tm_year -= 1900; tm.tm_mon -= 1;
			dateTime = mktime(&tm);
			int curHour = TimeOfDay().TotalHours();
			if (tmHour < curHour) tmHour += 24;
			dateTime += (tmHour - curHour)*3600;
		}
		DateTime operator+(const TimeSpan& other) const { return DateTime(dateTime + other.TotalSeconds()); }
		DateTime operator-(const TimeSpan& other) const { return DateTime(dateTime - other.TotalSeconds()); }
		TimeSpan operator-(const DateTime& other) const { return TimeSpan(int((dateTime - other.dateTime)/60)); }
		
		TimeSpan TimeOfDay() const { return *this - (DateTime)(Date)*this; }
		/*void localtime(tm* tm) { localtime_s(tm, &dateTime); }
		int Year() const
		{
			tm tm;
			localtime_s(&tm, &dateTime);
			return tm.tm_year + 1900;
		}
		int Month() const
		{
			tm tm;
			localtime_s(&tm, &dateTime);
			return tm.tm_mon + 1;
		}
		int Day() const
		{
			tm tm;
			localtime_s(&tm, &dateTime);
			return tm.tm_mday;
		}
		int Hour() const
		{
			tm tm;
			localtime_s(&tm, &dateTime);
			return tm.tm_hour;
		}
		int Minute() const
		{
			tm tm;
			localtime_s(&tm, &dateTime);
			return tm.tm_min;
		}*/

		friend class Date;
	private:
		void createFrom(int year, int month, int day)
		{
			tm tm;
			tm.tm_year = year - 1900;
			tm.tm_mon = month - 1;
			tm.tm_mday = day;
			tm.tm_sec = tm.tm_hour = tm.tm_min = 0;
			dateTime = mktime(&tm);
		}
	};

	Date::Date(const DateTime& dateTime)
	{
		createFrom(dateTime.dateTime);
	}
	int Date::WeekDay() const
	{
		DateTime this_ = *this;
		tm tm;
		localtime_s(&tm, &this_.dateTime);
		return tm.tm_wday;
	}


	/*class Date
	{
		uint16 data;
	public:

	};*/
}

#endif