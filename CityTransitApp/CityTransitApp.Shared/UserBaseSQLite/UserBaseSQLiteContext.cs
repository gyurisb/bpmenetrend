using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserBase.Entities;

namespace UserBase
{
    public class UserBaseSQLiteContext : SQLiteConnection
    {
        private SemaphoreSlim contextLock = new SemaphoreSlim(1);

        public UserBaseSQLiteContext(string databasePath) : base(databasePath) 
        {
            lock (this)
            {
                CreateTable<FavoriteEntry>();
                CreateTable<TileEntry>();
                CreateTable<HistoryEntry>();
                CreateTable<StopHistoryEntry>();
                CreateTable<PlanningHistoryEntry>();
                CreateTable<RecentEntry>();
            }
        }

        public async Task<LockObj> Lock()
        {
            await contextLock.WaitAsync();
            return new LockObj { Lock = contextLock };
        }
        public class LockObj : IDisposable
        {
            public SemaphoreSlim Lock;
            public void Dispose()
            {
                Lock.Release();
            }
        }
        public void DeleteAll<T>(IEnumerable<T> removable)
        {
            foreach (var entry in removable)
                Delete<T>(removable);
        }

        public TableQuery<FavoriteEntry> FavoriteEntries { get { return Table<FavoriteEntry>(); } }
        //public TableQuery<TileEntry> TileEntries { get { return Table<TileEntry>(); } }
        public TableQuery<HistoryEntry> HistoryEntries { get { return Table<HistoryEntry>(); } }
        public TableQuery<StopHistoryEntry> StopHistoryEntries { get { return Table<StopHistoryEntry>(); } }
        public TableQuery<PlanningHistoryEntry> PlanningHistoryEntries { get { return Table<PlanningHistoryEntry>(); } }
        public TableQuery<RecentEntry> RecentEntries { get { return Table<RecentEntry>(); } }
    }
}
