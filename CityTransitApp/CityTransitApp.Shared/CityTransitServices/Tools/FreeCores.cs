using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CityTransitServices.Tools
{
    public class FreeCores
    {
        private static void operation() { }

        public static double Measure()
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 300; i++)
                operation();
            sw.Stop();
            long serialTime = sw.ElapsedTicks;
            sw = Stopwatch.StartNew();
            //Parallel.For(0, 300, i => { operation(); });
            sw.Stop();
            long parallelTime = sw.ElapsedTicks;

            if (serialTime != (int)serialTime || parallelTime != (int)parallelTime)
                throw new OverflowException();
            return (double)serialTime / (double)parallelTime;
        }
    }
}
