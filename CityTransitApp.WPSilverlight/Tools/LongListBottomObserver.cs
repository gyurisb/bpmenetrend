using CityTransitServices.Tools;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CityTransitApp.WPSilverlight.Tools
{
    public class LongListBottomObserver : IDisposable
    {
        private PeriodicTask pollingTask;
        private Dictionary<object, ContentPresenter> items = new Dictionary<object, ContentPresenter>();
        private bool isAtBottom = false;
        private ViewportControl viewPort = null;
        private LongListSelector longList;

        public event EventHandler ScrollToBottom;

        public LongListBottomObserver(LongListSelector longList)
        {
            this.longList = longList;
            longList.ItemRealized += LLS_ItemRealized;
            longList.ItemUnrealized += LLS_ItemUnrealized;
            pollingTask = new PeriodicTask(500, checkIfBottom);
            pollingTask.Run();
        }

        public void Dispose()
        {
            pollingTask.Cancel();
            longList.ItemRealized -= LLS_ItemRealized;
            longList.ItemUnrealized -= LLS_ItemUnrealized;
            items.Clear();
        }

        private void checkIfBottom()
        {
            if (viewPort == null)
            {
                viewPort = FindViewport(longList);
                if (viewPort == null)
                    return;
            }
            if (!HasItemsAfter(viewPort))
            {
                if (!isAtBottom && ScrollToBottom != null)
                    ScrollToBottom(longList, new EventArgs());
                isAtBottom = true;
            }
            else
            {
                isAtBottom = false;
            }
        }

        public bool IsAtBottom()
        {
            if (viewPort == null)
            {
                viewPort = FindViewport(longList);
                if (viewPort == null)
                    return false;
            }
            return !HasItemsAfter(viewPort);
        }


        private bool HasItemsAfter(ViewportControl viewPort)
        {
            var offset = viewPort.Viewport.Bottom;
            return items.Where(x => Canvas.GetTop(x.Value) + x.Value.ActualHeight > offset).Count() > 0;
            //if (itemsAfter.Count > 0)
            //    return itemsAfter.MinBy(x => Canvas.GetTop(x.Value)).Key;
        }

        private void LLS_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                object o = e.Container.DataContext;
                items[o] = e.Container;
            }
        }

        private void LLS_ItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                object o = e.Container.DataContext;
                items.Remove(o);
            }
        }

        private static ViewportControl FindViewport(DependencyObject parent)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childCount; i++)
            {
                var elt = VisualTreeHelper.GetChild(parent, i);
                if (elt is ViewportControl) return (ViewportControl)elt;
                var result = FindViewport(elt);
                if (result != null) return result;
            }
            return null;
        }
    }
}
