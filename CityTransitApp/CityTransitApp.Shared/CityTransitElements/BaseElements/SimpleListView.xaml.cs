using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.BaseElements
{
    public sealed partial class SimpleListView : UserControl
    {
        private ScrollViewer scrollViewer = null;
        private bool initialized = false;

        public event EventHandler<object> ItemSelected;

        public SimpleListView()
        {
            this.InitializeComponent();
            this.LayoutUpdated += SimpleListView_LayoutUpdated;
        }

        public DataTemplate HeaderTemplate { get; set; }
        public DataTemplate FooterTemplate { get; set; }

        public new Thickness ContentPadding
        {
            get { return ContentPanel.Margin; }
            set { ContentPanel.Margin = value; }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
              "ItemsSource",
              typeof(IList),
              typeof(SimpleListView),
              new PropertyMetadata(null, itemsSourceValueChanged)
          );

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.RegisterAttached(
              "ItemTemplate",
              typeof(DataTemplate),
              typeof(SimpleListView),
              new PropertyMetadata(null)
          );

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached(
              "Oriantation",
              typeof(Orientation),
              typeof(SimpleListView),
              new PropertyMetadata(Orientation.Vertical, orientationChanged)
          );

        private static void orientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleListView)d).setOrientation((Orientation)e.NewValue);
        }

        private static void itemsSourceValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleListView)d).setItemsSource((IList)e.NewValue);
        }

        private void setOrientation(Orientation orientation)
        {
            ContentPanel.Orientation = orientation;
        }

        private async void setItemsSource(IList items)
        {
            if (!initialized)
            {
                if (HeaderTemplate != null)
                    ContentPanel.Children.Add((FrameworkElement)HeaderTemplate.LoadContent());
                if (FooterTemplate != null)
                    ContentPanel.Children.Add((FrameworkElement)FooterTemplate.LoadContent());
                initialized = true;
            }

            if (items == null)
                items = new object[0];
            int headerCount = HeaderTemplate != null ? 1 : 0;
            int footerCount = FooterTemplate != null ? 1 : 0;
            int itemsCount = ContentPanel.Children.Count - headerCount - footerCount;
            int changeableCount = Math.Min(itemsCount, items.Count);
            int addableCount = items.Count - changeableCount;
            int removableCount = itemsCount - changeableCount;
            var itemTemplate = this.ItemTemplate;

            for (int i = 0; i < changeableCount; i++)
            {
                ((FrameworkElement)(((Border)ContentPanel.Children[headerCount + i]).Child)).DataContext = items[i];
            }
            for (int i = 0; i < addableCount; i++)
            {
                object newItem = items[changeableCount + i];
                FrameworkElement newElement = (FrameworkElement)itemTemplate.LoadContent();
                newElement.DataContext = newItem;
                var newElementContainer = new Border { Child = newElement, Background = new SolidColorBrush(Colors.Transparent) };
                newElementContainer.Tapped += elementContainer_Tapped;
                ContentPanel.Children.Insert(ContentPanel.Children.Count - footerCount, newElementContainer);
            }
            for (int i = removableCount - 1; i >= 0; i--)
            {
                ContentPanel.Children.RemoveAt(headerCount + changeableCount + i);
            }
        }

        void SimpleListView_LayoutUpdated(object sender, object e)
        {
            if (ContentPanel.ActualHeight > LayoutRoot.ActualHeight)
            {
                if (scrollViewer == null)
                {
                    scrollViewer = new ScrollViewer();
                    LayoutRoot.Children.Add(scrollViewer);
                    LayoutRoot.Children.Remove(ContentPanel);
                    scrollViewer.Content = ContentPanel;
                }
            }
            else
            {
                if (scrollViewer != null)
                {
                    scrollViewer.Content = null;
                    LayoutRoot.Children.Remove(scrollViewer);
                    LayoutRoot.Children.Add(ContentPanel);
                    scrollViewer = null;
                }
            }
        }

        void elementContainer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemSelected != null)
                ItemSelected(this, ((FrameworkElement)((Border)sender).Child).DataContext);
        }

        public IList ItemsSource
        {
            get { return (IList)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }
        public Orientation Orientation
        {
            get { return (Orientation)this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        public async Task ScrollIntoView(object item)
        {
            await Task.Delay(200);
            if (ItemsSource.Count == 0)
                return;
            if (scrollViewer != null)
            {
                int headerCount = HeaderTemplate != null ? 1 : 0;
                int index = ItemsSource.IndexOf(item);
                double targetOffset = ContentPanel.Children.Take(index + headerCount).Sum(x => ((FrameworkElement)x).ActualHeight);
                targetOffset = Math.Min(targetOffset, ContentPanel.ActualHeight - LayoutRoot.ActualHeight);
                int tries = 10;
                while (Math.Abs(scrollViewer.VerticalOffset - targetOffset) >= 1.0 && tries-- >= 0)
                {
                    scrollViewer.ChangeView(null, targetOffset, null, true);
                    await Task.Delay(10);
                }
            }
        }
    }
}
