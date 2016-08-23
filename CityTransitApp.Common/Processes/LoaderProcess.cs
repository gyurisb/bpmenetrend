using CityTransitApp;
using CityTransitApp.Common;
using CityTransitServices.Tools;
using PlannerComponent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CityTransitApp.Common.Processes
{
    public class LoaderProcess : Process<LoaderProcess, int, bool>
    {
        protected override bool Start(object[] parameters)
        {
            var factory = (ICommonCompomentsFactory)parameters[0];
            CommonComponent.Current.TB.LoadDataFiles();

            if (CommonComponent.Current.DatabaseExists && !AppFields.ForceUpdate)
            {
                if (CommonComponent.Current.Planner != null)
                {
                    CommonComponent.Current.Planner.Close(safe: true);
                    CommonComponent.Current.Planner = null;
                }
                CommonComponent.Current.Planner = factory.CreatePlannerComponent();
            }
            CommonComponent.Current.TB.UsingFiles.Start();

            CommonComponent.Current.TB.LoadAllEntities();
            return true;
        }
    }
}
