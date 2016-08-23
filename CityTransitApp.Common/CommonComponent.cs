using CityTransitServices;
using CityTransitApp.Common.Processes;
using CityTransitServices.Tools;
using PlannerComponent.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase;
using UserBase.Interface;

namespace CityTransitApp.Common
{
    public class CommonComponent
    {
        internal static CommonComponent Current { get; private set; }

        public ICommonServices Services { get; private set; }
        public Config Config { get; internal set; }
        private ICommonCompomentsFactory componentFactory;

        public TransitBaseComponent TB { get; private set; }
        public IUserBase UB { get; internal set; }
        public IPlannerComponent Planner { get; internal set; }

        public event EventHandler<object> ComposedComponentChanged;

        public CommonComponent(ICommonServices services, ICommonCompomentsFactory componentFactory)
        {
            Current = this;
            this.Services = services;
            this.componentFactory = componentFactory;

            InitializerProcess.Run(componentFactory);
        }

        public CommonComponent(ICommonServices services, TransitBaseComponent tb, IUserBase ub)
        {
            Current = this;
            this.Services = services;
            this.Config = Config.Current;
            this.TB = tb;
            this.UB = ub;
        }


        internal bool DatabaseExists
        {
            get
            {
                return TB.DatabaseExists;
            }
        }

        public bool OfflinePlanningEnabled
        {
            get
            {
                if (Config.IAPOfflinePlanning)
                    return AppFields.OfflinePlanningPurchused || !AppFields.PlanningTrialExpired;
                else return true;
            }
        }


        internal void LoadTransitBase()
        {
            this.TB = new TransitBaseComponent(
                root: CommonComponent.Current.Services.FileSystem.GetAppStorageRoot(),
                directionsService: CommonComponent.Current.Services.Directions,
                preLoad: true,
                bigTableLimit: CommonComponent.Current.Config.BigTableLimit,
                checkSameRoutes: CommonComponent.Current.Config.CheckSameRoutes,
                latitudeDegreeDistance: CommonComponent.Current.Config.LatitudeDegreeDistance,
                longitudeDegreeDistance: CommonComponent.Current.Config.LongitudeDegreeDistance
                );
            LoaderProcess.RunWithParametersAsync(componentFactory);
            if (ComposedComponentChanged != null)
                ComposedComponentChanged(this, TB);
        }

        internal void DeleteUserBase()
        {
            CommonComponent.Current.UB.Dispose();
            CommonComponent.Current.UB = null;
            Services.FileSystem.GetAppStorageRoot().GetFile(UB.FileName).Delete();
        }
    }
}
