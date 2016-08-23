using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NetPortableServices;
using Windows.Web.Http;
using System.Net.NetworkInformation;
using Windows.Networking.Connectivity;

namespace CityTransitApp.NetPortableServicesImplementations
{
    class UniversalHttpClient : IHttpService
    {
        public async Task<Stream> GetAsync(string url)
        {
            return await OpenReadAsync(url);
        }

        public async Task<Stream> GetAsync(string url, Action<NetPortableServices.HttpProgress> progressChangeHandler)
        {
            return await OpenReadAsync(url, delegate(Windows.Web.Http.HttpProgress progress) 
            {
                progressChangeHandler(new NetPortableServices.HttpProgress
                {
                    BytesReceived = (int)progress.BytesReceived,
                    TotalBytesToReceive = (int)(progress.TotalBytesToReceive ?? 0)
                });
            });
        }

        public string UrlEncode(string text)
        {
            return "";
        }

        private static Random Random = new Random();

        private static Uri CreateRandomizedUri(string url)
        {
            string newUrl = url + (url.Contains("?") ? "&" : "?") + "random=" + Random.Next();
            return new Uri(newUrl);
        }

        private static async Task<Stream> OpenReadAsync(string url, Action<Windows.Web.Http.HttpProgress> progressChangeHandler = null)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var uri = CreateRandomizedUri(url);
                    var task = client.GetInputStreamAsync(uri);
                    if (progressChangeHandler != null)
                        task.Progress += (asyncInfo, progressInfo) => progressChangeHandler(progressInfo);
                    return (await task).AsStreamForRead();
                }
            }
            catch (Exception) { return null; }
        }

        public static bool IsLanAvailable()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
            {
                var interfaceType = profile.NetworkAdapter.IanaInterfaceType;
                return interfaceType == 71 || interfaceType == 6;
            }
            return false;
        }
    }
}
