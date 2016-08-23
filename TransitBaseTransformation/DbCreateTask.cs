using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FastDatabaseSaver;
using TransitBase.Entities;
using TransitBase;
using StreetPathData;
using TransitBaseTransformation.DataSource;

namespace TransitBaseTransformation
{
    public class DbCreateTask
    {
        private IGtfsDatabase gtfs;
        private AppSourceConfig config;
        private StaticDBCreater sdb;
        private string outputPath;

        public delegate void LogHandler(int percent, String message);
        public event LogHandler Log;
        private void log(int percent, String message)
        {
            if (Log != null)
                Log(percent, message);
        }

        public DbCreateTask(IGtfsDatabase gtfsDb, string outputPath, AppSourceConfig config)
        {
            this.outputPath = outputPath;
            this.gtfs = gtfsDb;
            this.sdb = new StaticDBCreater(outputPath, config.CreateString);
            this.config = config;

            var tb = new TransitBaseComponent
            {
                LatitudeDegreeDistance = config.LatitudeUnit,
                LongitudeDegreeDistance = config.LongitudeUnit,
                //FilterTravelRouteStops = config.FilterTravelRouteStops
            };
        }

        private Dictionary<string, Stop> getStop = new Dictionary<string, Stop>();
        private Dictionary<string, RouteGroup> getRouteGroup = new Dictionary<string, RouteGroup>();
        private Dictionary<Tuple<string, byte>, Route> getRoute = new Dictionary<Tuple<string, byte>, Route>();
        private Dictionary<string, Trip> getTrip = new Dictionary<string, Trip>();
        private Dictionary<string, Service> getService = new Dictionary<string, Service>();
        private Dictionary<string, Shape> getShape = new Dictionary<string, Shape>();

        private Dictionary<string, Route> getRouteOfTrip = new Dictionary<string, Route>();
        private Dictionary<Trip, Shape> getShapeOfTrip = new Dictionary<Trip, Shape>();
        private Dictionary<Trip, string> getNameOfTrip = new Dictionary<Trip, string>();
        private Shape nullShape;

        public bool Execute()
        {
            registerTypes();
            log(0, "Converting stops!");
            convertStopTable();
            log(0, "Creating stopgroups!");
            createStopGroups();
            log(0, "Calculating transfers!");
            bool success = calculateTransferTimes();
            if (success)
            {
                log(0, "Converting shapes!");
                convertShapeTable();
                log(0, "Converting routes!");
                convertRouteTable();
                log(0, "Converting calendar!");
                convertCalendarTable();
                log(0, "Converting trips!");
                convertTripTable();
                log(0, "Converting stop_times!");
                convertStopTimesTable();
                log(0, "Creating stoplist cache for routes!");
                createRouteStopCache();

                log(0, "Executing repairs");
                PostDbRepair.DoRepair(config.CitySign, sdb, log);
            }

            log(0, "Saving database!");
            sdb.SaveAll(progress => { log(progress, null); });
            log(0, "Creating metafile!");
            using (var fileStream = File.OpenWrite(outputPath + "\\meta.txt"))
                MetaData.SerializeDatabase(sdb, fileStream);
            log(0, "Done!");
            return success;
        }

        private void registerTypes()
        {
            sdb.RegisterTableType(typeof(Stop));
            sdb.RegisterTableType(typeof(Agency));
            sdb.RegisterTableType(typeof(RouteGroup));
            sdb.RegisterTableType(typeof(Route));
            sdb.RegisterTableType(typeof(StopGroup));
            sdb.RegisterTableType(typeof(Service));
            sdb.RegisterTableType(typeof(CalendarException));
            sdb.RegisterTableType(typeof(Trip));
            sdb.RegisterTableType(typeof(TripType));
            sdb.RegisterTableType(typeof(StopEntry));
            sdb.RegisterTableType(typeof(TTEntry));
            sdb.RegisterTableType(typeof(TripTimeType));
            sdb.RegisterTableType(typeof(TimeEntry));
            sdb.RegisterTableType(typeof(Transfer));
            sdb.RegisterTableType(typeof(TransferPoint));
            sdb.RegisterTableType(typeof(RouteStopEntry));
            sdb.RegisterTableType(typeof(Shape));
            sdb.RegisterTableType(typeof(ShapePoint));
        }

