using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.Common;
using TransitBase.Entities;
using CityTransitApp.WPSilverlight.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace CityTransitApp.WPSilverlight.PageElements.SearchElements
{
    public partial class CategoryTreeSelector : UserControl
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
                    panel.Tap += (sender, args) =>
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
                    panel.Tap += (sender, args) =>
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
                var panel = new StackPanel { Margin = new Thickness(5, 0, 5, 0) };
                //panel.SetValue(TiltEffectCustom.IsTiltEnabledProperty, true);
                panel.SetValue(RotateEffect.IsEnabledProperty, true);
                panel.Children.Add(new Image
                {
                    Source = new BitmapImage(category.Icon),
                    Height = 60,
                    Width = 60,
                    Margin = new Thickness(5, 20, 5, 5),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                });
                panel.Children.Add(new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = category.Name,
                    MaxWidth = 100,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    FontSize = 16,
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
                        TextWrapping = System.Windows.TextWrapping.Wrap,
                        FontSize = 20,
                        Margin = new Thickness(7),
                        Foreground = (Brush)App.Current.Resources["AppForegroundBrush"],
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    MinWidth = 80,
                    MaxWidth = 120,
                    MinHeight = 60,
                    Margin = new Thickness(5),
                    BorderBrush = (Brush)App.Current.Resources["AppBorderBrush"],
                    BorderThickness = new Thickness(2),
                    Background = (Brush)App.Current.Resources["AppBackgroundBrush"],
                    VerticalAlignment = System.Windows.VerticalAlignment.Stretch
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
