using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using System.Threading.Tasks;

namespace CityTransitApp
{
    public static class ElementExtensions
    {
        public static IList ItemsSource(this ListView list) { return (IList)list.ItemsSource; }
        public static IList<T> ItemsSource<T>(this ListView list) { return (IList<T>)list.ItemsSource; }
        //public static IList GroupedItemsSource(this ListView list) { return (IList)(((CollectionViewSource)list.ItemsSource).Source); }

        public static ListViewPositionResult GetScrollPosition(this ListView list)
        {
            var scrollViewer = list.FindVisualChildren<ScrollViewer>().FirstOrDefault();
            if (scrollViewer == null)
                return new ListViewPositionResult { Item = list.ItemsSource().Cast<object>().FirstOrDefault(), VerticalOffset = 0 };

            double offset = scrollViewer.VerticalOffset;

            for (int i = 0; i < list.ItemsSource().Count; i++)
            {
                var listViewItem = list.ContainerFromIndex(i) as UIElement;
                if (listViewItem == null)
                    break;
                double top = listViewItem.TransformToVisual(list).TransformPoint(new Point()).Y;
                if (top > offset)
                    return new ListViewPositionResult { Item = list.ItemsSource()[Math.Max(0, i - 1)], VerticalOffset = offset };
            }

            return new ListViewPositionResult { Item = list.ItemsSource().Cast<object>().LastOrDefault(), VerticalOffset = offset };
        }

        public static ListViewPositionResult GetGroupedScrollPosition(this ListView list, IEnumerable groupedItemsSource)
        {
            var itemsSource = groupedItemsSource.Cast<IEnumerable>().SelectMany(x => x.Cast<object>()).Where(x => !(x is IEnumerable)).ToList();

            var scrollViewer = list.FindVisualChildren<ScrollViewer>().FirstOrDefault();
            if (scrollViewer == null)
                return new ListViewPositionResult { Item = itemsSource.FirstOrDefault(), VerticalOffset = 0 };

            double offset = scrollViewer.VerticalOffset;

            for (int i = 0; i < itemsSource.Count; i++)
            {
                var listViewItem = list.ContainerFromItem(itemsSource[i]) as UIElement;
                if (listViewItem != null)
                {
                    double top = listViewItem.TransformToVisual(scrollViewer.Content as UIElement).TransformPoint(new Point()).Y;
                    if (top > offset)
                        return new ListViewPositionResult { Item = itemsSource[i], VerticalOffset = offset };
                }
            }

            return new ListViewPositionResult { Item = itemsSource.LastOrDefault(), VerticalOffset = scrollViewer.ScrollableHeight - scrollViewer.ActualHeight };
        }

        public static async void ScrollToGroupedPosition(this ListView list, ListViewPositionResult result)
        {
            while (list.ActualHeight == 0.0)
                await Task.Delay(25);
            var scrollViewer = list.FindVisualChildren<ScrollViewer>().FirstOrDefault();
            if (scrollViewer == null)
                return;

            //while (list.ContainerFromItem(result.Item) == null)
            while (scrollViewer.ScrollableHeight < result.VerticalOffset)
            {
                list.ScrollIntoView(result.Item);
                await Task.Delay(25);
            }

            while (Math.Abs(scrollViewer.VerticalOffset - result.VerticalOffset) > 0.5)
            {
                scrollViewer.ChangeView(null, result.VerticalOffset, null, true);
                await Task.Delay(25);
            }
        }

        public static async Task ForceScrollIntoView(this ListView list, object item)
        {
            var itemsSource = list.ItemsSource().Cast<object>().ToList();
            while (list.ActualHeight == 0.0)
                await Task.Delay(25);
            var scrollViewer = list.FindVisualChildren<ScrollViewer>().FirstOrDefault();
            if (scrollViewer == null)
                return;

            list.ScrollIntoView(item);
            while (list.ContainerFromItem(item) == null)
            {
                list.ScrollIntoView(item);
                await Task.Delay(1);
            }

            //var current = (FrameworkElement)list.ContainerFromItem(item);
            var first = (FrameworkElement)list.ContainerFromItem(itemsSource.First());
            double maxOffset = (scrollViewer.Content as FrameworkElement).ActualHeight - scrollViewer.ActualHeight;

            double offset = first.ActualHeight * itemsSource.IndexOf(item);
            offset = Math.Min(maxOffset, offset);
            while (Math.Abs(scrollViewer.VerticalOffset - offset) > 0.5)
            {
                scrollViewer.ChangeView(null, offset, null, true);
                await Task.Delay(25);
            }
            await Task.Delay(25);
            scrollViewer.ChangeView(null, offset, null, true);
            await Task.Delay(25);
            scrollViewer.ChangeView(null, offset, null, true);
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static T FirstVisualParent<T>(this FrameworkElement element) where T : FrameworkElement
        {
            while (element != null && !(element is T))
            {
                element = (FrameworkElement)element.Parent;
            }
            return (T)element;
        }

        public static FrameworkElement FirstVisualParent(this FrameworkElement element, Type parentType)
        {
            while (element != null && element.GetType() != parentType)
            {
                element = (FrameworkElement)element.Parent;
            }
            return element;
        }

        /*
         * Automata font size: Viewbox
         * 
         * Font size conversions
         * factor ~0.85
         * 69 => 58
         * 42 => 36
         * 30 => 25
         * 27 => 23
         * 25 => 21
         * 24 => 20
         * 22 => 19
         * 20 => 17
         * 19 => 16
         * 15 => 13
         * 14 => 12
        */
    }

    public class ListViewPositionResult
    {
        public object Item;
        public double VerticalOffset;
    }
}
