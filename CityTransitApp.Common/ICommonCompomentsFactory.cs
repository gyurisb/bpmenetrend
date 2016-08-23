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
    public interface ICommonCompomentsFactory
    {
        TransitBaseComponent CreateTransitBase();
        IUserBase CreateUserBase();
        IPlannerComponent CreatePlannerComponent();

        void StopBackgroundAgent();
        void StartBackgroundAgent();

        void InitializeTools();
    }
}
