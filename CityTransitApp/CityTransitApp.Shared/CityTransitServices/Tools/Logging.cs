using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;
using Windows.Web.Http;
using CityTransitServices.Tools;
using CityTransitApp.NetPortableServicesImplementations;

namespace CityTransitApp.CityTransitServices.Tools
{
    public class Logging
    {
        public static async Task Save()
        {
            await PerfLogging.Save();
        }
        public static async Task Upload()
        {
            await PerfLogging.Upload();
        }
    }

    public class PerfLogging
    {
        private static List<string> data = new List<string>();
        private static bool saveInProgress = false;

        public static void AddRow(params object[] fields)
        {
            Add(String.Join("\t", fields));
        }
        public static async void Add(string line)
        {
            try
            {
                while (saveInProgress)
                    await Task.Delay(100);
                data.Add(line);
            }
            catch (Exception) { }
        }

        public static async Task Save()
        {
            try
            {
                if (data.Any() && !saveInProgress)
                {
                    saveInProgress = true;
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("perf.log", CreationCollisionOption.OpenIfExists);
                    await FileIO.AppendLinesAsync(file, data);
                    data.Clear();
                    saveInProgress = false;
                }
            }
            catch (Exception) { }
        }

        public static async Task Upload()
        {
            try
            {
                if (UniversalHttpClient.IsLanAvailable())
                {
                    while (saveInProgress)
                        await Task.Delay(100);
                    saveInProgress = true;

                    if (await ApplicationData.Current.LocalFolder.ContainsFileAsync("perf.log"))
                    {
                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync("perf.log");
                        string data = await FileIO.ReadTextAsync(file);

                        HttpStringContent stringContent = new HttpStringContent(data, Windows.Storage.Streams.UnicodeEncoding.Utf8, "text/plain");
                        using (HttpClient client = new HttpClient())
                        {
                            try
                            {
                                var response = await client.PostAsync(new Uri("http://globalcitytransit.azurewebsites.net/Perflog"), stringContent);
                                if (response.StatusCode == HttpStatusCode.Ok)
                                {
                                    await file.DeleteAsync();
                                }
                            }
                            catch (Exception e)
                            {
                                saveInProgress = false;
                            }
                        }
                    }

                    saveInProgress = false;
                }
            }
            catch (Exception) { }
        }
    }
}
