using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBaseTransformation.DataSource;

namespace TransitBaseIntegration.Gtfs
{
    public class GtfsDistributedDatabase : IGtfsDatabase
    {
        private GtfsDatabase[] databases;

        public GtfsDistributedDatabase(IEnumerable<string> sources)
        {
            databases = sources.Select(source => new GtfsDatabase(source)).ToArray();
        }

        public IGtfsTable GetTable(string name)
        {
            return new GtfsDistributedTable(databases.Select(db => db.GetTable(name) as GtfsTable));
        }

        public string GetIdValue(string data)
        {
            if (data.Contains('@'))
                return data.Substring(0, data.Length - 2);
            else return data;
        }
    }
}
