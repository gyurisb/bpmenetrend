using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Attributes
{
    public interface IColumnAttribute
    {
        bool OrderBy { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Column : Attribute, IColumnAttribute
    {
        public bool OrderBy { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class HiddenColumn : Attribute, IColumnAttribute
    {
        public bool OrderBy { get; set; }
    }
}

