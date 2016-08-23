using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TransitBase;
using TransitBase.Entities;
using CityTransitServices.Tools;
using System.Threading.Tasks;
using CityTransitApp.WPSilverlight.Effects;
using System.Windows.Media;
using CityTransitApp.WPSilverlight.Resources;
using CityTransitApp.Common;

namespace CityTransitApp.WPSilverlight.BaseElements
{
    public partial class StopPicker : UserControl
    {
        private CustomMessageBox messageBox;

        public StopPicker()
        {
            InitializeComponent();
            Animations.OnMouseColorChange(Root);
        }

        public event SelectionChangedEventHandler SelectionChanged;

        public bool IsSource { get; set; }

        private StopGroup selected;
        public StopGroup Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (value != null)
                {
                    BaseText.Text = value.Name;
                    BaseText.Foreground = (Brush)App.Current.Resources["AppFieldForegroundBrush"];
                }
                else
                {
                    BaseText.Text = AppResources.TimetableChooseStop;
                    BaseText.Foreground = (Brush)App.Current.Resources["AppFieldForegroundBrush2"];
                }
                if (SelectionChanged != null)
                    SelectionChanged(this, new SelectionChangedEventArgs(new object[0], new object[] { selected }));
            }
        }

        private void Base_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            messageBox = new CustomMessageBox
            {
                ContentTemplate = (DataTemplate)Resources["MessageBoxTemplate"],
                IsFullScreen = true
            };
            messageBox.Show();
        }

        private static Task<List<StopModel>> boxItemSourceTask;
        private static Task<List<StopModel>> defaultItemSourceTaskSources;
        private static Task<List<StopModel>> defaultItemSourceTaskTargets;
        public static void LoadItemSource()
        {
            App.EngineLoaded += Model_Loaded;

        }
        private static void Model_Loaded(object sender, EventArgs e)
        {
            boxItemSourceTask = Task.Run(
                () => App.Model.StopGroups.Select(x => new StopModel(x)).ToList()
            );
            defaultItemSourceTaskSources = Task.Run(
                () => UserEstimations.GetPlanningHistory(true)
                    .Select(stop => new StopModel(stop))
                    .ToList()
            );
            defaultItemSourceTaskTargets = Task.Run(
                () => UserEstimations.GetPlanningHistory(false)
                    .Select(stop => new StopModel(stop))
                    .ToList()
            );
        }

        private async void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            ProgressBar progressBar = panel.Children[0] as ProgressBar;

            AutoCompleteBox box = panel.Children[1] as AutoCompleteBox;
            box.Visibility = Visibility.Collapsed;
            box.PriorityComparer = new StopModelComparer();
            box.ItemsSource = await boxItemSourceTask;
            box.DefaultItems = IsSource ? await defaultItemSourceTaskSources : await defaultItemSourceTaskTargets;
            box.Visibility = Visibility.Visible;
            progressBar.Visibility = Visibility.Collapsed;

            box.Focus();
        }

        private class StopModelComparer : IComparer<object>
        {
            public int Compare(object x, object y)
            {
                StopModel model1 = x as StopModel, model2 = y as StopModel;
                double val1 = model1.HighestPriority - model2.HighestPriority;
                if (val1 != 0)
                    return Math.Sign(val1);
                int val2 = model2.RouteCount - model1.RouteCount;
                if (val2 != 0)
                    return val2;
                return model1.ComparableName.CompareTo(model2.ComparableName);
            }
        }

        private class StopModel
        {
            protected StopGroup stopGroup;

            public StopModel(StopGroup stop) { stopGroup = stop; }

            public string Title { get { return stopGroup.Name; } }
            public string Footer { get { return String.Join(", ", stopGroup.RouteGroups.OrderByText(r => r.Name).OrderByWithCache(r => r.GetCustomTypePriority()).Select(r => r.Name)); } }

            private int routeCount = -1;
            public int RouteCount
            {
                get
                {
                    if (routeCount == -1)
                        setValues();
                    return routeCount;
                }
            }
            private double highestPriority = -1;
            public double HighestPriority
            {
                get
                {
                    if (highestPriority == -1)
                        setValues();
                    return highestPriority;
                }
            }
            private ComparableString comparableName;
            public ComparableString ComparableName
            {
                get
                {
                    if (comparableName == null)
                        setValues();
                    return comparableName;
                }
            }

            private void setValues()
            {
                var routes = stopGroup.RouteGroups.ToList();
                routeCount = routes.Count;
                highestPriority = routes.Count > 0 ? routes.Min(r => r.GetCustomTypePriority()) : double.MaxValue;
                comparableName = new ComparableString(stopGroup.Name);
            }

            public override string ToString() { return stopGroup.Name; }
            public StopGroup Value { get { return stopGroup; } }
        }

        private void TextBox_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                Selected = e.AddedItems.Cast<StopModel>().First().Value;
                messageBox.Dismiss();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TextBox)
                messageBox.Dismiss();
        }
    }
}
