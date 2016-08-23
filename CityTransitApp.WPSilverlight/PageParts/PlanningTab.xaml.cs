using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using TransitBase.BusinessLogic;
using System.Collections.ObjectModel;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels.Settings;
using CityTransitApp.WPSilverlight.Resources;
using System.Globalization;
using System.Diagnostics;
using TransitBase.Entities;
using CityTransitApp.Common.Processes;
using PlannerComponent.Interface;

namespace CityTransitApp.WPSilverlight.PageParts
{
    public partial class PlanningTab : UserControl
    {
        public static PlanningTab Current;

        private bool planningInProgress = false;
        private bool unset = true;

        public WayModel SelectedWay { get; private set; }

        public PlanningTab()
        {
            Current = this;
            InitializeComponent();
            ResultList.ItemsSource = new ObservableCollection<WayModel>();

            //if (App.DatabaseExists())
            //    setReference();
        }


        public void SetNearStop(StopGroup stop, TimeSpan walkTime)
        {
            if (unset /*&& MainPage.Current.Pivot.SelectedIndex != MainPage.PLANNING_INDEX*/)
            {
                //set source stop
                SourceText.Selected = stop;
                //set begin time
                DateTimePicker.Time = DateTime.Now + walkTime;
            }
        }

        private void setReference()
        {
            unset = false;
            SourceText.Selected = App.Model.FindStops("Üröm vasúti")[0];
            TargetText.Selected = App.Model.FindStops("Csepel")[0];
            DateTimePicker.Time = new DateTime(2014, 12, 8, 12, 0, 0);
            DateTimePicker.TimeType = PlanningTimeType.Departure;
        }

        void StopText_ValueSet(object sender, SelectionChangedEventArgs e)
        {
            unset = false;
        }

        private async void StartBtnNative_Click(object sender, RoutedEventArgs e)
        {
            if (planningInProgress)
            {
                MessageBox.Show(AppResources.PlanningInProgress);
                return;
            }
            if (!App.OfflinePlanningEnabled)
            {
                bool hasConnection = await CheckConnectionProcess.RunAsync();
                if (!hasConnection)
                {
                    MessageBox.Show(AppResources.PlanningNoAccess);
                    return;
                }
            }
            planningInProgress = true;
            await planning();
            planningInProgress = false;
        }

        private async Task planning()
        {
            if (SourceText.Selected == null)
            {
                MessageBox.Show(AppResources.PlanningSourceEmpty);
                return;
            }
            if (TargetText.Selected == null)
            {
                MessageBox.Show(AppResources.PlanningTargetEmpty);
                return;
            }

            StopGroup source = SourceText.Selected;
            StopGroup target = TargetText.Selected;
            DateTime pickedDateTime = DateTimePicker.Time;


            App.UB.History.AddPlanningHistory(source, true);
            App.UB.History.AddPlanningHistory(target, false);

            ResultList.ItemsSource.Clear();
            ResultBorder.Height = 0;
            ProgressBar.IsIndeterminate = true;

            await App.TB.UsingFiles;

            Stopwatch watch = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                try
                {
                    App.NativeComponent.SetParams(new PlanningArgs
                    {
                        Type = DateTimePicker.TimeType,
                        EnabledTypes = Convert(new bool[] { PlanSettingsModel.TramAllowed, PlanSettingsModel.MetroAllowed, PlanSettingsModel.UrbanTrainAllowed, PlanSettingsModel.BusAllowed, true, true, true, true }),
                        OnlyWheelchairAccessibleTrips = PlanSettingsModel.WheelchairAccessibleTrips,
                        OnlyWheelchairAccessibleStops = PlanSettingsModel.WheelchairAccessibleStops,
                        LatitudeDegreeDistance = App.Config.LatitudeDegreeDistance,
                        LongitudeDegreeDistance = App.Config.LongitudeDegreeDistance,
                        WalkSpeedRate = PlanSettingsModel.WalkingSpeed / 3.6 * 60
                    });
                }
                catch (Exception e)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(AppResources.PlanningError));
                }
            });

            IEnumerable<PlanningAspect> aspects = new PlanningAspect[] { PlanningAspect.Time, PlanningAspect.TransferCount, PlanningAspect.WalkDistance };
            //aspects = aspects.Take(App.Config.PlanningAspectsCount);
            //aspects = aspects.Take(1);

            await Task.WhenAll(aspects.Select(async aspect =>
            {
                var middleResult = await Task.Run(() =>
                {
                    try
                    {
                        return App.NativeComponent.CalculatePlanning(source, target, pickedDateTime, aspect);
                    }
                    catch (Exception e)
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show(AppResources.PlanningError));
                        return null;
                    }
                });
                if (middleResult != null)
                {
                    //MessageBox.Show(middleResult.Message);
                    TransitBase.BusinessLogic.Way result = Convert(middleResult, PlanSettingsModel.WalkingSpeedInMps);
                    var resultList = ResultList.ItemsSource as ObservableCollection<WayModel>;
                    if (resultList.All(x => !x.Way.Equals(result)))
                    {
                        int i = 0;
                        while (i < resultList.Count && resultList[i].Way < result) i++;
                        resultList.Insert(i, new WayModel(result, source, target, pickedDateTime));
                        ResultBorder.Height = double.NaN;
                    }
                }
            }));

            ProgressBar.IsIndeterminate = false;
            if (ResultList.ItemsSource.Count == 0)
            {
                MessageBox.Show(AppResources.PlanningNoResult);
            }
            else
            {
                TimeText.Text = string.Format("{0} ({1:0.##} sec)", AppResources.PlanningTimeLabel, watch.ElapsedMilliseconds / 1000.0);
                await Task.Delay(3000);
                TimeText.Text = "";
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

        void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultList.SelectedItem != null)
            {
                SelectedWay = ResultList.SelectedItem as WayModel;
                MainPage.Current.NavigationService.Navigate(new Uri("/PlanDetailsPage.xaml", UriKind.Relative));
                ResultList.SelectedItem = null;
            }
        }

        private void SwapStops_Click(object sender, RoutedEventArgs e)
        {
            StopGroup start = SourceText.Selected;
            SourceText.Selected = TargetText.Selected;
            TargetText.Selected = start;
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
