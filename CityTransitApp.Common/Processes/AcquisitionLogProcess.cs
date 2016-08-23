using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CityTransitApp;
using CityTransitServices.Tools;
using System.Runtime.InteropServices.WindowsRuntime;
using CityTransitApp.Common;

namespace CityTransitApp.Common.Processes
{
    public class AcquisitionLogProcess : Process<AcquisitionLogProcess, int, bool>
    {
        protected override async Task<bool> StartAsync()
        {
            if (Services.Auth == null || Services.Cryptography == null)
                return false;

            //int acquisitionLoggedType = App.GetAcquisitionType()
            int acquisitionType = AppFields.AcquisitionType;
            if (AppFields.AcquisitionLoggedType != acquisitionType)
            {
                string anid = Services.Auth.GetUserID();

                string text = CommonComponent.Current.Config.AppNumber + "/" + acquisitionType + "/" + anid;
                byte[] signatureData = Services.Cryptography.Sign(null, Encoding.UTF8.GetBytes(text));
                string signature = Convert.ToBase64String(signatureData);

                string requestUrl = String.Format(
                    CommonComponent.Current.Config.StatisticsUrl + "acquire.jsp?data={0}&signature={1}&response=true",
                    Services.Http.UrlEncode(text),
                    Services.Http.UrlEncode(signature)
                    );
                var stream = await Services.Http.GetAsync(requestUrl);
                if (stream == null)
                    return false;
                string[] result = getResultMessage(new StreamReader(stream).ReadToEnd()).Split(' ');
                if (result[0] == "ok")
                {
                    acquisitionType = int.Parse(result[1]);
                    string resultText = CommonComponent.Current.Config.AppNumber + "/" + acquisitionType + "/" + anid;
                    string resultSign = string.Join(" ", result.Skip(2));

                    bool verified = Services.Cryptography.IsSignatureValid(null, Encoding.UTF8.GetBytes(resultText), Convert.FromBase64String(resultSign));

                    if (verified)
                    {
                        AppFields.AcquisitionLoggedType = AppFields.AcquisitionType = acquisitionType;
                        return true;
                    }
                }
            }
            return false;
        }

        private static string getResultMessage(string result)
        {
            string[] lines = result.Split('\n');
            int bodyStartIndex = lines.ToList().IndexOf("<BODY>");
            return lines.Skip(bodyStartIndex + 1).First(line => line.Cast<char>().Any(ch => char.IsLetter(ch)));
        }
    }
}
