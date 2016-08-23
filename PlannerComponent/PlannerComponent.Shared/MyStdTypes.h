#include "pch.h"
#include <list>
#include <algorithm>
#include <ctime>
#include <limits>
#include <string>
#include <vector>
#include <set>
#include "TimeSpan.h"
#include "linq.h"
#include "DateTime.h"
#include "Array2d.h"
#include "Stopper.h"
#include "pch.h"

#define VAR(V,init) __typeof(init) V=(init)
#define FOREACH(I,C) for(VAR(I,(C).begin());I!=(C).end();I++)

#ifndef MYSTDTYPES_H
#define MYSTDTYPES_H

namespace std
{
	class BreakPoint
	{
	public:
		static void Add() {}
	};

	
	/*Stopper StopMain;
	Stopper SStop(&StopMain);
	Stopper SNextTrip(&SStop);
	Stopper STripSt(&StopMain);
	Stopper SServiceAc(&StopMain);
	Stopper STrip(&StopMain);*/
	/*Stopper StopperTotal;
	Stopper StopperTransfer(&StopperTotal);
	Stopper StopperGroupTransfer(&StopperTotal);*/
	Platform::String^ TimeMeasResult()
	{
		return "";
		/*return "Total time: " + StopperTransfer.EllapsedMilliseconds() + "ms of " + StopperTotal.EllapsedMilliseconds() + "\n" + 
			StopperTransfer.RunPercentage() + "%" + "\n" +
			StopperGroupTransfer.EllapsedMilliseconds() + "ms = " + StopperGroupTransfer.RunPercentage() +"%";*/
		/*return "Total time: " + StopMain.EllapsedMilliseconds() + "ms\n" +
			"Stop loop time: " + SStop.RunPercentage() + "%: " + SStop.EllapsedMilliseconds() + "ms\n" +
			" Next trip time: " + SNextTrip.RunPercentage() + "%: " + SNextTrip.EllapsedMilliseconds() + "ms\n" +
			" Next type time: " + STripSt.RunPercentage() + "%: " + STripSt.EllapsedMilliseconds() + "ms\n" +
			" Next type time: " + SServiceAc.RunPercentage() + "%: " + SServiceAc.EllapsedMilliseconds() + "ms\n" +
			"Trip loop time: " + STrip.RunPercentage() + "%\n";*/
	}

	void ResetStoppers()
	{
		/*StopperTotal.Reset();
		StopperTransfer.Reset();*/
	}

}

#endif