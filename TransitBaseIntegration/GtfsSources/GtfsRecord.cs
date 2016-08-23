using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBaseTransformation.DataSource;

namespace TransitBaseIntegration.Gtfs
{
    class GtfsRecord : IGtfsRecord
    {
        private Dictionary<string, int> header;
        private string[] currentLine;

        public GtfsRecord(Dictionary<string, int> header, string[] currentLine)
        {
            this.header = header;
            this.currentLine = currentLine;
        }

        public GtfsRecord(GtfsRecord other)
        {
            this.header = other.header;
            this.currentLine = other.currentLine;
        }

        public string this[int column] { get { return currentLine[column]; } }

        public virtual string this[String column]
        {
            get
            {
                if (header.ContainsKey(column))
                {
                    string val =  currentLine[header[column]];
                    if (val.Any(ch => ch != ' '))
                        return val;
                }
                return null;
            }
        }
    }
}