        private void convertStopTable()
        {
            using (var table = gtfs.GetTable("stops"))
            {
                foreach (var record in table.Records)
                {
                    Stop entity = new Stop
                    {
                        Name = record["stop_name"],
                        Latitude = double.Parse(record["stop_lat"], System.Globalization.CultureInfo.InvariantCulture),
                        Longitude = double.Parse(record["stop_lon"], System.Globalization.CultureInfo.InvariantCulture),
                        WheelchairBoarding = byte.Parse(record["wheelchair_boarding"] ?? "0", System.Globalization.CultureInfo.InvariantCulture)
                    };

                    getStop.Add(record["stop_id"], entity);
                    sdb.AddEntity(entity);

                    calculatePercent(table);
                }
            }
        }

        private void createStopGroups()
        {
            List<Stop> stopList = config.Sort(getStop.Values).ToList();
            int count = stopList.Count;
            while (stopList.Count > 0)
            {
                //find a group of stops with same name (or similar)
                Stop stop = stopList.First();
                List<Stop> sameStops = new List<Stop>();
                sameStops.Add(stop);
                foreach (Stop stopOther in stopList.Skip(1))
                {
                    if (config.IsStopSame(stop, stopOther))
                        sameStops.Add(stopOther);
                    else break;
                }

                //find stops that are in the same network from the group
                List<Stop> networkStops = new List<Stop>();
                List<Stop> newNetworkStops = new List<Stop>();
                networkStops.Add(stop);
                newNetworkStops.Add(stop);
                while (true)
                {
                    var neighbors = newNetworkStops.SelectMany(s => sameStops.Where(x => s.StraightLineDistanceTo(x) < 300)).Distinct().ToList();
                    if (neighbors.Count == 0)
                        break;
                    networkStops.AddRange(neighbors);
                    foreach (var n in neighbors)
                        sameStops.Remove(n);
                    newNetworkStops = neighbors;
                }

                //create a stopgroup, and fill with elements
                StopGroup newGroup = new StopGroup { Name = stop.Name };
                sdb.AddEntity(newGroup);
                foreach (Stop curStop in networkStops)
                {
                    curStop.Group = newGroup;
                    stopList.Remove(curStop);
                }

                log(100 - 100 * stopList.Count / count, null);
            }
        }
        private IEnumerable<Stop> NearStops(Stop stop, IEnumerable<Stop> selection)
        {
            return selection.Where(s => stop.StraightLineDistanceTo(s) < 400);
        }

