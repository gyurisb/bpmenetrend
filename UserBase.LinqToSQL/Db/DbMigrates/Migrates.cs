using Microsoft.Phone.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserBase.LinqToSQL
{
    public partial class Migrates
    {
        [Migrate(InitialVersion = 0, TargetVersion = 1)]
        public void AddPositionColumn(DatabaseSchemaUpdater updater)
        {
            //DML update
            updater.AddColumn<FavoriteEntry>("Position");
            updater.DatabaseSchemaVersion = 1;
            updater.Execute();
            //DDL update
            var ctx = (DyanamicDBContext)updater.Context;
            foreach (var fav in ctx.FavoriteEntries)
                fav.Position = fav.EntryID;
            ctx.SubmitChanges();
        }

        [Migrate(InitialVersion = 1, TargetVersion = 2)]
        public void AddTablesHistoryTables(DatabaseSchemaUpdater updater)
        {
            //DML update
            updater.AddTable<StopHistoryEntry>();
            updater.AddTable<PlanningHistoryEntry>();
            updater.AddTable<RecentEntry>();
            updater.DatabaseSchemaVersion = 2;
            updater.Execute();

            //DDL update
            var ctx = (DyanamicDBContext)updater.Context;
            foreach (var entry in ctx.HistoryEntries)
            {
                if (entry.TypeLegacy == HistoryEntry.HistoryEntryTypeLegacy.Stop)
                {
                    entry.TypeIDLegacy = 0;
                    var stopEntry = new StopHistoryEntry { StopID = entry.StopID, Count = entry.Count };
                    ctx.StopHistoryEntries.InsertOnSubmit(stopEntry);
                    ctx.HistoryEntries.DeleteOnSubmit(entry);
                }
                else if (entry.TypeLegacy == HistoryEntry.HistoryEntryTypeLegacy.SourceTarget)
                {
                    entry.TypeIDLegacy = 0;
                    var planningEntry = new PlanningHistoryEntry { StopID = entry.StopID, Count = entry.Count, IsSource = false };
                    var planningEntry2 = new PlanningHistoryEntry { StopID = entry.StopID, Count = entry.Count, IsSource = true };
                    ctx.PlanningHistoryEntries.InsertOnSubmit(planningEntry);
                    ctx.PlanningHistoryEntries.InsertOnSubmit(planningEntry2);
                    ctx.HistoryEntries.DeleteOnSubmit(entry);
                }
                else
                {
                    entry.TypeIDLegacy = 0;
                }
            }
            ctx.SubmitChanges();
        }
    }
}
