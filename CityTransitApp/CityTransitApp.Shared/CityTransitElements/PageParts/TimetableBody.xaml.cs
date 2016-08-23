using CityTransitApp.CityTransitElements.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TransitBase;
using Windows.UI.Text;
using CityTransitElements.Effects;
using TransitBase.Entities;
using Windows.UI.Xaml.Documents;
using TransitBase.BusinessLogic.Helpers;
using CityTransitApp.Common.ViewModels;
using CityTransitApp.Common.ViewModels.Settings;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class TimetableBody : UserControl
    {
        private static readonly double BLOCK_SIZE = 51.0;
        private static readonly double BODY_FONT = 21.0;
        private static readonly double LABEL_FONT = 17.0;
        private static readonly double LABEL_HEIGHT = 25.0;
        private static readonly double MARGIN_XLARGE = 10.0;
        private static readonly double MARGIN_LARGE = 8.0;
        private static readonly double MARGIN_SMALL = 4.0;

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
              new PropertyMetadata(false, dataSourceValueChanged)
          );

        private static void dataSourceValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeTableBodySource value = e.NewValue as TimeTableBodySource;
            TimetableBody element = (TimetableBody)d;
            if (value == null)
                element.Clear();
            else element.SetHourList((Frame)Window.Current.Content, value.HourList, value.ScrollTarget);
        }

        public TimeTableBodySource DataSource
        {
            get { return (TimeTableBodySource)this.GetValue(DataSourceProperty); }
            set { this.SetValue(DataSourceProperty, value); }
        }

        public async void SetHourList(Frame page, IList<TimeTableBodyHourGroup> hourList, TimeTableBodyHourGroup scrollTarget)
        {
            LayoutPanel.Children.Clear();

            //néha az ActualWidth property nem áll be időben, ekkor meg kell várni
            while (page.ActualWidth == 0)
                await Task.Delay(10);

            int columnCount = (int)Math.Floor((page.ActualWidth - 61.0) / BLOCK_SIZE);
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
                    hourPanel.Children.Add(new TextBlock {
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
                                //Text = trip.TimeMin.ToString(),
                                FontSize = BODY_FONT,
                                Foreground = (Brush)trip.TextColor,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            if (underlineWheelchair && (trip.Trip.WheelchairAccessible ?? false))
                                text.Inlines.Add(new Underline());
                            else
                                text.Inlines.Add(new Span());
                            text.Inlines.Add(new Run { Text = trip.TimeMin.ToString() });
                            var control = new Border
                            {
                                Width = columnWidth,
                                Height = BLOCK_SIZE,
                                Background = (Brush)trip.BorderColor,
                                Child = text,
                                Tag = Tuple.Create(trip.Trip, trip.Time)
                            };
                            control.SetValue(RotateEffect.IsEnabledProperty, true);
                            control.Tapped += control_Tap;

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
                ScrollViewer.ChangeView(null, targetHeight, null, true);
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
                ScrollViewer.ChangeView(null, offset, null, true);
            }
        }

        private void control_Tap(object sender, TappedRoutedEventArgs e)
        {
            Selected = ((sender as FrameworkElement).Tag as Tuple<Trip, DateTime>).Item1;
            SelectedTime = ((sender as FrameworkElement).Tag as Tuple<Trip, DateTime>).Item2;
            if (ItemTapped != null)
                ItemTapped(this, new SelectionChangedEventArgs(new object[0], new object[]{Selected}));
        }

        private void scrollTo(FrameworkElement scrollTarget)
        {
            GeneralTransform transform = scrollTarget.TransformToVisual(ScrollViewer);
            Point position = transform.TransformPoint(new Point(0, 0));
            ScrollViewer.ChangeView(null, position.Y, null, true);
        }
    }
}