        private void convertRouteTable()
        {
            var getAgency = new Dictionary<string, Agency>();
            using (var table = gtfs.GetTable("agency"))
            {
                foreach (var record in table.Records)
                {
                    string id = record["agency_id"];
                    Agency entity = new Agency
                    {
                        Name = record["agency_name"],
                        ShortName = id
                    };
                    if (!getAgency.ContainsKey(id))
                    {
                        sdb.AddEntity(entity);
                        getAgency.Add(id, entity);
                    }
                }
            }

            using (var table = gtfs.GetTable("routes"))
            {
                var typeConversion = config.GetRouteTypeConversion();
                foreach (var record in table.Records)
                {
                    string id = record["route_id"];
                    int type = int.Parse(record["route_type"]);
                    if (typeConversion != null && typeConversion.ContainsKey(type))
                        type = typeConversion[type];
                    RouteGroup entity = new RouteGroup
                    {
                        Name = record["route_short_name"] ?? record["route_long_name"],
                        Description = record["route_long_name"] ?? "" + record["route_desc"] ?? "",
                        Type = (byte)type,
                        BgColor = UInt32.Parse(record["route_color"] ?? "FFFFFF", System.Globalization.NumberStyles.HexNumber),
                        FontColor = UInt32.Parse(record["route_text_color"] ?? "000000", System.Globalization.NumberStyles.HexNumber),
                        Agency = getAgency[record["agency_id"]]
                    };
                    if (entity.Type > 7)
                        throw new InvalidDataException("Invalid route type number");

                    var sameEntity = getRouteGroup.FirstOrDefault(x =>
                            gtfs.GetIdValue(x.Key) == gtfs.GetIdValue(id) &&
                            x.Value.Name == entity.Name &&
                            x.Value.Description == entity.Description).Value ?? null;

                    if (sameEntity == null)
                        sdb.AddEntity(entity);
                    else
                        entity = sameEntity;
                    getRouteGroup.Add(id, entity);

                    //RouteGroup sameEntity;
                    //getRouteGroup.TryGetValue(id, out sameEntity);
                    //if (sameEntity == null)
                    //{
                    //    sdb.AddEntity(entity);
                    //    getRouteGroup.Add(id, entity);
                    //}
                    //else if (sameEntity.Description != entity.Description || sameEntity.Name != entity.Name)
                    //    throw new InvalidDataException("RouteGroup id multiplication");

                    calculatePercent(table);
                }
            }
            foreach (var group in getRouteGroup.GroupBy(x => x.Value))
            {
                Route r0 = new Route { RouteGroup = group.Key, Name = "Unknown" }, r1 = new Route { RouteGroup = group.Key, Name = "Unknown" };
                sdb.AddEntity(r0);
                sdb.AddEntity(r1);
                foreach (var id in group)
                {
                    getRoute.Add(Tuple.Create(id.Key, (byte)0), r0);
                    getRoute.Add(Tuple.Create(id.Key, (byte)1), r1);
                }
            }
        }

        private void convertCalendarTable()
        {
            using (var table = gtfs.GetTable("calendar"))
            {
                foreach (var record in table.Records)
                {
                    bool[] days = new bool[7];
                    days[0] = record["sunday"] == "1";
                    days[1] = record["monday"] == "1";
                    days[2] = record["tuesday"] == "1";
                    days[3] = record["wednesday"] == "1";
                    days[4] = record["thursday"] == "1";
                    days[5] = record["friday"] == "1";
                    days[6] = record["saturday"] == "1";
                    Service entity = new Service
                    {
                        Days = days,
                        startDate = new DateTime(Int32.Parse(record["start_date"].Substring(0, 4)),
                            Int32.Parse(record["start_date"].Substring(4, 2)), Int32.Parse(record["start_date"].Substring(6, 2))),
                        endDate = new DateTime(Int32.Parse(record["end_date"].Substring(0, 4)),
                            Int32.Parse(record["end_date"].Substring(4, 2)), Int32.Parse(record["end_date"].Substring(6, 2)))
                    };
                    sdb.AddEntity(entity);
                    getService.Add(record["service_id"], entity);

                    calculatePercent(table);
                }
            }
            using (var table = gtfs.GetTable("calendar_dates"))
            {
                foreach (var record in table.Records)
                {
                    Service serviceEntity = null;
                    if (!getService.ContainsKey(record["service_id"]))
                    {
                        serviceEntity = Service.CreateEmptyService();
                        sdb.AddEntity(serviceEntity);
                        getService.Add(record["service_id"], serviceEntity);
                    }
                    CalendarException entity = new CalendarException
                    {
                        Date = new DateTime(Int32.Parse(record["date"].Substring(0, 4)),
                            Int32.Parse(record["date"].Substring(4, 2)), Int32.Parse(record["date"].Substring(6, 2))),
                        Type = Int32.Parse(record["exception_type"]),
                        Service = getService[record["service_id"]] ?? serviceEntity
                    };
                    sdb.AddEntity(entity);

                    calculatePercent(table);
                }
            }
        }
        
