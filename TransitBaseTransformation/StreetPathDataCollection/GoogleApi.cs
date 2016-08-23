using Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBaseTransformation.Tools;

namespace StreetPathData
{
    class GoogleApi : RouteingApi
    {
        public override async Task<WalkPath> FindRoute(double lat1, double lon1, double lat2, double lon2)
        {
            string methodAdress = String.Format(
                CultureInfo.InvariantCulture,
                "https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&mode=walking&key={4}",
                lat1, lon1,
                lat2, lon2,
                Config.ApiKey);
            Stream responseStream = await Web.OpenReadAsync(methodAdress);
            StreamReader reader = new StreamReader(responseStream);
            string response = reader.ReadToEnd();
            var res = parseFromJson(response);
            if (res != null)
            {
                res.From = new Point { Latitude = lat1, Longitude = lon1 };
                res.To = new Point { Latitude = lat2, Longitude = lon2 };
                return res;
            }
            return null;
        }

        private WalkPath parseFromJson(string data)
        {
            var responseObject = JsonParser.Deserialize(data);
            double distance = double.MaxValue;
            double time = -1;
            int minIndex = -1;
            for (int i = 0; i < responseObject.Routes.Count; i++)
            {
                if (responseObject.Routes[i]["legs"][0]["distance"]["value"] < distance)
                {
                    distance = responseObject.Routes[i]["legs"][0]["distance"]["value"];
                    time = responseObject.Routes[i]["legs"][0]["duration"]["value"];
                    minIndex = i;
                }
            }
            if (minIndex == -1)
                return null;
            var points = new List<Tuple<double, double>>();
            var route = responseObject.Routes[minIndex];
            var leg = route["legs"][0];
            for (int i = 1; i < leg["steps"].Count; i++)
                points.Add(Tuple.Create((double)leg["steps"][i]["start_location"]["lat"], (double)leg["steps"][i]["start_location"]["lng"]));

            return new WalkPath 
            { 
                Distance = distance, 
                Time = time, 
                InnerPoints = points.Select(p => new Point { Latitude = p.Item1, Longitude = p.Item2}).ToArray() 
            };
        }
    }
}
