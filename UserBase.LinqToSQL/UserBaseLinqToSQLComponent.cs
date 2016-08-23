using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UserBase.Interface;
using UserBase.LinqToSQL;

namespace UserBase
{
    public class UserBaseLinqToSQLComponent : IUserBase
    {
        private DyanamicDBContext context;

        public UserBaseLinqToSQLComponent(string connectionString, int currentDDBVersion, bool forbidMigration = false)
        {
            context = new DyanamicDBContext(connectionString, currentDDBVersion, forbidMigration);
            Favorites = new Favorites(context);
            History = new History(context);
            TileRegister = new TileRegister(context);

            //Data Source=isostore:ddb.sdf
            var connectionFields = connectionString.Split(';').ToDictionary(x => x.Split('=').First(), x => x.Split('=').Last());
            string dataSource = connectionFields["Data Source"];
            FileName = dataSource.Replace("isostore:", "");
        }

        public IFavorites Favorites { get; private set; }
        public IHistory History { get; private set; }
        public ITileRegister TileRegister { get; private set; }

        public string FileName { get; private set; }

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
