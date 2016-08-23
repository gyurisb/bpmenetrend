using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitServices.Tools
{
    public class DailyTasks
    {
        private static List<Action> tasks = new List<Action>();
        private static bool done = false;

        public static void Subscribe(Action task)
        {
            if (done) throw new InvalidOperationException("Cannot subscribe a task after tasks are done.");

            tasks.Add(task);
        }

        public static void DoAll()
        {
            if (done) throw new InvalidOperationException("Tasks are already done.");
            done = true;

            int ellapsedDays = (int)(DateTime.Today - AppFields.LastDailyCheck).TotalDays;
            if (ellapsedDays == 0) return;

            foreach (var task in tasks)
            {
                for (int i = 0; i < ellapsedDays; i++)
                    task();
            }
            AppFields.LastDailyCheck = DateTime.Today;
        }
    }
}
