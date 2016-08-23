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
        public static void Execute(DatabaseSchemaUpdater updater, int initialVersion, int targetVersion)
        {
            while (true)
            {
                var migrates = new Migrates();
                var method = migrates.GetType().GetMethods().FirstOrDefault(m => Migrate.MigrationEnabled(m, initialVersion));
                if (method != null)
                {
                    method.Invoke(migrates, new object[] { updater });
                    initialVersion = Migrate.Get(method).TargetVersion;
                    if (initialVersion == targetVersion)
                        break;
                }
                else break;
            }

            if (updater.DatabaseSchemaVersion != targetVersion)
            {
                throw new NotSupportedException("Valid dynamic db version update migration failed.");
            }
        }
    }
}
