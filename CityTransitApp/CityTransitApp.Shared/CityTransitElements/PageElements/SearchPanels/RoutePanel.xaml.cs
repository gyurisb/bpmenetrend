using CityTransitElements.Effects;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageElements.SearchPanels
{
    public sealed partial class RoutePanel : UserControl
    {
        public RoutePanel()
        {
            InitializeComponent();
            this.SetBinding(BoundDataContextProperty, new Binding());
        }

        public event EventHandler Click;
        public event EventHandler RouteClick;

        public double BorderThickness
        {
            get { return Root.BorderThickness.Top; }
            set
            {
                Root.BorderThickness = new Thickness(0, value, 0, 0);
                NrBorder.BorderThickness = new Thickness(0, 0, value, 0);
            }
        }

        private void Route_Tap(object sender, TappedRoutedEventArgs e)
        {
            Route selected = (Route)(sender as FrameworkElement).DataContext;
            if (RouteClick != null)
                RouteClick(selected, new EventArgs());
        }

        private void Category_Tap(object sender, TappedRoutedEventArgs e)
        {
            RouteGroup RouteGroup = (RouteGroup)DataContext;
            if (Click != null && RouteGroup.Routes.Count > 0)
                Click(RouteGroup, new EventArgs());
        }

        private void SetContent()
        {
            var routeGroup = DataContext as RouteGroup;
            if (routeGroup != null)
            {
                DataTemplate template = (DataTemplate)Resources["RouteTemplate"];
                var routes = routeGroup.Routes;
                int i = 0;
                for (; i < ContentPanel.Children.Count && i < routes.Count; i++)
                {
                    (ContentPanel.Children[i] as FrameworkElement).DataContext = routes[i];
                }
                for (; i < ContentPanel.Children.Count; i++)
                {
                    ContentPanel.Children.RemoveAt(i);
                }
                for (; i < routes.Count; i++)
                {
                    FrameworkElement newElem = (FrameworkElement)template.LoadContent();
                    newElem.DataContext = routes[i];
                    ContentPanel.Children.Add(newElem);
                }
            }
        }

        public static readonly DependencyProperty BoundDataContextProperty = DependencyProperty.Register(
            "BoundDataContext",
            typeof(object),
            typeof(UserControl),
            new PropertyMetadata(null, OnBoundDataContextChanged));

        private static void OnBoundDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoutePanel panel = d as RoutePanel;
            panel.SetContent();
        }
    }
}
