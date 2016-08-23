using System;
using System.Collections.Generic;
using System.Text;
using UserBase;
using UserBase.Backup;
using UserBase.BusinessLogic;
using UserBase.Interface;

namespace UserBase
{
    public class UserBaseSQLiteComponent : IUserBase
    {
        public IFavorites Favorites { get; private set; }
        public IHistory History { get; private set; }
        public ITileRegister TileRegister { get { throw new NotImplementedException("TileRegister is not implemented in SQLite database."); } }

        public string FileName { get; private set; }

        private UserBaseSQLiteContext context;

        public UserBaseSQLiteComponent(string databasePath)
        {
            context = new UserBaseSQLiteContext(databasePath);
            Favorites = new Favorites(context);
            History = new History(context);
            //TileRegister = new TileRegister(context);

            FileName = System.IO.Path.GetFileName(databasePath);
        }

        public IDBBackup CreateBackup()
        {
            return DbBackup.Import(context);
        }

        public bool HasAnyData()
        {
            return context.FavoriteEntries.FirstOrDefault() != null ||
                context.HistoryEntries.FirstOrDefault() != null ||
                context.PlanningHistoryEntries.FirstOrDefault() != null ||
                context.StopHistoryEntries.FirstOrDefault() != null ||
                context.RecentEntries.FirstOrDefault() != null;
        }

        public void Dispose()
        {
            if (context != null)
            {
                lock (context)
                {
                    context.Dispose();
                }
            }
        }
    }
}
