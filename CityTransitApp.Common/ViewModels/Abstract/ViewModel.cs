using System;
using System.Collections.Generic;
using System.Text;
using TransitBase.BusinessLogic.Helpers;
using System.Collections.ObjectModel;
using CityTransitApp.Common;

namespace CityTransitApp
{
    public abstract class ViewModel : Bindable
    {
        protected ICommonServices Services = CommonComponent.Current.Services;

        //public event EventHandler<Action> TaskScheduleRequired;
        public abstract void Initialize(object initialData);
        public IReadOnlyCollection<Action> TasksToSchedule { get { return new ReadOnlyCollection<Action>(tasksToSchedule); } }

        private List<Action> tasksToSchedule = new List<Action>();
        protected void AddTaskToSchedule(Action task) { tasksToSchedule.Add(task); }
        protected void AddTasksToSchedule(IEnumerable<Action> task) { tasksToSchedule.AddRange(task); }
    }

    public abstract class ViewModel<TParam> : ViewModel
    {
        public abstract void Initialize(TParam initialData);

        public override void Initialize(object initialData)
        {
            this.Initialize((TParam)initialData);
        }
    }

    /* Visibility -> bool
     * StopGroupModel.WheelchairVisibility
     * StopGroupModel.TransferVisibility
     * StopGroupModel.DistanceVisibility
     * StopGroupModel.SeparatorVisibility
     * StopGroupModel.BtnVisibility
     * RouteModel.TimeVisibility
     * RouteStopModel.NextVisibilities
     * SearchResultModel.StopVisibility
     * SearchResultModel.MetroVisibility
     * SearchResultModel.TrainVisibility
     * SearchResultModel.TramVisibility
     * SearchResultModel.BusVisibility
     * SearchResultModel.FerryVisibility
     * TripViewModel.DistanceVisibility
     * WayModel.WalkAfterVisibility
     * EntryModel.WaitVisibility
     * EntryModel.WalkVisibility
     * EntryModel.IsMetroVisibility
     */
}
