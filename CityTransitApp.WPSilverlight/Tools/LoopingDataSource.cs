using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CityTransitApp.WPSilverlight.Tools
{
    public abstract class LoopingDataSource<T> : ILoopingSelectorDataSource
    {
        protected LoopingDataSource() { selectedItem = default(T); }
        protected LoopingDataSource(T initialSelection) { selectedItem = initialSelection; }

        protected abstract T GetNext(T relativeTo);
        protected abstract T GetPrevious(T relativeTo);

        public object GetNext(object relativeTo)
        {
            if (relativeTo == null) return null;
            return GetNext((T)relativeTo);
        }

        public object GetPrevious(object relativeTo)
        {
            if (relativeTo == null) return null;
            return GetPrevious((T)relativeTo);
        }

        private T selectedItem;
        public object SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                object oldVal = selectedItem;
                selectedItem = (T)value;
                if (SelectionChanged != null)
                    SelectionChanged(this, new SelectionChangedEventArgs(new object[] { oldVal }, new object[] { value }));
            }
        }
        public T Selected
        {
            get { return (T)SelectedItem; }
            set { SelectedItem = value; }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
    }
}
