using FastDatabaseLoader;
using FastDatabaseLoader.Tables;
using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace TransitBase
{
    public class TransitDB : FastDatabase
    {
        //public TransitDB(StorageFolder folder, int bigTableLimit, bool preload)
        //    : base("TransitBase.Entities.%, TransitBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", folder, bigTableLimit, preload)
        //{
        //}
        public TransitDB(IDirectoryService root, int bigTableLimit, bool preload)
            : base("TransitBase.Entities.%, TransitBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", root, bigTableLimit, preload)
        {
        }

        public Table<RouteGroup> RouteGroups;
        public Table<StopGroup> StopGroups;
        public Table<Shape> Shapes;
        public Table<ShapePoint> ShapePoints;
        public Table<Service> Services;
        public Table<CalendarException> CalendarExceptions;
        public Table<Agency> Agencies;
        public Table<Route> Routes;
        public Table<Stop> Stops;
        public Table<TripType> TripTypes;
        public Table<TripTimeType> TripTimeTypes;
        public Table<Trip> Trips;
        public Table<StopEntry> StopEntries;
        public Table<TTEntry> TTEntries;
        public Table<TimeEntry> TimeEntries;
        public Table<RouteStopEntry> RouteStopEntries;
        public Table<Transfer> Transfers;
        public Table<TransferPoint> InnerPoints;
        //public Table<TripTypeHeadsign> TripTypeHeadsigns;
    }
}