        private void convertTripTable()
        {
            //var routeNames = new Dictionary<Tuple<string, byte>, Dictionary<string, int>>();

            using (var table = gtfs.GetTable("trips"))
            {
                foreach (var record in table.Records)
                {
                    //route kezelése:
                    string headsign = record["trip_headsign"] ?? "";
                    var routeId = Tuple.Create(record["route_id"], Byte.Parse(record["direction_id"]));

                    //if (headsign != "")
                    //{
                    //    if (!routeNames.Keys.Contains(routeId))
                    //        routeNames.Add(routeId, new Dictionary<string, int>());
                    //    var dict = routeNames[routeId];
                    //    if (!dict.Keys.Contains(headsign)) dict.Add(headsign, 1);
                    //    else dict[headsign] = dict[headsign] + 1;
                    //}

                    string wheelchair = record["wheelchair_accessible"] ?? "0";
                    if (wheelchair != "0" && wheelchair != "1" && wheelchair != "2")
                        throw new InvalidDataException();

                    //trip kezelése:
                    Trip entityTrip = new Trip()
                    {
                        Service = getService[record["service_id"]],
                        WheelchairAccessible = wheelchair=="0" ? (bool?)null : (wheelchair == "1")
                    };
                    Route route = getRoute[routeId];
                    Shape shape = getShape[record["shape_id"] ?? ""];
                    getRouteOfTrip.Add(record["trip_id"], route);
                    getShapeOfTrip.Add(entityTrip, shape);

                    sdb.AddEntity(entityTrip);
                    getTrip.Add(record["trip_id"], entityTrip);
                    getNameOfTrip.Add(entityTrip, headsign);

                    calculatePercent(table);
                }

                foreach (var group in getTrip.Select(x => new { Route = getRouteOfTrip[x.Key], Name = getNameOfTrip[x.Value] }).GroupBy(x => x.Route))
                {
                    group.Key.Name = group.Select(x => x.Name).MostFrequent();
                }
                //foreach (var route in sdb.GetTable<Route>().ToArray())
                //{
                //    if (route.Name == "Unknown")
                //        sdb.RemoveEntity(route);
                //}
                //Remove unused routegroups (not necessary)
                //var validGroups = sdb.GetTable<Route>().Select(r => r.RouteGroup).Distinct().ToList();
                //var invalidGroups = sdb.GetTable<RouteGroup>().Except(validGroups).ToList();
                //sdb.RemoveEntityAll(invalidGroups);
            }
        }

