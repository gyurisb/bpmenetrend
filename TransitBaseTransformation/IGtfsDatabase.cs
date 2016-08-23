using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBaseTransformation.DataSource
{
    public interface IGtfsDatabase
    {
        IGtfsTable GetTable(string name);
        string GetIdValue(string key);
    }

    public interface IGtfsTable : IDisposable
    {
        int ProcessPercent { get; }
        IEnumerable<IGtfsRecord> Records { get; }
    }

    public interface IGtfsRecord
    {
        string this[String column] { get; }
    }
}
