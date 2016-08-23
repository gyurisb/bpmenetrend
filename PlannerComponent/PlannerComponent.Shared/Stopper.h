#include <ctime>
#include <time.h>

#ifndef STOPPER_H
#define STOPPER_H

namespace std
{
	class Stopper
	{
		clock_t timeEllapsed;
		clock_t startTime;
		Stopper* outer;
	public:
		Stopper(Stopper* outer=NULL) : outer(outer), timeEllapsed(0) {}

		static Stopper StartAs(Stopper* outer = NULL)
		{
			Stopper s(outer);
			s.Start();
			return s;
		}
		void Start()
		{
			startTime = clock();
		}
		void Stop()
		{
			timeEllapsed += clock() - startTime;
		}
		int RunPercentage()
		{
			if (outer == NULL || outer->timeEllapsed == 0) return 100;
			return timeEllapsed*100 / outer->timeEllapsed;
		}
		int EllapsedMilliseconds()
		{
			return timeEllapsed/(CLOCKS_PER_SEC/1000);
		}
		void Reset()
		{
			timeEllapsed = 0;
		}
	};
}

#endif