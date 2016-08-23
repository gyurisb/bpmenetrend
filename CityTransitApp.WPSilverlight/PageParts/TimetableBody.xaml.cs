using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using CityTransitApp.WPSilverlight.Effects;
using TransitBase.Entities;
using System.Threading.Tasks;
using TransitBase;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels.Settings;

namespace CityTransitApp.WPSilverlight.PageParts
{
    public partial class TimetableBody : UserControl
    {
        private static readonly double BLOCK_SIZE = 60.0;
        private static readonly double BODY_FONT = 25.0;
        private static readonly double LABEL_FONT = 20.0;
        private static readonly double LABEL_HEIGHT = 30.0;
        private static readonly double MARGIN_XLARGE = 12.0;
        private static readonly double MARGIN_LARGE = 10.0;
        private static readonly double MARGIN_SMALL = 5.0;

        private bool underlineWheelchair;
        private DateTime date = DateTime.Today;

        public TimetableBody()
        {
            InitializeComponent();
            underlineWheelchair = SettingsModel.WheelchairUnderlined;
        }

        public Trip Selected { get; private set; }
        public DateTime SelectedTime { get; private set; }

        public event EventHandler<SelectionChangedEventArgs> ItemTapped;

        private double targetHeight;

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.RegisterAttached(
              "DataSource",
              typeof(TimeTableBodySource),
              typeof(TimetableBody),
              new PropertyMetadata(null, dataSourceValueChanged)
          );

        private static void dataSourceValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeTableBodySource value = e.NewValue as TimeTableBodySource;
            TimetableBody element = (TimetableBody)d;
            if (value == null)
                element.Clear();
            else element.SetHourList((PhoneApplicationPage)App.RootFrame.Content, value.HourList, value.ScrollTarget);
        }

        public TimeTableBodySource DataSource
        {
            get { return (TimeTableBodySource)this.GetValue(DataSourceProperty); }
            set { this.SetValue(DataSourceProperty, value); }
        }

        public async void SetHourList(PhoneApplicationPage page, IList<TimeTableBodyHourGroup> hourList, TimeTableBodyHourGroup scrollTarget)
        {
            LayoutPanel.Children.Clear();

            //néha az ActualWidth property nem áll be időben, ekkor meg kell várni
            while (page.ActualWidth == 0)
                await Task.Delay(10);

            int columnCount = (int)Math.Floor((page.ActualWidth - (BLOCK_SIZE + MARGIN_XLARGE)) / BLOCK_SIZE);
            double columnWidth = (page.ActualWidth - (BLOCK_SIZE + MARGIN_XLARGE)) / (double)columnCount;
            double currentHeight = 0;
            targetHeight = -1;

            foreach (var hour in hourList)
            {
                StackPanel hourPanel = new StackPanel { Orientation = Orientation.Horizontal, Background = (Brush)hour.BackgroundColor };


                if (hour == scrollTarget)
                    targetHeight = currentHeight;

                if (hour is TimeTableBodyLabelGroup)
                {
                    hourPanel.Children.Add(new TextBlock
                    {
                        Text = (hour as TimeTableBodyLabelGroup).Label,
                        Height = LABEL_HEIGHT,
                        FontSize = LABEL_FONT,
                        Foreground = (Brush)App.Current.Resources["AppPreviousForegroundBrush"],
                        VerticalAlignment = VerticalAlignment.Bottom
                    });
                    currentHeight += LABEL_HEIGHT;
                }
                else
                {
                    DateTime time = date + TimeSpan.FromHours(hour.Hour);
                    hourPanel.Children.Add(new Border
                    {
                        Background = (Brush)App.Current.Resources["AppHeaderBackgroundBrush"],
                        Margin = new Thickness(0, 0, MARGIN_XLARGE, 0),
                        Width = BLOCK_SIZE,
                        Child = new TextBlock
                        {
                            Text = time.HourString(),
                            FontWeight = time.IsPM() ? FontWeights.Bold : FontWeights.Normal,
                            FontSize = BODY_FONT,
                            Margin = new Thickness(MARGIN_LARGE, MARGIN_SMALL, MARGIN_LARGE, MARGIN_SMALL),
                            Foreground = (Brush)App.Current.Resources["AppForegroundBrush"]
                        }
                    });

                    int rowCount = (int)Math.Ceiling(hour.Trips.Count / (double)columnCount);
                    StackPanel rightPanel = new StackPanel { Orientation = Orientation.Vertical };
                    for (int i = 0; i < rowCount; i++)
                    {
                        StackPanel minPanel = new StackPanel { Orientation = Orientation.Horizontal };

                        for (int k = 0; k < columnCount && i * columnCount + k < hour.Trips.Count; k++)
                        {
                            var trip = hour.Trips[i * columnCount + k];
                            var text = new TextBlock
                            {
                                Text = trip.TimeMin.ToString(),
                                FontSize = BODY_FONT,
                                Foreground = (Brush)trip.TextColor,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                TextDecorations = (underlineWheelchair && (trip.Trip.WheelchairAccessible ?? false)) ? TextDecorations.Underline : null
                            };
                            var control = new Border
                            {
                                Width = columnWidth,
                                Height = BLOCK_SIZE,
                                Background = (Brush)trip.BorderColor,
                                Child = text,
                                Tag = Tuple.Create(trip.Trip, trip.Time)
                            };
                            control.SetValue(RotateEffect.IsEnabledProperty, true);
                            control.Tap += control_Tap;

                            minPanel.Children.Add(control);
                        }

                        rightPanel.Children.Add(minPanel);
                        currentHeight += BLOCK_SIZE;
                    }
                    if (rightPanel.Children.Count == 0 || rightPanel.Children.Any(e => (e as StackPanel).Children.Count == 0))
                        throw new InvalidOperationException("Timetable item addition failed!");

                    hourPanel.Children.Add(rightPanel);
                }

                LayoutPanel.Children.Add(hourPanel);
            }

            LayoutPanel.LayoutUpdated += LayoutRoot_LayoutUpdated;
        }

        private void LayoutRoot_LayoutUpdated(object sender, object e)
        {
            if (targetHeight != -1)
                ScrollViewer.ScrollToVerticalOffset(targetHeight);
            LayoutPanel.LayoutUpdated -= LayoutRoot_LayoutUpdated;
        }

        public void Clear()
        {
            LayoutPanel.Children.Clear();
        }

        public double GetCurrentOffset()
        {
            return ScrollViewer.VerticalOffset;
        }
        public async void SetCurrentOffset(double offset)
        {
            while (ScrollViewer.VerticalOffset != offset)
            {
                await Task.Delay(25);
                //ScrollViewer.ChangeView(null, offset, null, true);
                ScrollViewer.ScrollToVerticalOffset(offset);
            }
        }

        private void control_Tap(object sender, object e)
        {
            Selected = ((sender as FrameworkElement).Tag as Tuple<Trip, DateTime>).Item1;
            SelectedTime = ((sender as FrameworkElement).Tag as Tuple<Trip, DateTime>).Item2;
            if (ItemTapped != null)
                ItemTapped(this, new SelectionChangedEventArgs(new object[0], new object[] { Selected }));
        }

        //private void scrollTo(FrameworkElement scrollTarget)
        //{
        //    GeneralTransform transform = scrollTarget.TransformToVisual(ScrollViewer);
        //    Point position = transform.TransformPoint(new Point(0, 0));
        //    ScrollViewer.ChangeView(null, position.Y, null, true);
        //}
    }
}
