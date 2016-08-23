using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using FastDatabaseLoader;
using FastDatabaseLoader.Tables;
using TransitBase.Entities;
using TransitBase.BusinessLogic;
using NetPortableServices;

namespace TransitBase
{
    public class TransitBaseComponent : IDisposable
    {
        public static TransitBaseComponent Current { get; private set; }

        public TransitDB DB { get; private set; }
        public bool CheckSameRoutes { get; set; }
        public double LatitudeDegreeDistance { get; set; }
        public double LongitudeDegreeDistance { get; set; }
        public TransitLogic Logic { get; private set; }
        public Task UsingFiles { get; private set; }

        #region loader functions

        public TransitBaseComponent()
        {
            Current = this;
        }

        public TransitBaseComponent(IDirectoryService root, IDirectionsService directionsService, bool preLoad, int bigTableLimit, bool checkSameRoutes, double latitudeDegreeDistance, double longitudeDegreeDistance)
        {
            DB = new TransitDB(root, bigTableLimit, preLoad);
            Initialize(directionsService, checkSameRoutes, latitudeDegreeDistance, longitudeDegreeDistance);
        }

        public TransitBaseComponent(IDirectoryService root, double latitudeDegreeDistance, double longitudeDegreeDistance)
        {
            DB = new TransitDB(root, int.MaxValue, true);
            Initialize(null, false, latitudeDegreeDistance, longitudeDegreeDistance);
        }

        private void Initialize(IDirectionsService directionsService,  bool checkSameRoutes, double latitudeDegreeDistance, double longitudeDegreeDistance)
        {
            //if (Current != null)
            //    throw new InvalidOperationException("SingletonException: Only a single TransitDB object can be present.");
            Current = this;
            UsingFiles = new Task(() => { });

            this.CheckSameRoutes = checkSameRoutes;
            this.LatitudeDegreeDistance = latitudeDegreeDistance;
            this.LongitudeDegreeDistance = longitudeDegreeDistance;

            if (DB.DatabaseExists)
            {
                //Init(reserveSize: preLoad);
                Logic = new TransitLogic(this, directionsService);
            }
        }

        public void LoadDataFiles()
        {
            if (DB.DatabaseExists)
                DB.LoadData();
        }

        public void LoadAllEntities()
        {
            if (DB.DatabaseExists)
            {
                Task.Run(async () =>
                {
                    await Task.WhenAll(DB.RouteGroups.Loaded, DB.StopGroups.Loaded);
                    Logic.LoadCache();
                });
                DB.Load();
            }
        }

        #endregion

        #region table getters
        public Table<RouteGroup> RouteGroups { get { return DB.RouteGroups; } }
        public Table<StopGroup> StopGroups { get { return DB.StopGroups; } }
        public Table<Shape> Shapes { get { return DB.Shapes; } }
        public Table<ShapePoint> ShapePoints { get { return DB.ShapePoints; } }
        public Table<Service> Services { get { return DB.Services; } }
        public Table<CalendarException> CalendarExceptions { get { return DB.CalendarExceptions; } }
        public Table<Agency> Agencies { get { return DB.Agencies; } }
        public Table<Route> Routes { get { return DB.Routes; } }
        public Table<Stop> Stops { get { return DB.Stops; } }
        public Table<TripType> TripTypes { get { return DB.TripTypes; } }
        public Table<TripTimeType> TripTimeTypes { get { return DB.TripTimeTypes; } }
        public Table<Trip> Trips { get { return DB.Trips; } }
        public Table<StopEntry> StopEntries { get { return DB.StopEntries; } }
        public Table<TTEntry> TTEntries { get { return DB.TTEntries; } }
        public Table<TimeEntry> TimeEntries { get { return DB.TimeEntries; } }
        public Table<RouteStopEntry> RouteStopEntries { get { return DB.RouteStopEntries; } }
        public Table<Transfer> Transfers { get { return DB.Transfers; } }
        public Table<TransferPoint> InnerPoints { get { return DB.InnerPoints; } }
        #endregion

        //~TransitBaseComponent()
        //{
        //    Current = null;
        //}

        public void Dispose()
        {
            DB.Dispose();
        }

        public bool ContainsData { get { return Logic != null; } }
        public bool DatabaseExists { get { return DB.DatabaseExists; } }

        public void Flush()
        {
            DB.Flush();
        }
    }
}
