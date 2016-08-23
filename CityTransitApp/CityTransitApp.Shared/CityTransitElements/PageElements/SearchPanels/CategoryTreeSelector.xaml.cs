using CityTransitApp.Common;
using CityTransitElements.Effects;
using CityTransitServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageElements.SearchPanels
{
    public sealed partial class CategoryTreeSelector : UserControl
    {
        public CategoryTreeSelector()
        {
            InitializeComponent();
        }

        public event EventHandler<IEnumerable<RouteGroup>> SelectionChanged;

        public CategoryTree TreeModel
        {
            set
            {
                var adapter = new CategoryTreeAdapter(value);
                createRow(0, adapter.TopCategories);
            }
        }

        private void createRow(int level, IList<CategoryTreeNode> categories)
        {
            StackPanel container;
            if (level == 0)
            {
                //container = TopContainer;
                container = null;
            }
            else
            {
                clearRows(level);
                container = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    HorizontalAlignment = HorizontalAlignment.Center 
                };
                LayoutRoot.Children.Add(new ScrollViewer 
                {
                    Content = container, 
                    MaxWidth = LayoutRoot.ActualWidth, 
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                });
            }

            foreach (var category in categories)
            {
                UIElement panel = createNode(category);
                if (!category.HasChildren)
                {
                    panel.Tapped += (sender, args) =>
                    {
                        clearRows(level + 1);
                        if (SelectionChanged != null)
                        {
                            SelectionChanged(this, category.Values);
                        }
                    };
                }
                else
                {
                    panel.Tapped += (sender, args) =>
                    {
                        createRow(level + 1, category.Children);
                    };
                }
                if (container == null)
                    TopContainer.Children.Add(panel);
                else
                    container.Children.Add(panel);
            }
        }

        private UIElement createNode(CategoryTreeNode category)
        {
            if (category.Icon != null)
            {
                var panel = new StackPanel { Margin = new Thickness(4, 0, 4, 0) };
                //panel.SetValue(TiltEffectCustom.IsTiltEnabledProperty, true);
                panel.SetValue(RotateEffect.IsEnabledProperty, true);
                panel.Children.Add(new Image
                {
                    Source = new BitmapImage(category.Icon),
                    Height = 51,
                    Width = 51,
                    Margin = new Thickness(4, 17, 4, 4),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                panel.Children.Add(new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = category.Name,
                    MaxWidth = 85,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 14,
                    Foreground = (Brush)App.Current.Resources["AppForegroundBrush"]
                });
                return panel;
            }
            else
            {
                var border = new Border
                {
                    Child = new TextBlock
                    {
                        Text = category.Name,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 17,
                        Margin = new Thickness(6),
                        Foreground = (Brush)App.Current.Resources["AppForegroundBrush"],
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    MinWidth = 68,
                    MaxWidth = 102,
                    MinHeight = 51,
                    Margin = new Thickness(4),
                    BorderBrush = (Brush)App.Current.Resources["AppBorderBrush"],
                    BorderThickness = new Thickness(2),
                    Background = (Brush)App.Current.Resources["AppBackgroundBrush"],
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                border.SetValue(RotateEffect.IsEnabledProperty, true);
                return border;
            }
        }

        private void clearRows(int level)
        {
            for (int i = LayoutRoot.Children.Count - 1; i >= level; i--)
                LayoutRoot.Children.RemoveAt(i);
        }
    }
}
