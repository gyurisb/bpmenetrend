using PlannerComponent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TransitBase.BusinessLogic;
using System.Threading.Tasks;
using CityTransitServices.Tools;
using CityTransitApp.Common.Processes;
using TransitBase.Entities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CityTransitApp.Common;
using PlannerComponent.Interface;
using CityTransitApp.Common.ViewModels.Settings;

namespace CityTransitApp.Common.ViewModels
{
    public class PlanningViewModel : ViewModel<PlanningParameter>
    {
        private bool planningInProgress = false;
        private bool unset = true;

        public ObservableCollection<WayModel> FoundRoutes { get; set; }
        public double ResultBorderHeight { get { return Get<double>(); } set { Set(value); } }
        public bool InProgress { get { return Get<bool>(); } set { Set(value); } }
        public StopGroup SourceStop { get; private set; }
        public StopGroup DestStop { get; private set; }

        public PlanningViewModel()
        {
            FoundRoutes = new ObservableCollection<WayModel>();
        }

        public override void Initialize(PlanningParameter initialData)
        {
            SourceStop = initialData.SourceStop;
            DestStop = initialData.DestStop;
        }
        public async Task PlanAsync(PlanningParameter planningData)
        {
            await PlanAsync(planningData.SourceStop, planningData.DestStop, planningData.DateTime, planningData.PlanningType);
        }
        public async Task PlanAsync(StopGroup sourceStop, StopGroup targetStop, DateTime pickedDateTime, PlanningTimeType planningType)
        {
            if (planningInProgress)
            {
                Services.MessageBox.Show(Services.Resources.LocalizedStringOf("PlanningInProgress"));
                return;
            }
            if (!CommonComponent.Current.OfflinePlanningEnabled)
            {
                bool hasConnection = await CheckConnectionProcess.RunAsync();
                if (!hasConnection)
                {
                    Services.MessageBox.Show(Services.Resources.LocalizedStringOf("PlanningNoAccess"));
                    return;
                }
            }
            planningInProgress = true;
            await planAsync(sourceStop, targetStop, pickedDateTime, planningType);
            planningInProgress = false;
        }

        private async Task planAsync(StopGroup source, StopGroup target, DateTime pickedDateTime, PlanningTimeType planningType)
        {
            if (source == null)
            {
                Services.MessageBox.Show(Services.Resources.LocalizedStringOf("PlanningSourceEmpty"));
                return;
            }
            if (target == null)
            {
                Services.MessageBox.Show(Services.Resources.LocalizedStringOf("PlanningTargetEmpty"));
                return;
            }

            //var source = SourceText.Selected;
            //var target = TargetText.Selected;
            //var pickedDateTime = DateTimePicker.Time;

            CommonComponent.Current.UB.History.AddPlanningHistory(source, true);
            CommonComponent.Current.UB.History.AddPlanningHistory(target, false);

            FoundRoutes.Clear();
            ResultBorderHeight = 0;
            InProgress = true;

            await CommonComponent.Current.TB.UsingFiles;

            Stopwatch watch = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                try
                {
                    CommonComponent.Current.Planner.SetParams(new PlanningArgs
                    {
                        Type = planningType,
                        EnabledTypes = Convert(new bool[] { PlanSettingsModel.TramAllowed, PlanSettingsModel.MetroAllowed, PlanSettingsModel.UrbanTrainAllowed, PlanSettingsModel.BusAllowed, true, true, true, true }),
                        OnlyWheelchairAccessibleTrips = PlanSettingsModel.WheelchairAccessibleTrips,
                        OnlyWheelchairAccessibleStops = PlanSettingsModel.WheelchairAccessibleStops,
                        LatitudeDegreeDistance = CommonComponent.Current.Config.LatitudeDegreeDistance,
                        LongitudeDegreeDistance = CommonComponent.Current.Config.LongitudeDegreeDistance,
                        WalkSpeedRate = PlanSettingsModel.WalkingSpeed / 3.6 * 60
                    });
                }
                catch (Exception e)
                {
                    //Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Services.MessageBox.Show(Services.Localization.StringOf("PlanningError")));
                }
            });

            IEnumerable<PlanningAspect> aspects = new PlanningAspect[] { PlanningAspect.Time, PlanningAspect.TransferCount, PlanningAspect.WalkDistance };
            //aspects = aspects.Take(CommonComponent.Current.Config.PlanningAspectsCount);
            //aspects = aspects.Take(1);

            await Task.WhenAll(aspects.Select(async aspect =>
            {
                PlannerComponent.Interface.Way middleResult = await Task.Run(() =>
                {
                    try
                    {
                        return CommonComponent.Current.Planner.CalculatePlanning(source, target, pickedDateTime, aspect);
                    }
                    catch (Exception e)
                    {
                        //Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Services.MessageBox.Show(Services.Localization.StringOf("PlanningError")));
                        return null;
                    }
                });
                if (middleResult != null)
                {
                    //Services.MessageBox.Show(middleResult.Message);
                    TransitBase.BusinessLogic.Way result = Convert(middleResult, PlanSettingsModel.WalkingSpeedInMps);
                    if (FoundRoutes.All(x => !x.Way.Equals(result)))
                    {
                        int i = 0;
                        while (i < FoundRoutes.Count && FoundRoutes[i].Way < result) i++;
                        FoundRoutes.Insert(i, new WayModel(result, source, target, pickedDateTime));
                        ResultBorderHeight = double.NaN;
                    }
                }
            }));

            InProgress = false;
            if (FoundRoutes.Count == 0)
            {
                Services.MessageBox.Show(Services.Resources.LocalizedStringOf("PlanningNoResult"));
            }
            else
            {
                //TimeText.Text = string.Format("{0} ({1:0.##} sec)", Services.Localization.StringOf("PlanningTimeLabel"), watch.ElapsedMilliseconds / 1000.0);
                //await Task.Delay(3000);
                //TimeText.Text = "";
            }
        }

        private static byte Convert(bool[] array)
        {
            byte b = 255;
            for (int i = 0; i < array.Length; i++)
                if (!array[i])
                    b &= (byte)~(1 << i);
            return b;
        }


        private static TransitBase.BusinessLogic.Way Convert(PlannerComponent.Interface.Way middleWay, double walkSpeedMps)
        {
            var way = new TransitBase.BusinessLogic.Way();

            way.TotalTime = middleWay.TotalTime;
            way.TotalTransferCount = middleWay.TotalTransfers;
            way.TotalWalkDistance = middleWay.TotalWalk;
            way.LastWalkDistance = middleWay.LastWalkDistance;
            way.walkSpeedMps = walkSpeedMps;

            way.AddRange(middleWay.Select(
                mw => new TransitBase.BusinessLogic.Way.Entry(mw)
                {
                    Route = mw.Route,
                    TripType = mw.TripType,
                    StartStop = mw.StartStop,
                    EndStop = mw.EndStop,
                    StartTime = mw.StartTime,
                    EndTime = mw.EndTime,
                    WaitMinutes = mw.WaitMinutes,
                    WalkBeforeMeters = mw.WalkBeforeMeters,
                    StopCount = mw.StopCount
                }
            ));

            return way;
        }
    }
}
