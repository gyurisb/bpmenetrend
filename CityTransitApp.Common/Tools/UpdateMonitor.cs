using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitServices.Tools
{
    public class UpdateMonitor
    {
        public enum Result { Found, NotFound, NoAccess, NotRequired };

        public static void UpdateDone(int newUpdatesVersion)
        {
            AppFields.UpdateAvailable = false;
            AppFields.DbUpdateVersion = newUpdatesVersion;
        }

        private static bool outOfDate()
        {
            TimeSpan updateInterval = TimeSpan.FromDays(AppFields.GetAutomaticUpdateIntervalInDays());
            return AppFields.LastUpdateCheck + updateInterval <= DateTime.Today;
        }

        public static async Task<Result> CheckUpdate(bool force = false)
        {
            if (AppFields.UpdateAvailable)
            {
                return Result.Found;
            }
            else if (force || outOfDate())
            {
                var checkResult = await doCheckUpdate();
                if (checkResult != Result.NoAccess)
                {
                    AppFields.LastUpdateCheck = DateTime.Today;
                    if (checkResult == Result.Found)
                    {
                        AppFields.UpdateAvailable = true;
                    }
                    //CommonComponent.Current.Services.Settings.Save();
                }
                return checkResult;
            }
            return Result.NotRequired;
        }

        private static async Task<Result> doCheckUpdate()
        {
            Stream resultStream = await CommonComponent.Current.Services.Http.GetAsync(Config.Current.GetUpdatesUrl());
            if (resultStream == null)
            {
                return Result.NoAccess;
            }

            int newVersion = int.Parse(new StreamReader(resultStream).ReadLine());

            if (newVersion != AppFields.DbUpdateVersion)
            {
                return Result.Found;
            }
            else
            {
                return Result.NotFound;
            }
        }

        //private static string getCurrentUpdate(StreamReader streamReader)
        //{
        //    string ret = null, line;
        //    while ((line = streamReader.ReadLine()) != null)
        //    {
        //        string[] current = line.Split(' ');
        //        if (current.Length == 1)
        //            ret = current[0];
        //        else if (current[0] == CommonComponent.Current.Config.Version)
        //            ret = current[1];
        //    }
        //    return ret;
        //}

        //private static string getCurrentUpdate(Stream stream)
        //{
        //    using (StreamReader reader = new StreamReader(stream))
        //        return getCurrentUpdate(reader);
        //}
    }
}
