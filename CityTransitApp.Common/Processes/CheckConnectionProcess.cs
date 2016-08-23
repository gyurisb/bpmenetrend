using CityTransitApp;
using CityTransitApp.Common;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.Common.Processes
{
    public class CheckConnectionProcess : Process<CheckConnectionProcess, int, bool>
    {
        protected override async Task<bool> StartAsync()
        {
            Stream webStream = await Services.Http.GetAsync(CommonComponent.Current.Config.CheckUrl);

            if (webStream == null)
                return false;

            string line = new StreamReader(webStream).ReadLine();
            return line == CommonComponent.Current.Config.CheckLine;
        }
    }
}
