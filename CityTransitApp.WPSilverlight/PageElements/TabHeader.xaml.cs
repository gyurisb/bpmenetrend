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
using CityTransitApp.Common.ViewModels;
using TransitBase.BusinessLogic.Helpers;

namespace CityTransitApp.WPSilverlight.PageElements
{
    public partial class TabHeader : UserControl
    {
        public class TabElementViewModel : Bindable
        {
            public Brush BackgroundBrush { get { return Get<Brush>(); } set { Set(value); } }
            public Brush ForegroundBrush { get { return Get<Brush>(); } set { Set(value); } }
            public string PrimaryLine { get { return Get<string>(); } set { Set(value); } }
            public string SecondaryLine { get { return Get<string>(); } set { Set(value); } }
        }

        public event SelectionChangedEventHandler SelectionChanged;

        private TabElementViewModel[] ViewModels;

        #region dependency property declarations
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.RegisterAttached(
              "SelectedIndex",
              typeof(int),
              typeof(TabHeader),
              new PropertyMetadata(0, selectedIndexValueChanged)
          );

        public static readonly DependencyProperty BaseBrushProperty = DependencyProperty.RegisterAttached(
              "BaseBrush",
              typeof(Brush),
              typeof(TabHeader),
              new PropertyMetadata(new SolidColorBrush(Colors.Transparent), baseBrushValueChanged)
          );

        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.RegisterAttached(
              "SelectedBrush",
              typeof(Brush),
              typeof(TabHeader),
              new PropertyMetadata(new SolidColorBrush(Colors.Transparent), selectedBrushValueChanged)
          );

        public static readonly DependencyProperty BaseForegroundBrushProperty = DependencyProperty.RegisterAttached(
              "BaseForegroundBrush",
              typeof(Brush),
              typeof(TabHeader),
              new PropertyMetadata(new SolidColorBrush(Colors.Black), baseForegroundBrushValueChanged)
          );

        public static readonly DependencyProperty SelectedForegroundBrushProperty = DependencyProperty.RegisterAttached(
              "SelectedForegroundBrush",
              typeof(Brush),
              typeof(TabHeader),
              new PropertyMetadata(new SolidColorBrush(Colors.Black), selectedForegroundBrushValueChanged)
          );

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.RegisterAttached(
              "DataSource",
              typeof(TabHeaderText[]),
              typeof(TabHeader),
              new PropertyMetadata(null, dataSourceValueChanged)
          );
        #endregion

        private static void selectedIndexValueChanged(DependencyObject outer, DependencyPropertyChangedEventArgs e)
        {
            var element = (TabHeader)outer;
            int newValue = (int)e.NewValue;
            int oldValue = (int)e.OldValue;
            element.ViewModels[oldValue].BackgroundBrush = element.BaseBrush;
            element.ViewModels[newValue].BackgroundBrush = element.SelectedBrush;
            element.ViewModels[oldValue].ForegroundBrush = element.BaseForegroundBrush;
            element.ViewModels[newValue].ForegroundBrush = element.SelectedForegroundBrush;
        }
        private static void baseBrushValueChanged(DependencyObject outer, DependencyPropertyChangedEventArgs e)
        {
            var element = (TabHeader)outer;
            Brush newValue = e.NewValue as Brush;
            for (int i = 0; i < element.ViewModels.Length; i++)
                if (i != element.SelectedIndex)
                    element.ViewModels[i].BackgroundBrush = newValue;
        }
        private static void selectedBrushValueChanged(DependencyObject outer, DependencyPropertyChangedEventArgs e)
        {
            var element = (TabHeader)outer;
            Brush newValue = e.NewValue as Brush;
            element.ViewModels[element.SelectedIndex].BackgroundBrush = newValue;
        }
        private static void baseForegroundBrushValueChanged(DependencyObject outer, DependencyPropertyChangedEventArgs e)
        {
            var element = (TabHeader)outer;
            Brush newValue = e.NewValue as Brush;
            for (int i = 0; i < element.ViewModels.Length; i++)
                if (i != element.SelectedIndex)
                    element.ViewModels[i].ForegroundBrush = newValue;
        }
        private static void selectedForegroundBrushValueChanged(DependencyObject outer, DependencyPropertyChangedEventArgs e)
        {
            var element = (TabHeader)outer;
            Brush newValue = e.NewValue as Brush;
            element.ViewModels[element.SelectedIndex].ForegroundBrush = newValue;
        }
        private static void dataSourceValueChanged(DependencyObject outer, DependencyPropertyChangedEventArgs e)
        {
            var element = (TabHeader)outer;
            var dataSource = e.NewValue as TabHeaderText[];
            if (dataSource != null)
            {
                int i = 0;
                foreach (var source in dataSource)
                {
                    element.ViewModels[i].PrimaryLine = source.PrimaryLine;
                    element.ViewModels[i].SecondaryLine = source.SecondaryLine;
                    i++;
                }
            }
        }

        #region dependency property stubs
        public int SelectedIndex
        {
            get { return (int)this.GetValue(SelectedIndexProperty); }
            set { this.SetValue(SelectedIndexProperty, value); }
        }
        public Brush BaseBrush
        {
            get { return (Brush)this.GetValue(BaseBrushProperty); }
            set { this.SetValue(BaseBrushProperty, value); }
        }
        public Brush SelectedBrush
        {
            get { return (Brush)this.GetValue(SelectedBrushProperty); }
            set { this.SetValue(SelectedBrushProperty, value); }
        }
        public Brush BaseForegroundBrush
        {
            get { return (Brush)this.GetValue(BaseForegroundBrushProperty); }
            set { this.SetValue(BaseForegroundBrushProperty, value); }
        }
        public Brush SelectedForegroundBrush
        {
            get { return (Brush)this.GetValue(SelectedForegroundBrushProperty); }
            set { this.SetValue(SelectedForegroundBrushProperty, value); }
        }
        public TabHeaderText[] DataSource
        {
            get { return (TabHeaderText[])this.GetValue(DataSourceProperty); }
            set { this.SetValue(DataSourceProperty, value); }
        }
        #endregion

        public TabHeader()
        {
            this.InitializeComponent();
            ViewModels = Enumerable.Repeat(true, 4).Select(x => new TabElementViewModel()).ToArray();
            int i = 0;
            foreach (Button item in TimeStripGrid.Children)
                item.DataContext = ViewModels[i++];
        }

        private void NextTrip_Click(object sender, RoutedEventArgs e)
        {
            int targetHeaderIndex = ViewModels.ToList().IndexOf((TabElementViewModel)((FrameworkElement)sender).DataContext);
            int oldIndex = this.SelectedIndex;
            this.SelectedIndex = targetHeaderIndex;
            if (SelectionChanged != null)
                SelectionChanged(this, new SelectionChangedEventArgs(new object[] { oldIndex }, new object[] { targetHeaderIndex }));
        }
    }
}
