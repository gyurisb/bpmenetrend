using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TransitBaseTransformation.DataSource;

namespace TransitBaseIntegration.Gtfs
{
    public class GtfsTable : IGtfsTable
    {
        public Dictionary<string, int> header = new Dictionary<string, int>();
        private StreamReader reader;

        public GtfsTable(string table)
        {
            reader = new StreamReader(table);
            //string[] headerArray = reader.ReadLine().Replace("\"", "").Split(",".ToArray());
            string[] headerArray = SplitLine(reader.ReadLine());
            for (int i = 0; i < headerArray.Length; i++)
                header.Add(headerArray[i], i);
            DataLength = reader.BaseStream.Length;
        }

        public IEnumerable<IGtfsRecord> Records
        {
            get
            {
                string lineString = null;
                while ((lineString = reader.ReadLine()) != null)
                {
                    string[] currentLine = SplitLine(lineString);

                    yield return new GtfsRecord(header, currentLine) as IGtfsRecord;
                }
            }
        }

        public static string[] SplitLine(string lineString)
        {
            char[] line = lineString.ToArray();

            bool inCommas = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"') inCommas = !inCommas;
                if (!inCommas && line[i] == ',')
                    line[i] = '\0';
                if (i > 0 && (line[i - 1] == '\0' || line[i - 1] == '"') && line[i] == ' ')
                    line[i] = '"';
            }
            return new String(line)
                .Replace("\"", "")
                .Split('\0')
                .ToArray();
        }

        public int ProcessPercent
        {
            get
            {
                return (int)(DataPosition * (long)100 / DataLength);
            }
        }

        internal long DataLength { get; private set; }
        internal long DataPosition { get { return reader.BaseStream.Position; } }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