        private void convertStopTimesTable()
        {
            //sdb.RegisterTableType(typeof(TripTypeHeadsign));
            var set = new SortedDictionary<StopTimeList, List<string>>();
            var timeDiffs = new List<int>();
            var seenTrips = new HashSet<string>();

            using (var table = gtfs.GetTable("stop_times"))
            {
                string lastTrip = null;
                StopTimeList curList = null;
                TimeSpan startTime = new TimeSpan();
                foreach (var record in table.Records)
                {
                    string trip = record["trip_id"];
                    string arrivalTime = record["arrival_time"];
                    string departureTime = record["departure_time"];
                    if (trip != lastTrip)
                    {
                        if (lastTrip != null)
                        {
                            if (!set.Keys.Contains(curList))
                                set.Add(curList, new List<String>());
                            set[curList].Add(lastTrip);
                        }
                        startTime = parseGtfsTime(arrivalTime);
                        getTrip[trip].StartTime = startTime;
                        curList = new StopTimeList(getRouteOfTrip[trip]);
                        lastTrip = trip;
                        if (seenTrips.Contains(trip))
                            throw new InvalidDataException("Trip contained in an interrupted sequence.");
                        seenTrips.Add(trip);
                    }
                    TimeSpan curTime = parseGtfsTime(arrivalTime);
                    TimeSpan curEndTime = parseGtfsTime(departureTime);
                    while (curTime < startTime) curTime += TimeSpan.FromDays(1);
                    curList.Add(Tuple.Create(getStop[record["stop_id"]], curTime - startTime, record["stop_headsign"]));

                    timeDiffs.Add((int)(curEndTime - curTime).TotalMinutes);
                    calculatePercent(table);
                }
                if (lastTrip != null)
                {
                    if (!set.Keys.Contains(curList))
                        set.Add(curList, new List<String>());
                    set[curList].Add(lastTrip);
                }
            }
            foreach (var pair in set)
                pair.Key.CreateLists(pair.Value.Select(str => getTrip[str]).ToList());

            var getTripType = new Dictionary<StopList, TripType>();
            foreach (var stopList in StopTimeList.StopLists)
            {
                TripType tt = new TripType { Route = stopList.Key.Route };
                tt.HeadsignEntries = new List<TripTypeHeadsign>();
                tt.Route.TripTypes = addToList(tt, tt.Route.TripTypes);
                sdb.AddEntity(tt);
                getTripType[stopList.Value] = tt;
                string prevName = tt.Route.Name;
                int pos = 0;
                foreach (var stopAndName in stopList.Key)
                {
                    StopEntry se = new StopEntry { Stop = stopAndName.Item1, TripType = tt };
                    tt.StopEntries = addToList(se, tt.StopEntries);
                    TTEntry te = new TTEntry { Position = pos, Stop = stopAndName.Item1, TripType = tt };
                    sdb.AddEntity(se);
                    sdb.AddEntity(te);
                    if (stopAndName.Item2 != null && stopAndName.Item2 != prevName)
                    {
                        TripTypeHeadsign he = new TripTypeHeadsign { Headsign = stopAndName.Item2, StartIndex = (short)pos, TripType = tt };
                        tt.HeadsignEntries = addToList(he, tt.HeadsignEntries);
                        //sdb.AddEntity(he);
                        prevName = stopAndName.Item2;
                    }
                    pos++;
                }
            }
            foreach (var timeList in StopTimeList.TimeLists)
            {
                TripTimeType ttt = new TripTimeType { TripType = getTripType[timeList.Key.Base] };
                ttt.TripType.TripTimeTypes = addToList(ttt, ttt.TripType.TripTimeTypes);
                sdb.AddEntity(ttt);
                foreach (var timeEntry in timeList.Key)
                {
                    TimeEntry te = new TimeEntry { Time = (short)timeEntry.TotalMinutes, TripTimeType = ttt };
                    sdb.AddEntity(te);
                    ttt.TimeEntries = addToList(te, ttt.TimeEntries);
                }
                foreach (var trip in timeList.Key.Trips)
                {
                    trip.TripTimeType = ttt;
                    ttt.Trips = addToList(trip, ttt.Trips);
                    //if (ttt.TripType.Shape == null)
                    //    ttt.TripType.Shape = getShapeOfTrip[trip];
                    //else if (ttt.TripType.Shape != getShapeOfTrip[trip])
                    //    throw new InvalidDataException();
                }
            }
            var emptyTrips = sdb.GetTable<Trip>().Where(t => t.TripTimeType == null).ToList();
            Log(0, "Trips missing from stop_times.txt count: " + emptyTrips.Count);
            sdb.RemoveEntityAll(emptyTrips);

            //Setting shapes to TripTypes
            int wrongShapeTrips = 0;
            foreach (var tt in sdb.GetTable<TripType>())
            {
                var tripAndShapes = tt.Trips.Select(trip => new { Trip = trip, Shape = getShapeOfTrip[trip] }).ToList();
                tt.Shape = tripAndShapes.GroupBy(x => x.Shape).MaxBy(x => x.Key == nullShape ? int.MinValue : x.Count()).Key;
                wrongShapeTrips += tripAndShapes.Where(x => x.Shape != tt.Shape).Count();
            }
            log(0, "Shapes set to triptypes. Trips with wrong shape: " + wrongShapeTrips * 100 / getTrip.Count + "%");

            //Settings names to TripTypes
            int wrongNameTrips = 0;
            foreach (var tt in sdb.GetTable<TripType>())
            {
                var ttNames = tt.Trips.Select(trip => getNameOfTrip[trip]).Except(new string[] { null, "" }).ToList();
                if (ttNames.Any())
                {
                    tt.Name = ttNames.MostFrequent();
                    wrongNameTrips += ttNames.Except(new string[] { tt.Name }).Count();
                }
            }
            log(0, "Names set to triptypes. Trips with wrong name: " + wrongNameTrips * 100 / getTrip.Count + "%");

            //setting unknown route names
            var unknownTripTypes = sdb.GetTable<TripType>().Where(tt => tt.Name == "" || tt.Name == null).ToList();
            foreach (var tt in unknownTripTypes)
            {
                if (tt.HeadsignEntries.Any())
                {
                    var names = new Dictionary<string, int>();
                    for (int i = 0; i < tt.HeadsignEntries.Count; i++)
                    {
                        var headsignEntry = tt.HeadsignEntries[i];
                        int size = (i < tt.HeadsignEntries.Count - 1 ? tt.HeadsignEntries[i + 1].StartIndex : tt.StopEntries.Count) - headsignEntry.StartIndex;
                        int oldValue = 0;
                        names.TryGetValue(headsignEntry.Headsign, out oldValue);
                        names[headsignEntry.Headsign] = oldValue + size;
                    }
                    tt.Name = names.MaxBy(n => n.Value).Key;
                }
                else
                {
                    var stoplist = sdb.GetTable<StopEntry>().Where(se => se.TripType == tt);
                    tt.Name = stoplist.Last().Stop.Name;
                }
            }
            var unknownRoutes = sdb.GetTable<Route>().Where(r => r.Name == "Unknown" || r.Name == "" || r.Name == null).ToList();
            foreach (var route in unknownRoutes)
            {
                var triptypes = sdb.GetTable<TripType>().Where(tt => tt.Route == route).ToList();
                if (triptypes.Count == 0)
                    sdb.RemoveEntity(route);
                else
                    route.Name = triptypes.Where(tt => tt.Name != "" && tt.Name != null).GroupBy(tt => tt.Name).MaxBy(x => x.Sum(y => y.Trips.Count())).Key;
            }

            log(0, "Average end-stop time difference: " + timeDiffs.Average() + ", max diff: " + timeDiffs.Max());

            log(0, "Compressing stop_times done! First compression efficiency: " + set.Average(x => x.Value.Count));
            long firstSize = set.Sum(x => x.Key.Count * (4 + 2));
            long totalSize = StopTimeList.StopLists.Sum(x => x.Key.Count * 4) + StopTimeList.TimeLists.Sum(x => x.Key.Count * 2);
            log(0, "Second total db compression rate: " + (totalSize * 100 / firstSize) + "%");
            log(0, "Stoplist number compression rate: " + (StopTimeList.StopLists.Count * 100 / set.Count) + "%");

            int changingCount = sdb.GetTable<TripType>().Where(tt => tt.HeadsignEntries.Count > 1).Count();
            int totalTripCount = sdb.GetTable<TripType>().Count();
            log(0, string.Format("Triptype with changing name: {0}/{1} ({2}%)", changingCount, totalTripCount, changingCount * 100 / totalTripCount));
        }

