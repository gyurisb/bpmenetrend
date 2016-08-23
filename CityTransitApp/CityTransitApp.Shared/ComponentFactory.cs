using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Text;
using TransitBase;
using UserBase.Interface;
using PlannerComponent.Interface;
using CityTransitServices.Tools;
using UserBase;
using Windows.Storage;
using System.IO;
using PlannerComponent;
using Windows.UI.Core;
using Windows.UI.Popups;
using TransitBase.Entities;
using Windows.UI.Xaml.Media;
using CityTransitApp.CityTransitElements.BaseElements;

namespace CityTransitApp
{
    class ComponentFactory : ICommonCompomentsFactory
    {
        public TransitBaseComponent CreateTransitBase()
        {
            throw new NotImplementedException();
        }

        public IUserBase CreateUserBase()
        {
            return new UserBaseSQLiteComponent(Path.Combine(ApplicationData.Current.LocalFolder.Path, "local.db"));
        }

        public IPlannerComponent CreatePlannerComponent()
        {
            try
            {
                var tb = TransitBaseComponent.Current;
                var nativeComponent = new PlannerRuntimeComponent(
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
                return new PlannerComponentImplementation.PlannerComponentUniversalCpp(nativeComponent);
            }
            catch (TypeLoadException)
            {
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        new MessageDialog("TypeoLoadException").ShowAsync();
                    }
                    catch (Exception) { }
                });
                return null;
            }
        }

        public void StopBackgroundAgent()
        {
            AppTileCreater.RemoveBackgroundAgent();
        }

        public void StartBackgroundAgent()
        {
            AppTileCreater.CheckBackgroundAgent();
        }

        public void InitializeTools()
        {
            //RouteGroupColors.Overlay = (App.Current.Resources["AppBackgroundBrush"] as SolidColorBrush).Color;
            RouteGroup.TFactory = routeGroup => new RouteGroupColors(routeGroup);
            StopPicker.LoadItemSource();
#if WINDOWS_PHONE_APP
            Windows.Services.Maps.MapService.ServiceToken = Config.Current.MapAuthenticationToken;
#endif
        }
    }
}
