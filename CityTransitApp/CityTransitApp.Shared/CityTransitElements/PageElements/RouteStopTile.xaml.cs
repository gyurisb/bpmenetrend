using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CityTransitApp;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitCommon.Elements
{
    public sealed partial class RouteStopTile : UserControl
    {
        public RouteStopTile()
        {
            this.InitializeComponent();
        }

        public string RouteShortName { set { RouteText.Text = value; } }
        public string RouteDirName { set { DirText.Text = " | " + value; } }
        public string StopName { set { StopText.Text = value; } }

        //public int Hour1 { set { Hour1Text.Text = String.Format("{0}", value); } }
        //public int Hour2 { set { Hour2Text.Text = String.Format("{0}", value); } }

        //public string[] TimeSource1 { set { addItemSource(TimeList1, value); } }
        //public string[] TimeSource2 { set { addItemSource(TimeList2, value); } }

        private static readonly int rowLength = 291 / 50;


        public void SetItemSource(int[] hours, int[][] itemSource)
        {
            Grid contentGrid = new Grid();
            Grid.SetRow(contentGrid, 2);
            Grid.SetColumnSpan(contentGrid, 2);
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < hours.Length && i < 5; i++)
            {
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Border border = new Border
                {
                    Style = (Style)Resources["HourBorderStyle"],
                    Child = new TextBlock
                    {
                        Text = hours[i].ToString(),
                        Style = (Style)Resources["HourTextStyle"]
                    }
                };
                Grid.SetColumn(border, 0);
                Grid.SetRow(border, i);
                contentGrid.Children.Add(border);

                StackPanel panel = new StackPanel();
                addItemSource(panel, itemSource[i]);
                Grid.SetColumn(panel, 1);
                Grid.SetRow(panel, i);
                contentGrid.Children.Add(panel);
            }
            LayoutRoot.Children.Add(contentGrid);
        }

        private static void addItemSource(StackPanel root, int[] itemSource, double scale = 1.0)
        {
            for (int i = 0; i < Math.Ceiling((double)itemSource.Length / rowLength); i++)
            {
                StackPanel row = new StackPanel { Orientation = Orientation.Horizontal };
                for (int k = 0; k < rowLength && i * rowLength + k < itemSource.Length; k++)
                {
                    Border border = new Border { Width = 25*scale, Height = 20*scale };
                    border.Child = new TextBlock { Text = itemSource[i * rowLength + k].ToString(), FontSize = 15*scale, HorizontalAlignment = HorizontalAlignment.Center };
                    row.Children.Add(border);
                }
                root.Children.Add(row);
            }
        }

        public Grid FrameRoot { get { return LayoutRoot; } }

        public static async Task<FrameworkElement> CreateRouteStopTile(string routeShortName, string routeDirName, string stopName, int[] hours, int[][] itemSource)
        {
            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/CustomTile.xml"));
            string xmlData;
            using (var stream = await storageFile.OpenReadAsync())
            {
                var reader = new StreamReader(stream.AsStream());
                xmlData = await reader.ReadToEndAsync();
            }
            Border root = (Border)XamlReader.Load(xmlData);
            TextBlock routeText = (TextBlock)root.FindName("RouteText");
            TextBlock dirText = (TextBlock)root.FindName("DirText");
            TextBlock stopText = (TextBlock)root.FindName("StopText");
            Grid layoutRoot = (Grid)root.FindName("LayoutRoot");

            routeText.Text = routeShortName;
            dirText.Text = " | " + routeDirName;
            stopText.Text = stopName;


            Grid contentGrid = new Grid();
            Grid.SetRow(contentGrid, 2);
            Grid.SetColumnSpan(contentGrid, 2);
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < hours.Length && i < 5; i++)
            {
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Border border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(0x3F, 0, 0, 0)), //#3F000000
                    Child = new TextBlock
                    {
                        Text = hours[i].ToString(),
                        Margin = new Thickness(2,0,5,0),
                        FontSize = 36
                    }
                };
                Grid.SetColumn(border, 0);
                Grid.SetRow(border, i);
                contentGrid.Children.Add(border);

                StackPanel panel = new StackPanel();
                addItemSource(panel, itemSource[i], 2.4);
                Grid.SetColumn(panel, 1);
                Grid.SetRow(panel, i);
                contentGrid.Children.Add(panel);
            }
            layoutRoot.Children.Add(contentGrid);

            foreach (var textBlock in root.FindVisualChildren<TextBlock>())
                textBlock.Foreground = new SolidColorBrush(Colors.White);
            return root;
        }
    }
}