        private void createRouteStopCache()
        {
            foreach (var route in sdb.GetTable<Route>())
            {
                foreach (var entry in route.CreateTravelRoute(config.FilterTravelRouteStops))
                {
                    sdb.AddEntity(entry);
                }
            }
            log(0, String.Format("Failed routecalculations: {0} ({1}%)", Route.FailedRequests, Route.FailedRequests * 100.0 / Route.TotalRequests));
        }

        private void convertShapeTable()
        {
            nullShape = new Shape();
            getShape.Add("", nullShape);
            sdb.AddEntity(nullShape);
            using (var table = gtfs.GetTable("shapes"))
            {
                foreach (var record in table.Records)
                {
                    if (!getShape.ContainsKey(record["shape_id"]))
                    {
                        Shape newShape = new Shape
                        {
                            Latitude = Double.Parse(record["shape_pt_lat"], System.Globalization.CultureInfo.InvariantCulture),
                            Longitude = Double.Parse(record["shape_pt_lon"], System.Globalization.CultureInfo.InvariantCulture)
                        };
                        getShape.Add(record["shape_id"], newShape);
                        sdb.AddEntity(newShape);
                    }
                    double lat = Double.Parse(record["shape_pt_lat"], System.Globalization.CultureInfo.InvariantCulture);
                    double lon = Double.Parse(record["shape_pt_lon"], System.Globalization.CultureInfo.InvariantCulture);
                    var shape = getShape[record["shape_id"]];
                    int dlat = (int)Math.Round((lat - shape.Latitude) * 10000);
                    int dlon = (int)Math.Round((lon - shape.Longitude) * 10000);
                    if (dlat > short.MaxValue || dlon > short.MaxValue || dlat < short.MinValue || dlon < short.MinValue)
                        throw new OverflowException();
                    ShapePoint sp = new ShapePoint
                    {
                        DLat = (short)dlat,
                        DLon = (short)dlon,
                        Shape = shape,
                        Index = int.Parse(record["shape_pt_sequence"])
                    };
                    sdb.AddEntity(sp);
                    calculatePercent(table);
                }
            }
        }

