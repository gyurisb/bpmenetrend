using CityTransitApp.CityTransitElements.BaseElements;
using CityTransitApp.CityTransitElements.PageElements.SearchPanels;
using CityTransitApp.CityTransitElements.Pages;
using CityTransitApp.Common.Processes;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels.Settings;
using CityTransitServices.Tools;
using PlannerComponent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.BusinessLogic;
using TransitBase.Entities;
using UserBase;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class PlanningPart : UserControl
    {
        public event EventHandler<WayModel> WaySelected;

        private bool planningInProgress = false;
        private bool unset = true;

        public PlanningPart()
        {
            InitializeComponent();
            ResultList.ItemsSource = new ObservableCollection<WayModel>();

            //if (App.DatabaseExists())
            //    setReference();
        }

        public void SetDateTimePickerDialog(IDateTimePickerDialog dialog)
        {
            this.DateTimePicker.CustomDialog = dialog;
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

        //private void setReference()
        //{
        //    unset = false;
        //    SourceText.Selected = App.Model.FindStops("Üröm vasúti")[0];
        //    TargetText.Selected = App.Model.FindStops("Csepel")[0];
        //    DateTimePicker.Time = new DateTime(2014, 12, 8, 12, 0, 0);
        //    DateTimePicker.TimeType = BkvNative.PlanningTimeType.Departure;
        //}

        void StopText_ValueSet(object sender, SelectionChangedEventArgs e)
        {
            unset = false;
        }

        private async void StartBtnNative_Click(object sender, RoutedEventArgs e)
        {
            if (planningInProgress)
            {
                new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanningInProgress")).ShowAsync();
                return;
            }
            if (!App.Common.OfflinePlanningEnabled)
            {
                bool hasConnection = await CheckConnectionProcess.RunAsync();
                if (!hasConnection)
                {
                    new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanningNoAccess")).ShowAsync();
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
                new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanningSourceEmpty")).ShowAsync();
                return;
            }
            if (TargetText.Selected == null)
            {
                new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanningTargetEmpty")).ShowAsync();
                return;
            }

            var source = SourceText.Selected;
            var target = TargetText.Selected;
            var pickedDateTime = DateTimePicker.Time;


            App.UB.History.AddPlanningHistory(source, true);
            App.UB.History.AddPlanningHistory(target, false);

            ResultList.ItemsSource().Clear();
            ResultBorder.Height = 0;
            ProgressBar.IsIndeterminate = true;

            await App.TB.UsingFiles;

            Stopwatch watch = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                try
                {
                    App.Planner.SetParams(new PlanningArgs
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
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanningError")).ShowAsync());
                }
            });

            IEnumerable<PlanningAspect> aspects = new PlanningAspect[] { PlanningAspect.Time, PlanningAspect.TransferCount, PlanningAspect.WalkDistance };
            //aspects = aspects.Take(App.Config.PlanningAspectsCount);
            //aspects = aspects.Take(1);

            await Task.WhenAll(aspects.Select(async aspect => 
            {
                MiddleWay middleResult = await Task.Run(() =>
                {
                    try
                    {
                        return App.Planner.CalculatePlanning(source.ID, target.ID, pickedDateTime.ToString(CultureInfo.InvariantCulture), aspect);
                    }
                    catch (Exception e)
                    {
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanningError")).ShowAsync());
                        return null;
                    }
                });
                if (middleResult != null)
                {
                    //new MessageDialog(middleResult.Message).ShowAsync();
                    Way result = createWay(middleResult, PlanSettingsModel.WalkingSpeedInMps);
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
            if (ResultList.ItemsSource().Count == 0)
            {
                new MessageDialog(App.Common.Services.Resources.LocalizedStringOf("PlanningNoResult")).ShowAsync();
            }
            else
            {
                TimeText.Text = string.Format("{0} ({1:0.##} sec)", App.Common.Services.Resources.LocalizedStringOf("PlanningTimeLabel"), watch.ElapsedMilliseconds / 1000.0);
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
                var selectedWay = ResultList.SelectedItem as WayModel;
                //MainPage.Current.NavigationService.Navigate(new Uri("/PlanDetailsPage.xaml", UriKind.Relative));
                if (WaySelected != null)
                    WaySelected(this, selectedWay);
                ResultList.SelectedItem = null;
            }
        }

        private void SwapStops_Click(object sender, RoutedEventArgs e)
        {
            StopGroup start = SourceText.Selected;
            SourceText.Selected = TargetText.Selected;
            TargetText.Selected = start;
        }

        private static Way createWay(MiddleWay middleWay, double walkSpeedMps)
        {
            Way way = new Way();

            way.TotalTime = TimeSpan.FromMinutes(middleWay.TotalTime);
            way.TotalTransferCount = middleWay.TotalTransfers;
            way.TotalWalkDistance = middleWay.TotalWalk;
            way.LastWalkDistance = middleWay.LastWalkDistance;
            way.walkSpeedMps = walkSpeedMps;

            if (middleWay.Entries != null)
            {
                way.AddRange(middleWay.Entries.Select(
                    mw => new Way.Entry(mw.Stops.Select(sID => App.TB.Stops[sID]))
                    {
                        Route = App.TB.Routes[mw.RouteID],
                        TripType = App.TB.TripTypes[mw.TripTypeID],
                        StartStop = App.TB.Stops[mw.StartStopID],
                        EndStop = App.TB.Stops[mw.EndStopID],
                        StartTime = TimeSpan.FromMinutes(mw.StartTimeMinutes),
                        EndTime = TimeSpan.FromMinutes(mw.EndTimeMinutes),
                        WaitMinutes = mw.WaitMinutes,
                        WalkBeforeMeters = mw.WalkBeforeMeters,
                        StopCount = mw.Stops.Length - 1
                    }
                ));
            }

            return way;
        }
    }
}
