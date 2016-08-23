using CityTransitApp;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Xaml;

namespace CityTransitServices
{
    public class AppInfo
    {
        internal bool DatabaseExists
        {
            get
            {
                return App.TB.DatabaseExists;
            }
        }

        public double GetScreenHeight()
        {
            var scaleFactor = DisplayInformation.GetForCurrentView().RawDpiY;
            return Window.Current.Bounds.Height;
        }
        public double GetScreenWidth()
        {
            var scaleFactor = DisplayInformation.GetForCurrentView().RawDpiX;
            return Window.Current.Bounds.Width;// *scaleFactor;
        }

        internal bool IsEnabled()
        {
            if (IsAcquired)
                return true;
            if (DateTime.Now - AppFields.ApplicationStarted <= TimeSpan.FromDays(App.Config.TrialDurationInDays))
                return true;
            AppFields.ApplicationStarted = DateTime.MinValue;
            return false;
        }

        public bool IsAcquired
        {
            get
            {
                return GetAcquisitionType() >= 0;
            }
        }
        public int GetAcquisitionType()
        {
            int acquisitionType = AppFields.AcquisitionType;
            if (acquisitionType == -1)
            {
                if (App.Config.IsFree)
                {
                    AppFields.AcquisitionType = acquisitionType = 0;
                }
                else if (!Windows.ApplicationModel.Store.CurrentAppSimulator.LicenseInformation.IsTrial)
                {
                    AppFields.AcquisitionType = acquisitionType = 1;
                }
            }
            return acquisitionType;
        }

        public async Task<bool> IsOldVersion()
        {
            string oldfile = "BkvMenetrend.Storage.Stop, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null.dat";
            return await ApplicationData.Current.LocalFolder.ContainsFileAsync(oldfile);
        }
    }
}
