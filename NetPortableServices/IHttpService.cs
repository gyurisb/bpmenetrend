using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPortableServices
{
    public interface IHttpService
    {
        Task<Stream> GetAsync(string url);
        Task<Stream> GetAsync(string url, Action<HttpProgress> progressChangeHandler);
        string UrlEncode(string text);
    }

    public struct HttpProgress
    {
        public int BytesReceived;
        public int TotalBytesToReceive;
    }
}
