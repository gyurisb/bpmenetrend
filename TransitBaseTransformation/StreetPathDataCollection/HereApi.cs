using Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using TransitBaseTransformation.Tools;

namespace StreetPathData
{
    class HereApi : RouteingApi
    {
        public override async Task<WalkPath> FindRoute(double lat1, double lon1, double lat2, double lon2)
        {
            string methodAdress = String.Format(
                CultureInfo.InvariantCulture,
                "http://route.cit.api.here.com/routing/7.2/calculateroute.xml?app_id=knt7q2hsa9CyqkGLCDlg&app_code=n5Xrl18Fm7miZC8ZJwesyQ&waypoint0=geo!{0},{1}&waypoint1=geo!{2},{3}&mode=shortest;pedestrian",
                lat1, lon1,
                lat2, lon2
                );
            Stream responseStream = await Web.OpenReadAsync(methodAdress);
            if (responseStream != null)
            {
                var res = parseFromXml(responseStream);
                if (res != null)
                {
                    res.From = new Point { Latitude = lat1, Longitude = lon1 };
                    res.To = new Point { Latitude = lat2, Longitude = lon2 };
                    //res.Query = methodAdress;
                    return res;
                }
            }
            return null;
        }

        private WalkPath parseFromXml(Stream data)
        {
            var route = XDocument.Load(data).Root.Element("Response").Elements("Route").Single();
            var summary = route.Element("Summary");
            double distance = double.Parse(summary.Element("Distance").Value, CultureInfo.InvariantCulture);
            double time = double.Parse(summary.Element("TravelTime").Value, CultureInfo.InvariantCulture);

            var leg = route.Elements("Leg").Single();
            var points = new List<Point>();
            foreach (var element in leg.Elements("Maneuver"))
            {
                var position = element.Element("Position");
                double lat = double.Parse(position.Element("Latitude").Value, CultureInfo.InvariantCulture);
                double lon = double.Parse(position.Element("Longitude").Value, CultureInfo.InvariantCulture);
                points.Add(new Point { Latitude = lat, Longitude = lon });
            }

            return new WalkPath
            {
                Distance = distance,
                Time = time,
                InnerPoints = points.ToArray()
            };
        }
    }
}
