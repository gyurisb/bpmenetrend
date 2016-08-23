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
    public enum ItemAlignmentMode { FixedVertical, FixedHorizontal };

    public sealed partial class SimpleGridView : UserControl
    {
        public event EventHandler<object> ItemSelected;

        private double rootHeight, rootWidth;
        private Task<double> rootHeightTask;
        private Task<double> rootWidthTask;

        public SimpleGridView()
        {
            this.InitializeComponent();
            this.rootHeightTask = new Task<double>(() => rootHeight);
            this.rootWidthTask = new Task<double>(() => rootWidth);
            LayoutRoot.LayoutUpdated += LayoutRoot_LayoutUpdated;
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
              "ItemsSource",
              typeof(IList),
              typeof(SimpleGridView),
              new PropertyMetadata(null, itemsSourceValueChanged)
          );

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.RegisterAttached(
              "ItemTemplate",
              typeof(DataTemplate),
              typeof(SimpleGridView),
              new PropertyMetadata(null)
          );

        public static readonly DependencyProperty ItemAlignmentModeProperty = DependencyProperty.RegisterAttached(
              "ItemAlignmentMode",
              typeof(ItemAlignmentMode),
              typeof(SimpleGridView),
              new PropertyMetadata(ItemAlignmentMode.FixedVertical)
          );

        private static void itemsSourceValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleGridView)d).setItemsSource((IList)e.NewValue);
        }

        private async void setItemsSource(IList items)
        {
            if (items == null)
                items = new object[0];
            //int changeableCount = Math.Min(ContentPanel.Children.Count, items.Count);
            //int addableCount = items.Count - changeableCount;
            //int removableCount = ContentPanel.Children.Count - changeableCount;
            //var itemTemplate = this.ItemTemplate;

            //for (int i = 0; i < changeableCount; i++)
            //{
            //    ((FrameworkElement)(((Border)ContentPanel.Children[i]).Child)).DataContext = items[i];
            //}
            //for (int i = 0; i < addableCount; i++)
            //{
            //    object newItem = items[changeableCount + i];
            //    FrameworkElement newElement = (FrameworkElement)itemTemplate.LoadContent();
            //    newElement.DataContext = newItem;
            //    var newElementContainer = new Border { Child = newElement, Background = new SolidColorBrush(Colors.Transparent) };
            //    newElementContainer.Tapped += elementContainer_Tapped;
            //    ContentPanel.Children.Add(newElementContainer);
            //}
            //for (int i = removableCount - 1; i >= 0; i--)
            //{
            //    ContentPanel.Children.RemoveAt(changeableCount + i);
            //}

            ContentPanel.Children.Clear();
            var itemTemplate = this.ItemTemplate;

            int rowCount, columnCount;
            if (ItemAlignmentMode == ItemAlignmentMode.FixedVertical)
            {
                rowCount = (int)Math.Floor(await rootHeightTask / elementHeight((FrameworkElement)itemTemplate.LoadContent()));
                columnCount = (int)Math.Ceiling((double)items.Count / rowCount);
            }
            else
            {
                columnCount = (int)Math.Floor(await rootWidthTask / elementWidth((FrameworkElement)itemTemplate.LoadContent()));
                rowCount = (int)Math.Ceiling((double)items.Count / columnCount);
            }

            for (int i = 0; i < rowCount; i++)
            {
                ContentPanel.Children.Add(new StackPanel { Orientation = Orientation.Horizontal });
            }
            for (int i = 0; i < rowCount; i++)
            {
                StackPanel rowPanel = (StackPanel)ContentPanel.Children[i];
                for (int k = 0; k < columnCount && i * columnCount + k < items.Count; k++)
                {
                    var newItem = items[i * columnCount + k];
                    FrameworkElement newElement = (FrameworkElement)itemTemplate.LoadContent();
                    if (rowCount == -1)
                    {
                        rowCount = (int)Math.Floor(await rootHeightTask / newElement.Height);
                    }
                    newElement.DataContext = newItem;
                    var newElementContainer = new Border { Child = newElement, Background = new SolidColorBrush(Colors.Transparent) };
                    newElementContainer.Tapped += elementContainer_Tapped;
                    rowPanel.Children.Add(newElementContainer);
                }
            }
        }

        private double elementHeight(FrameworkElement element)
        {
            return element.Height + element.Margin.Top + element.Margin.Bottom;
        }
        private double elementWidth(FrameworkElement element)
        {
            return element.Width + element.Margin.Left + element.Margin.Right;
        }

        void LayoutRoot_LayoutUpdated(object sender, object e)
        {
            if (ItemAlignmentMode == ItemAlignmentMode.FixedVertical)
            {
                if (rootHeight == 0 && this.ActualHeight != 0)
                {
                    this.rootHeight = this.ActualHeight;
                    this.rootHeightTask.Start();
                }
            }
            else
            {
                if (rootWidth == 0 && this.ActualWidth != 0)
                {
                    this.rootWidth = this.ActualWidth;
                    this.rootWidthTask.Start();
                }
            }
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
        public ItemAlignmentMode ItemAlignmentMode
        {
            get { return (ItemAlignmentMode)this.GetValue(ItemAlignmentModeProperty); }
            set { this.SetValue(ItemAlignmentModeProperty, value); }
        }

        void elementContainer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemSelected != null)
                ItemSelected(this, ((FrameworkElement)((Border)sender).Child).DataContext);
        }
    }
}
