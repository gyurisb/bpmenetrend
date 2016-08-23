using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CityTransitApp
{
    public abstract class PageStateManagerBase
    {
        protected Page Page { get; set; }
        protected abstract PageState State { get; }
        public abstract void OnNavigatedTo(NavigationEventArgs e, bool setStatusBar = true);
        public abstract void OnNavigatedFrom(NavigationMode mode, Type sourceType);

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            OnNavigatedFrom(e.NavigationMode, e.SourcePageType);
        }
        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            OnNavigatedFrom(e.NavigationMode, e.SourcePageType);
        }

        public void ScheduleTaskEveryMinute(Action action)
        {
            if (State.PeriodicTaskEntries == null)
                State.PeriodicTaskEntries = new List<IPeriodicTaskEntry>();
            var newTask = new PeriodicTask(action);
            State.PeriodicTaskEntries.Add(new SimplePeriodicTaskEntry(newTask));
            newTask.RunEveryMinute();
        }

        public void ScheduleTask(int intervalTimeInMs, Action action, int delay = 0, bool preExecute = false)
        {
            if (State.PeriodicTaskEntries == null)
                State.PeriodicTaskEntries = new List<IPeriodicTaskEntry>();
            var newTask = new PeriodicTask(intervalTimeInMs, action);
            State.PeriodicTaskEntries.Add(new SimplePeriodicTaskEntry(newTask));
            newTask.Run(delay, preExecute);
        }

        public void ScheduleTaskEveryMinute<TPage>(Action<TPage> action) where TPage : Page
        {
            if (State.PeriodicTaskEntries == null)
                State.PeriodicTaskEntries = new List<IPeriodicTaskEntry>();
            var taskEntry = new PortablePeriodicTaskEntry<TPage>(Page, action);
            State.PeriodicTaskEntries.Add(taskEntry);
            taskEntry.Task.RunEveryMinute();
        }
        public void ScheduleTask<TPage>(int intervalTimeInMs, Action<TPage> action, int delay = 0, bool preExecute = false) where TPage : Page
        {
            if (State.PeriodicTaskEntries == null)
                State.PeriodicTaskEntries = new List<IPeriodicTaskEntry>();
            var taskEntry = new PortablePeriodicTaskEntry<TPage>(Page, action, intervalTimeInMs);
            State.PeriodicTaskEntries.Add(taskEntry);
            taskEntry.Task.Run(delay, preExecute);
        }
    }

    public class PageState
    {
        public Type PageType;
        public object StateObject;
        public List<IPeriodicTaskEntry> PeriodicTaskEntries;

        public void StopTasks()
        {
            if (PeriodicTaskEntries != null)
                foreach (var entry in PeriodicTaskEntries)
                    entry.Task.Cancel();
        }
        public void ResumeTasks()
        {
            if (PeriodicTaskEntries != null)
                foreach (var entry in PeriodicTaskEntries)
                    entry.Task.Resume();
        }
        public void UpdateTasks(Page page)
        {
            if (PeriodicTaskEntries != null)
                foreach (var entry in PeriodicTaskEntries)
                    entry.OuterPage = page;
        }
    }

    public interface IPeriodicTaskEntry
    {
        Page OuterPage { set; }
        PeriodicTask Task { get; }
    }

    public class SimplePeriodicTaskEntry : IPeriodicTaskEntry
    {
        public Page OuterPage { set { } }
        public PeriodicTask Task { get; private set; }
        public SimplePeriodicTaskEntry(PeriodicTask task)
        {
            this.Task = task;
        }
    }

    public class PortablePeriodicTaskEntry<TPage> : IPeriodicTaskEntry where TPage : Page
    {
        public Page OuterPage { get; set; }
        private Action<TPage> action;
        public PeriodicTask Task { get; private set; }
        public PortablePeriodicTaskEntry(Page page, Action<TPage> action, int intervalTime = 60000)
        {
            this.OuterPage = page;
            this.action = action;
            this.Task = new PeriodicTask(intervalTime, taskAction);
        }
        private void taskAction()
        {
            action((TPage)OuterPage);
        }
    }
}
