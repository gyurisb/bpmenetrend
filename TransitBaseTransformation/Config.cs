using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TransitBase.Entities;

namespace TransitBaseTransformation
{
    [Serializable]
    public class AppSourceConfig
    {
        [XmlArrayItem(ElementName = "Link")]
        public string[] Sources { get; set; }
        public double LatitudeUnit { get; set; }
        public double LongitudeUnit { get; set; }
        public bool FindEquivalentStops { get; set; }
        public bool DeCapslocking { get; set; }
        public bool FilterTravelRouteStops { get; set; }
        public string TempFilesDir { get; set; }
        public string TargetDir { get; set; }
        public string KnowledgeDir { get; set; }
        public bool StopTimesSortRequired { get; set; }
        public string CitySign { get; set; }

        private HashSet<string> CapsExceptions;
        private string Root;

        public int Size { get { return Sources.Length; } }
        public string TempFilesPath { get { return Path.Combine(Root, TempFilesDir); } }
        public string TargetPath { get { return Path.Combine(Root, TargetDir); } }
        public string KnowledgePath { get { return Path.Combine(Root, KnowledgeDir); } }

        private void Initialize()
        {
            if (DeCapslocking)
            {
                var capsLines = ReadLines(Path.Combine(KnowledgePath, "caps_exceptions.txt"));
                CapsExceptions = new HashSet<string>(capsLines.Select(str => str.ToLowerInvariant()));
            }
        }
        public static AppSourceConfig ReadFrom(string file)
        {
            AppSourceConfig config;
            XmlSerializer ser = new XmlSerializer(typeof(AppSourceConfig));
            using (var stream = File.OpenRead(file))
                config = (AppSourceConfig)ser.Deserialize(stream);
            config.Root = Path.GetDirectoryName(file);
            config.Initialize();
            return config;
        }

        public static IEnumerable<string> ReadLines(string linksFile)
        {
            using (StreamReader reader = new StreamReader(linksFile))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                    yield return line;
            }
        }

        public Dictionary<int, int> GetRouteTypeConversion()
        {
            if (File.Exists(Path.Combine(this.KnowledgePath, "route_convert.txt")))
            {
                return ReadLines(Path.Combine(this.KnowledgePath, "route_convert.txt"))
                    .ToDictionary(line => int.Parse(line.Split(' ')[0]), line => int.Parse(line.Split(' ')[1]));
            }
            return null;
        }

        public IEnumerable<Stop> Sort(IEnumerable<Stop> stops)
        {
            return stops.OrderBy(s => s.Name.ToLowerInvariant());
        }

        public bool IsStopSame(Stop a, Stop b)
        {
            if (FindEquivalentStops)
            {
                return b.Name.ToLowerInvariant().StartsWith(a.Name.ToLowerInvariant());
            }
            else
            {
                return a.Name.ToLowerInvariant() == b.Name.ToLowerInvariant();
            }
        }

        public string CreateString(string str)
        {
            if (DeCapslocking)
            {
                return String.Join("", Split(str).Select(word => CreateWord(word)));
            }
            else return str;
        }

        public string CreateWord(string word)
        {
            if (word.Any(ch => char.IsLower(ch))) return word;
            else if (CapsExceptions.Contains(word.ToLowerInvariant())) return word;
            else
            {
                StringBuilder builder = new StringBuilder(word);
                for (int i = 1; i < builder.Length; i++)
                    if (char.IsUpper(builder[i]))
                        builder[i] = char.ToLowerInvariant(builder[i]);
                return builder.ToString();
            }
        }

        private static HashSet<char> delimiters = new HashSet<char>(new char[] { ' ', '/', '-', '.', ',' });

        public IEnumerable<string> Split(string str)
        {
            StringBuilder builder = null;
            for (int i = 0; i < str.Length; i++)
            {
                if (delimiters.Contains(str[i]))
                {
                    if (builder != null)
                    {
                        yield return builder.ToString();
                        builder = null;
                    }
                    yield return str[i].ToString();
                }
                else
                {
                    if (builder == null) builder = new StringBuilder();
                    builder.Append(str[i]);
                }
            }
            if (builder != null)
            {
                yield return builder.ToString();
                builder = null;
            }
        }
    }
}
