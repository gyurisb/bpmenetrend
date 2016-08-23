#include <limits>
#include <string>
#include <sstream>

#ifndef TIMESPAN_H
#define TIMESPAN_H
#undef max

namespace std
{
	class TimeSpan
	{
		short time;
		TimeSpan(short time) : time(time) {}
	public:
		TimeSpan(int hour, int minute) : time(hour*60 + minute) {}
		TimeSpan(int minute=0) : time(minute) {}
		TimeSpan operator+(const TimeSpan& other) const { return TimeSpan(time + other.time); }
		TimeSpan operator-(const TimeSpan& other) const { return TimeSpan(time - other.time); }
		void operator+=(const TimeSpan& other) { time += other.time; }
		inline bool operator==(const TimeSpan& other) const { return time == other.time; }
		inline bool operator!=(const TimeSpan& other) const { return time != other.time; }
		inline bool operator<(const TimeSpan& other) const { return time < other.time; }
		inline bool operator>(const TimeSpan& other) const { return time > other.time; }
		inline bool operator<=(const TimeSpan& other) const { return time <= other.time; }
		inline bool operator>=(const TimeSpan& other) const { return time >= other.time; }

		inline int TotalSeconds() const { return time*60; }
		inline int TotalMinutes() const { return time; }
		inline int TotalHours() const { return time / 60; }
		inline int Hour() const { return time/60; }
		inline int Minute() const { return time%60; }

		static TimeSpan FromMinutes(int minutes) { return TimeSpan(minutes); }
		static TimeSpan FromHours(int hours) { return TimeSpan(hours*60); }
		static TimeSpan FromDays(int days) { return TimeSpan(days*24*60); }
		static TimeSpan MaxValue() { return TimeSpan(numeric_limits<short>::max()); }
		static inline TimeSpan Zero() { return TimeSpan(0); }

		//operator int() const { return TotalSeconds(); }
	};
}


#endif