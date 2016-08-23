using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CityTransitServices.Tools
{
    public class PeriodicTask
    {
        private bool cancel = true;

        private Action task;
        private int intervalTime;
        private CancellationTokenSource token;
        private DateTime nextTime;

        public PeriodicTask(int intervalTime, Action task)
        {
            this.task = task;
            this.intervalTime = intervalTime;
            this.token = new CancellationTokenSource();
        }
        public PeriodicTask(Action task) : this(60000, task) { }

        public void Cancel()
        {
            cancel = true;
            if (!Debugger.IsAttached)
                token.Cancel();
        }

        public async void Run(int delay = 0, bool preExecute = false)
        {
            if (cancel == false)
                throw new InvalidOperationException("Task cannot be ran while is running");

            cancel = false;
            try
            {
                if (preExecute) task();
                await taskDelay(delay);
                while (!cancel)
                {
                    task();
                    await taskDelay(intervalTime);
                }
            }
            catch (TaskCanceledException e) { }
        }

        private async Task taskDelay(int time)
        {
            nextTime = DateTime.Now + TimeSpan.FromMilliseconds(time);
            await Task.Delay(time, token.Token);
        }


        public void RunEveryMinute(bool preExecute = true)
        {
            this.intervalTime = 60000;
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan nextMin = new TimeSpan(now.Hours, now.Minutes + 1, 0);
            Run((int)(nextMin - now).TotalMilliseconds, preExecute);
        }

        public void Resume()
        {
            token = new CancellationTokenSource();
            TimeSpan delay = nextTime > DateTime.Now ? nextTime - DateTime.Now : TimeSpan.Zero;
            Run((int)delay.TotalMilliseconds, false);
        }
    }
}