        private bool calculateTransferTimes()
        {
            int errorCount = 0;
            PathFinder pathFinder = new PathFinder();
            pathFinder.LoadData(Path.Combine(config.KnowledgePath, "transfers.xml"));
            var stops = sdb.GetTable<Stop>().ToArray();
            for (int i = 0; i < stops.Length - 1; i++)
            {
                for (int k = i + 1; k < stops.Length; k++)
                {
                    if (PathFinder.IsNear(stops[i], stops[k]))
                    {
                        WalkPath path = null;
                        short dist = (short)stops[i].StraightLineDistanceTo(stops[k]);
                        bool notSame = PathFinder.IsNearNotSamePace(stops[i], stops[k]);
                        if (notSame)
                        {
                            path = pathFinder.GetPath(
                                new Point { Latitude = stops[i].Latitude, Longitude = stops[i].Longitude },
                                new Point { Latitude = stops[k].Latitude, Longitude = stops[k].Longitude }
                            );
                            if (path == null)
                            {
                                errorCount++;
                                continue;
                            }
                            dist = short.Parse(Math.Round(path.Distance).ToString());
                        }
                        Transfer transfer1 = new Transfer
                        {
                            Distance = dist,
                            Origin = stops[i],
                            Target = stops[k]
                        };
                        Transfer transfer2 = new Transfer
                        {
                            Distance = dist,
                            Origin = stops[k],
                            Target = stops[i]
                        };
                        if (notSame)
                        {
                            if (stops[i].Latitude == path.From.Latitude && stops[i].Longitude == path.From.Longitude)
                            {
                                createInnerPoints(transfer1, path, false);
                                createInnerPoints(transfer2, path, true);
                            }
                            else if (stops[k].Latitude == path.From.Latitude && stops[k].Longitude == path.From.Longitude)
                            {
                                createInnerPoints(transfer1, path, true);
                                createInnerPoints(transfer2, path, false);
                            }
                            else throw new Exception("Something is wrong.");
                        }

                        sdb.AddEntity(transfer1);
                        sdb.AddEntity(transfer2);
                    }
                }
                log(i * 100 / (stops.Length - 1), null);
            }
            if (errorCount > 0)
            {
                log(0, "Missing transfers: " + errorCount + ". Calculate transfers then rerun the process.");
                return false;
            }
            return true;
        }
        private void createInnerPoints(Transfer parent, WalkPath path, bool reversed)
        {
            IEnumerable<Point> points = path.InnerPoints;
            if (reversed)
                points = points.Reverse();
            foreach (var point in points)
            {
                TransferPoint entity = new TransferPoint
                {
                    Transfer = parent,
                    DLat = (short)Math.Round((point.Latitude - parent.Origin.Latitude) * 10000.0),
                    DLon = (short)Math.Round((point.Longitude - parent.Origin.Longitude) * 10000.0)
                };
                sdb.AddEntity(entity);
            }
        }

