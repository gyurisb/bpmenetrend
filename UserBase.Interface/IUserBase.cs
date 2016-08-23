using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserBase.Interface
{
    public interface IUserBase : IDisposable
    {
        IFavorites Favorites { get; }
        IHistory History { get; }
        ITileRegister TileRegister { get; }

        string FileName { get; }

        IDBBackup CreateBackup();
        bool HasAnyData();
    }
}
