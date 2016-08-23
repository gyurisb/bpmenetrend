using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TransitBaseTransformation.Tools
{
    public class Web
    {
        private static Random Random = new Random();

        private static void OpenReadAsyncNoCache(WebClient client, string url, object userToken = null)
        {
            string newUrl = url + (url.Contains("?") ? "&" : "?") + "random=" + Random.Next();
            client.OpenReadAsync(new Uri(newUrl), userToken);
        }

        public static Task<Stream> OpenReadAsync(string url, DownloadProgressChangedEventHandler progressChangeHandler = null)
        {
            Stream result = null;

            Task<Stream> task = new Task<Stream>(() => result);

            WebClient client = new WebClient();
            client.OpenReadCompleted += (sender, args) =>
            {
                try { result = args.Result; }
                catch (Exception e) { }
                task.Start();
            };
            if (progressChangeHandler != null)
                client.DownloadProgressChanged += progressChangeHandler;
            OpenReadAsyncNoCache(client, url);

            return task;
        }
    }
}
