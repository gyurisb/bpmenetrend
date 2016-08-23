using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Foundation;
using Windows.UI.StartScreen;
using CityTransitServices;
using TransitBase;
using Windows.Storage;
using TransitBase.Entities;
using System.Net.NetworkInformation;
using Windows.Networking.Connectivity;

namespace CityTransitBgTaskCS
{
    sealed public class AppBackgroundTask : XamlRenderingBackgroundTask
    {
        protected override async void OnRun(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            //Config.Initialize();

            //if (Web.IsLanAvailable() && !AppFields.UpdateAvailable)
            //{
            //    var checkResult = await UpdateMonitor.CheckUpdate();
            //    if (checkResult == UpdateMonitor.Result.Found)
            //    {
            //        sendUpdateToastNotification();
            //    }
            //}

            //var tiles = await SecondaryTile.FindAllAsync();
            //if (tiles.Any())
            //{
            //    using (var TB = new TransitBaseComponent(
            //        root: App.Services.FileSystem.GetAppStorageRoot(), 
            //        preLoad: false,
            //        bigTableLimit: Config.Current.BigTableLimit,
            //        checkSameRoutes: Config.Current.CheckSameRoutes,
            //        latitudeDegreeDistance: Config.Current.LatitudeDegreeDistance,
            //        longitudeDegreeDistance: Config.Current.LongitudeDegreeDistance
            //        ))
            //    {
            //        foreach (var tile in tiles)
            //        {
            //            var arg = tile.Arguments.Split('-');
            //            Route route = TB.Logic.GetRouteByID(int.Parse(arg[0]));
            //            StopGroup stop = TB.Logic.GetStopGroupByID(int.Parse(arg[1]));
            //            await AppTileUpdater.UpdateTile(route, stop, tile: tile, doFlush: true);
            //            TB.Flush();
            //        }
            //    }
            //}

            deferral.Complete();
        }

        private void sendUpdateToastNotification()
        {
            ToastTemplateType toastType = ToastTemplateType.ToastText02;

            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastType);

            XmlNodeList toastTextElement = toastXml.GetElementsByTagName("text");
            toastTextElement[0].AppendChild(toastXml.CreateTextNode("Update"));
            toastTextElement[1].AppendChild(toastXml.CreateTextNode("A database update is required."));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast); 
        }
    }
}
