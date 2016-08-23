using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace CityTransitApp.WPSilverlight.PageElements
{
    public partial class RouteStopTile : UserControl
    {
        public RouteStopTile()
        {
            InitializeComponent();
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

        private void addItemSource(StackPanel root, int[] itemSource)
        {
            for (int i = 0; i < Math.Ceiling((double)itemSource.Length / rowLength); i++)
            {
                StackPanel row = new StackPanel { Orientation = Orientation.Horizontal };
                for (int k = 0; k < rowLength && i * rowLength + k < itemSource.Length; k++)
                {
                    Border border = new Border { Width = 50, Height = 50 };
                    border.Child = new TextBlock { Text = itemSource[i * rowLength + k].ToString(), FontSize = 30, HorizontalAlignment = HorizontalAlignment.Center };
                    row.Children.Add(border);
                }
                root.Children.Add(row);
            }
        }

        public Grid FrameRoot { get { return LayoutRoot; } }
    }
}
