using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBaseTransformation.DataSource;

namespace TransitBaseIntegration.Gtfs
{
    class GtfsDatabase : IGtfsDatabase
    {
        private Dictionary<string, int> names = new Dictionary<string, int>();
        private string[] files;

        public GtfsDatabase(String dir)
        {
            files = Directory.GetFiles(dir).Where(file => Path.GetExtension(file) == ".txt").ToArray();
            for (int i = 0; i < files.Length; i++)
                names.Add(Path.GetFileNameWithoutExtension(files[i]), i);
        }

        public IGtfsTable GetTable(string name)
        {
            if (!names.ContainsKey(name))
                return null;
            return new GtfsTable(files[names[name]]);
        }

        public string GetIdValue(string data)
        {
            if (data.Contains('@'))
                return data.Substring(0, data.Length - 2);
            else return data;
        }
    }
}
