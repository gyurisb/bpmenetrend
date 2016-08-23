using CityTransitElements.Effects;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase;
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

namespace CityTransitApp.CityTransitElements.BaseElements
{
    public sealed partial class StopPicker : UserControl
    {
#if WINDOWS_PHONE_APP
        private ContentDialog customDialog;
#endif

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
                    BaseText.Text = App.Common.Services.Resources.LocalizedStringOf("TimetableChooseStop");
                    BaseText.Foreground = (Brush)App.Current.Resources["AppFieldForegroundBrush2"];
                }
                if (SelectionChanged != null)
                    SelectionChanged(this, new SelectionChangedEventArgs(new object[0], new object[] { selected }));
            }
        }

        private void Base_Tapped(object sender, TappedRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            customDialog = new ContentDialog
            {
                ContentTemplate = (DataTemplate)Resources["MessageBoxTemplate"],
                FullSizeDesired = true,
            };
            customDialog.ShowAsync();
#endif
        }

        private static List<StopModel> boxItemSource;
        private static List<StopModel> defaultItemSourceSources;
        private static List<StopModel> defaultItemSourceTargets;
        private static Task<List<StopModel>> boxItemSourceTask = new Task<List<StopModel>>(() => boxItemSource);
        private static Task<List<StopModel>> defaultItemSourceSourcesTask = new Task<List<StopModel>>(() => defaultItemSourceSources);
        private static Task<List<StopModel>> defaultItemSourceTargetsTask = new Task<List<StopModel>>(() => defaultItemSourceTargets);

        public static void LoadItemSource()
        {
            App.ApplicationComponentChanged += async (sender, newComponent) =>
            {
                if (newComponent is TransitBaseComponent && newComponent != null && (newComponent as TransitBaseComponent).ContainsData)
                {
                    await LoadBoxItemSource();
                    await LoadBoxDefaultItemSourceSources();
                    await LoadBoxDefaultItemSourceTargets();
                }
            };
        }

        private static async Task LoadBoxItemSource()
        {
            boxItemSource = await Task.Run(
                () => App.TB.StopGroups.Select(x => new StopModel(x)).ToList()
            );
            if (!boxItemSourceTask.IsCompleted)
                boxItemSourceTask.Start();
        }
        private static async Task LoadBoxDefaultItemSourceSources()
        {
            defaultItemSourceSources = await Task.Run(() =>
            {
                var defaultSourceStops = new HashSet<StopGroup>(UserEstimations.GetPlanningHistory(true));
                return boxItemSource.Where(model => defaultSourceStops.Contains(model.Value)).ToList();
            });
            if (!defaultItemSourceSourcesTask.IsCompleted)
                defaultItemSourceSourcesTask.Start();
        }
        private static async Task LoadBoxDefaultItemSourceTargets()
        {
            defaultItemSourceTargets = await Task.Run(() =>
            {
                var defaultTargetStops = new HashSet<StopGroup>(UserEstimations.GetPlanningHistory(false));
                return boxItemSource.Where(model => defaultTargetStops.Contains(model.Value)).ToList();
            });
            if (!defaultItemSourceTargetsTask.IsCompleted)
                defaultItemSourceTargetsTask.Start();
        }

        public static async Task LoadItemsSourceTo(AutoCompleteBox box, bool isSource)
        {
            box.PriorityComparer = new StopModelComparer();
            box.ItemsSource = await boxItemSourceTask;
            box.DefaultItems = isSource ? await defaultItemSourceSourcesTask : await defaultItemSourceTargetsTask;
        }

        private async void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            ProgressBar progressBar = panel.Children[0] as ProgressBar;

            AutoCompleteBox box = panel.Children[1] as AutoCompleteBox;
            box.Visibility = Visibility.Collapsed;
            await LoadItemsSourceTo(box, IsSource);
            box.Visibility = Visibility.Visible;
            progressBar.Visibility = Visibility.Collapsed;

            box.Focus(FocusState.Pointer);
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

        public class StopModel : AutoCompleteBoxItemBase
        {
            protected StopGroup stopGroup;

            public StopModel(StopGroup stop) { stopGroup = stop; }

            public override string Title { get { return stopGroup.Name; } }
            public override string Footer { get { return String.Join(", ", stopGroup.RouteGroups.OrderByText(r => r.Name).OrderByWithCache(r => r.GetCustomTypePriority()).Select(r => r.Name)); } }

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
#if WINDOWS_PHONE_APP
            if (e.AddedItems.Count == 1)
            {
                Selected = e.AddedItems.Cast<StopModel>().First().Value;
                customDialog.Hide();
            }
#endif
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (e.OriginalSource is TextBox)
                customDialog.Hide();
#endif
        }
    }
}
