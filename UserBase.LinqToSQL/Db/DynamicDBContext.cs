using Microsoft.Phone.Data.Linq;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserBase.LinqToSQL
{
    public class DyanamicDBContext : DataContext
    {
        public DyanamicDBContext(string connectionString, int currentDDBVersion, bool forbidMigration = false) //currentDDBVersion = Config.DDBVersion
            : base(connectionString)
        {
            if (!DatabaseExists())
            {
                CreateDatabase();
                DatabaseSchemaUpdater versionSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                versionSchemaUpdater.DatabaseSchemaVersion = currentDDBVersion;
                versionSchemaUpdater.Execute();
            }
            else
            {
                DatabaseSchemaUpdater versionSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                int version = versionSchemaUpdater.DatabaseSchemaVersion;
                if (version != currentDDBVersion)
                {
                    if (forbidMigration)
                        throw new InvalidOperationException("Database migration is forbidden!");
                    Migrates.Execute(versionSchemaUpdater, version, currentDDBVersion);
                }
            }

            try
            {
                FavoriteEntry entry = FavoriteEntries.FirstOrDefault();
            }
            catch (Exception e)
            {
                DatabaseSchemaUpdater databaseSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                databaseSchemaUpdater.AddTable<FavoriteEntry>();
                databaseSchemaUpdater.Execute();
            }
            try
            {
                TileEntry entry = TileEntries.FirstOrDefault();
            }
            catch (Exception e)
            {
                DatabaseSchemaUpdater databaseSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                databaseSchemaUpdater.AddTable<TileEntry>();
                databaseSchemaUpdater.Execute();
            }
            try
            {
                HistoryEntry entry = HistoryEntries.FirstOrDefault();
            }
            catch (Exception e)
            {
                DatabaseSchemaUpdater databaseSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                databaseSchemaUpdater.AddTable<HistoryEntry>();
                databaseSchemaUpdater.Execute();
            }
            try
            {
                StopHistoryEntry entry = StopHistoryEntries.FirstOrDefault();
            }
            catch (Exception e)
            {
                DatabaseSchemaUpdater databaseSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                databaseSchemaUpdater.AddTable<StopHistoryEntry>();
                databaseSchemaUpdater.Execute();
            }
            try
            {
                PlanningHistoryEntry entry = PlanningHistoryEntries.FirstOrDefault();
            }
            catch (Exception e)
            {
                DatabaseSchemaUpdater databaseSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                databaseSchemaUpdater.AddTable<PlanningHistoryEntry>();
                databaseSchemaUpdater.Execute();
            }
            try
            {
                RecentEntry entry = RecentEntries.FirstOrDefault();
            }
            catch (Exception e)
            {
                DatabaseSchemaUpdater databaseSchemaUpdater = this.CreateDatabaseSchemaUpdater();
                databaseSchemaUpdater.AddTable<RecentEntry>();
                databaseSchemaUpdater.Execute();
            }
        }

        public Table<FavoriteEntry> FavoriteEntries;
        public Table<TileEntry> TileEntries;
        public Table<HistoryEntry> HistoryEntries;
        public Table<StopHistoryEntry> StopHistoryEntries;
        public Table<PlanningHistoryEntry> PlanningHistoryEntries;
        public Table<RecentEntry> RecentEntries;
    }
}
