using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBaseTransformation.DataSource;

namespace TransitBaseIntegration.Gtfs
{
    class GtfsDistributedTable : IGtfsTable
    {
        private GtfsTable[] tables;
        private int current = 0;

        private long dataPosition = 0;
        private long dataLength;

        public GtfsDistributedTable(IEnumerable<GtfsTable> tables)
        {
            this.tables = tables.ToArray();
            dataLength = tables.Where(t => t != null).Sum(t => t.DataLength);
        }


        public IEnumerable<IGtfsRecord> Records
        {
            get
            {
                current = 0;
                foreach (var table in tables)
                {
                    if (table != null)
                        foreach (var record in table.Records)
                            yield return new GtfsDistributedRecord(record as GtfsRecord, current) as IGtfsRecord;
                    current++;
                }
            }
        }

        private long completeTablesLength = 0;
        private int lastCurrent = -1;
        public int ProcessPercent
        { //csibi szerelmim:) kaca
            get
            {
                if (lastCurrent != current)
                {
                    completeTablesLength = tables.Take(current).Where(t => t != null).Sum(t => t.DataLength);
                    lastCurrent = current;
                }
                return (int)(100 * (completeTablesLength + tables[current].DataPosition) / dataLength);
            }
        }

        public void Dispose()
        {
            foreach (var table in tables.Where(t => t != null))
                table.Dispose();
        }
    }

    class GtfsDistributedRecord : GtfsRecord
    {
        private int current;

        private static HashSet<string> ids = new HashSet<string>(new string[] { "stop_id", "route_id", "shape_id", "service_id" });

        public GtfsDistributedRecord(GtfsRecord baseRec, int current)
            : base(baseRec)
        {
            if (current > 9)
                throw new ArgumentOutOfRangeException();
            this.current = current;
        }

        public override string this[string column]
        {
            get
            {
                if (ids.Contains(column) && base[column] != null)
                    return base[column] + "@" + current;
                return base[column];
            }
        }
    }
}
