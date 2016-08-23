using CityTransitApp;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.Entities;
using UserBase;
using CityTransitApp.Common;
using UserBase.Interface;
using CityTransitApp.Common.ViewModels.Settings;
#if WINDOWS_PHONE_APP
using Windows.Services.Maps;
#endif

namespace CityTransitApp.Common.Processes
{
    public class InitializerProcess : Process<InitializerProcess, int, bool>
    {
        private static bool ran = false;
        public static bool FirstRun = false;

        protected override bool Start(params object[] parameters)
        {
            ICommonCompomentsFactory factory = (ICommonCompomentsFactory)parameters[0];
            //if (ran) throw new InvalidOperationException("Initializing only allowed at program start!");
            if (ran) return false;
            ran = true;

            Config.Initialize();
            CommonComponent.Current.Config = Config.Current;
            StopTransfers.WalkingSpeed = () => PlanSettingsModel.WalkingSpeedInMps;
            factory.InitializeTools();

            InitializeFields();
            //StopPicker.LoadItemSource();

            //Alkalmazás első indításakor hívódik meg
            if (AppFields.VersionId == null)
            {
                FirstRun = true;
            }
            //verzió kezelés, meghívódik verzió váltáskor
            if (AppFields.VersionId != CommonComponent.Current.Config.Version)
            {
                //Reset();
                AppFields.VersionId = CommonComponent.Current.Config.Version;
                AppFields.ForceUpdate = true;
                AppFields.PlanningTrialExpired = false;
                factory.StopBackgroundAgent();
            }
            try
            {
                //adatbázisok betöltése
                CommonComponent.Current.UB = factory.CreateUserBase();
                //előzmények öregítése, itt csinálom gyorsan, hogy ne legyenek konkurens tranzakciók, amik a model teljes betöltésének a hatására fog indulni
                DailyTasks.Subscribe(CommonComponent.Current.UB.History.OldenHistory);
                DailyTasks.DoAll();
                //majd a statikus adatbázist is betöltöm
                CommonComponent.Current.LoadTransitBase();
            }
            catch (Exception ex)
            {
                Reset();
                throw;
            }
            //csempe frissítő BackgroundAgent indítása
            factory.StartBackgroundAgent();

            return true;
        }

        private static void InitializeFields()
        {
            AppFields.Initialize();
            SettingsModel.InitializeSettings();
            PlanSettingsModel.InitializeSettings();
        }

        private static string AppName()
        {
            return XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Title").Value;
        }

        public static void Reset()
        {
            try
            {
                if (CommonComponent.Current.TB != null)
                    CommonComponent.Current.TB.Dispose();
                if (CommonComponent.Current.UB != null)
                    CommonComponent.Current.UB.Dispose();
                CommonComponent.Current.Services.FileSystem.GetAppStorageRoot().GetFile("strings.db").Delete();
                CommonComponent.Current.Services.FileSystem.GetAppStorageRoot().GetFile(CommonComponent.Current.UB.FileName).Delete();
            }
            catch (Exception e) { }
        }

        public static void SendStatisctics()
        {
            AcquisitionLogProcess.RunAsync();
        }
    }
}