        #region Trip stop relation classes
        private class StopTimeList : List<Tuple<Stop, TimeSpan, string>>, IComparable<StopTimeList>
        {
            public Route Route;
            public StopList StopList;
            public TimeList TimeList;

            public StopTimeList(Route route)
            {
                this.Route = route;
            }

            public static SortedDictionary<StopList, StopList> StopLists = new SortedDictionary<StopList, StopList>();
            public static SortedDictionary<TimeList, TimeList> TimeLists = new SortedDictionary<TimeList, TimeList>();

            public void CreateLists(List<Trip> trips)
            {
                var stopList = new StopList(this.Select(x => Tuple.Create(x.Item1, x.Item3)), Route);
                if (!StopLists.ContainsKey(stopList))
                    StopLists.Add(stopList, stopList);
                StopList = StopLists[stopList];

                var timeList = new TimeList(this.Select(x => x.Item2), StopList);
                if (!TimeLists.ContainsKey(timeList))
                    TimeLists.Add(timeList, timeList);
                else throw new InvalidOperationException();
                TimeList = TimeLists[timeList];
                TimeList.Trips = trips;
            }

            public int CompareTo(StopTimeList other)
            {
                if (Route != other.Route)
                    return Route.GetHashCode() - other.Route.GetHashCode();

                if (Count != other.Count) return Count - other.Count;
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].Item1 != other[i].Item1)
                        return this[i].Item1.GetHashCode() - other[i].Item1.GetHashCode();
                    if (this[i].Item2 != other[i].Item2)
                        return this[i].Item2.CompareTo(other[i].Item2);
                }
                //for (int i = 0; i < Count; i++)
                //    if (this[i].Item3 != other[i].Item3)
                //        throw new Exception("Wrong stop_headsign field (ambigousity)");
                return 0;
            }
        };

        private class StopList : List<Tuple<Stop, string>>, IComparable<StopList>
        {
            public Route Route;
            public StopList(IEnumerable<Tuple<Stop, string>> stops, Route route) : base(stops) { this.Route = route; }
            public int CompareTo(StopList other)
            {
                if (Route != other.Route) return Route.GetHashCode() - other.Route.GetHashCode();
                if (Count != other.Count) return Count - other.Count;
                for (int i = 0; i < Count; i++)
                    if (this[i].Item1 != other[i].Item1)
                        return this[i].Item1.GetHashCode() - other[i].Item1.GetHashCode();
                //for (int i = 0; i < Count; i++)
                //    if (this[i].Item2 != other[i].Item2)
                //        throw new Exception("Wrong stop_headsign field (ambigousity)");
                return 0;
            }
        }
        private class TimeList : List<TimeSpan>, IComparable<TimeList>
        {
            public StopList Base;
            public TimeList(IEnumerable<TimeSpan> times, StopList base_) : base(times) { this.Base = base_; }
            public int CompareTo(TimeList other)
            {
                if (Base != other.Base) return Base.CompareTo(other.Base);
                if (Count != other.Count) return Count - other.Count;
                for (int i = 0; i < Count; i++)
                    if (this[i] != other[i])
                        return this[i].CompareTo(other[i]);
                return 0;
            }

            public List<Trip> Trips;
        }
        #endregion

        private int lastPercent = 0;
        int rowCount = 0;
        private void calculatePercent(IGtfsTable table)
        {
            if (rowCount == 0)
            {
                rowCount = 50;
                int percent = table.ProcessPercent;
                if (percent != lastPercent)
                {
                    lastPercent = percent;
                    log(percent, null);
                }
            }
            rowCount--;
        }

        private IList<T> addToList<T>(T entity, IList<T> list)
        {
            IList<T> retList = list;
            if (retList == null) retList = new List<T>();
            retList.Add(entity);
            return retList;
        }

        private void BreakPointAdd(params object[] p) { }

        private TimeSpan parseGtfsTime(string text)
        {
            string[] parts = text.Split(':');
            return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), 0);
        }
    }
}
