using CityTransitApp.Common;
using CityTransitApp.PlannerComponentImplementation;
using CityTransitApp.WPSilverlight.Tools;
using PlannerComponent;
using PlannerComponent.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TransitBase;
using TransitBase.Entities;
using UserBase.Interface;
using Windows.UI.Core;

namespace CityTransitApp.WPSilverlight
{
    class ComponentFactory : ICommonCompomentsFactory
    {
        public TransitBaseComponent CreateTransitBase()
        {
            throw new NotImplementedException();
        }

        public IUserBase CreateUserBase()
        {
            var userBase = new UserBase.UserBaseLinqToSQLComponent("Data Source=isostore:ddb.sdf", Config.Current.UBVersion);
            userBase.TileRegister.Update();
            return userBase;
        }

        public IPlannerComponent CreatePlannerComponent()
        {
            try
            {
                var tb = TransitBaseComponent.Current;
                var nativeComponent = new PlannerComponent_WP80.PlannerRuntimeComponent(
                    tb.Trips.GetFileName(),
                    tb.TripTypes.GetFileName(),
                    tb.Routes.GetFileName(),
                    tb.RouteGroups.GetFileName(),
                    tb.Services.GetFileName(),
                    tb.CalendarExceptions.GetFileName(),
                    tb.Stops.GetFileName(),
                    tb.StopGroups.GetFileName(),
                    tb.StopEntries.GetFileName(),
                    tb.TTEntries.GetFileName(),
                    tb.TimeEntries.GetFileName(),
                    tb.TripTimeTypes.GetFileName(),
                    tb.Transfers.GetFileName()
                );
                return new PlannerComponentWPSilverlightCpp(nativeComponent);
            }
            catch (TypeLoadException)
            {
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        MessageBox.Show("The application's route planner component cannot be loaded currently. The planning functions are unavailable. Thank you for your understanding.");
                    }
                    catch (Exception) { }
                });
                return null;
            }
        }

        public void StopBackgroundAgent()
        {
            CityTransitApp.WPSilverlight.Tools.TilesApp.RemoveBackgroundAgent();
        }

        public void StartBackgroundAgent()
        {
            CityTransitApp.WPSilverlight.Tools.TilesApp.CheckBackgroundAgent();
        }

        public void InitializeTools()
        {
            RouteGroup.TFactory = routeGroup => new RouteGroupColors(routeGroup);
            CityTransitApp.WPSilverlight.BaseElements.StopPicker.LoadItemSource();

            Config.Current.DefaultDatabase = new Uri("Resources/database-266.zip", UriKind.Relative);
        }
    }
}
