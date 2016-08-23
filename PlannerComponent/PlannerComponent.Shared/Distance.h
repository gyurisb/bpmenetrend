#ifndef Distance_H
#define Distance_H

namespace Planning
{
	struct DistVertexPair;

	struct Distance
	{
		int Time;
		double WalkDistance;
		int TransferCount;

		Distance() : Time(numeric_limits<unsigned short>::max()), WalkDistance(numeric_limits<unsigned short>::max()), TransferCount(numeric_limits<unsigned short>::max()) {}
		Distance(int time, double walk, int transfer) : Time(time), WalkDistance(walk), TransferCount(transfer) {}
		Distance(const DistVertexPair& pair);

		Distance operator+(const Distance& other)
		{
			return Distance(Time + other.Time, WalkDistance + other.WalkDistance, TransferCount + other.TransferCount);
		}

		bool operator==(const Distance& other)
		{
			return Time == other.Time && WalkDistance == other.WalkDistance && TransferCount == other.TransferCount;
		}
		static Distance Zero() { return Distance(0, 0.0, 0); }
		
		struct CompareTime {
			int operator() (const Distance& a, const Distance& b) const
			{
				if (a.Time != b.Time) return a.Time - b.Time;
				else if (a.TransferCount != b.TransferCount) return a.TransferCount - b.TransferCount;
				else return sgn(a.WalkDistance - b.WalkDistance);
			}
		};
		struct CompareTransfer {
			int operator() (const Distance& a, const Distance& b) const
			{
				//eredeti: 1, 0.000...1, 8
				int d = (a.Time + (a.TransferCount << 4)) - (b.Time + (b.TransferCount << 4));
				if (d != 0) return d;
				else return sgn(a.WalkDistance - b.WalkDistance);
			}
		};
		struct CompareWalk {
			int operator() (const Distance& a, const Distance& b) const
			{
				//eredeti: 1, 0.1, 0.000...1
				int d = (a.Time + ((int)a.WalkDistance >> 2)) - (b.Time + ((int)b.WalkDistance >> 2));
				if (d != 0) return d;
				else return a.TransferCount - b.TransferCount;
			}
		};
	};
}

#endif