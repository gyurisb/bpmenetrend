using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader
{
    public interface IEntityFactory
    {
        String MetaDefinition { get; }
    }

    public abstract class EntityFactory<TEntity> : IEntityFactory where TEntity : IEntity
    {
        public abstract TEntity ReadEntity(Stream stream);

        public abstract String MetaDefinition { get; }
    }
}
