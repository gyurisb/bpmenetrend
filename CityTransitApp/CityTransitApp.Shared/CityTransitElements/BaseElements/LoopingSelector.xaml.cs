using CityTransitServices.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.BusinessLogic.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using CityTransitApp;
using Windows.UI.Xaml.Media.Animation;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.BaseElements
{
    public sealed partial class LoopingSelector : UserControl
    {
        public static event EventHandler OtherClick;
        public static void PerformOtherClick(object sender)
        {
            if (OtherClick != null)
                OtherClick(sender, new EventArgs());
        }

        //fields
        private LoopingSelectorModel dataContext;
        private double itemHeight;
        private double topPadding;
        private bool isInitialized = false;

        //scrolling fields
        double holdY = -1;
        double holdOffset = -1;
        bool isHolding = false;
        double lastOffset = 0;
        TimeSpan lastMeasuredTime, lastMovedTime;
        double speedInPxPer50ms = 0;
        AsyncStoryboard scrollingStory = null;
        PeriodicTask speedMonitorTask;

        public LoopingSelector()
        {
            this.InitializeComponent();
            this.DataContext = dataContext = new LoopingSelectorModel();
            ContentPanel.RenderTransform = new TranslateTransform { Y = 0.0 };
            Initialize();
            LostFocus += (sender, args) => stopAndDexpand();
            OtherClick += LoopingSelector_OtherClick;
            Unloaded += (sender, args) => OtherClick -= LoopingSelector_OtherClick;
        }

        void LoopingSelector_OtherClick(object sender, EventArgs e)
        {
            if (sender != this && DataSource != null)
            {
                stopAndDexpand();
            }
        }

        private async void stopAndDexpand()
        {
            if (scrollingStory != null)
                await scrollingStory.AsTask();
            dexpand(); 
        }

        private void ItemBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = (LoopingSelectorItemBase)(sender as FrameworkElement).DataContext;
            unselect();
            select(item);
            scrollIntoView(item, true);

            if (!isExpanded)
                expand();
        }

        #region Public properties

        public Size ItemSize
        {
            get { return new Size(dataContext.Width, dataContext.Height); }
            set { dataContext.SetSize(value); }
        }
        public Thickness ItemMargin
        {
            get { return dataContext.ItemMargin; }
            set { dataContext.ItemMargin = value; }
        }
        public DataTemplate ItemTemplate { get; set; }

        private LoopingSelectorItemBase currentSelectedItem;
        public LoopingSelectorItemBase SelectedItem
        {
            get
            {
                return getElementAtScrollPosition();
            }
            set
            {
                if (isInitialized)
                    throw new InvalidOperationException("LoopingSelector.SelectedItem can only be set right after initialization");
                currentSelectedItem = value;
            }
        }

        private List<LoopingSelectorItemBase> dataSource;
        public IEnumerable<LoopingSelectorItemBase> DataSource
        {
            get { return dataSource; }
            set
            {
                if (isInitialized)
                    throw new InvalidOperationException("LoopingSelector.DataSource can only be set right after initialization");
                dataSource = value.ToList();
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                throw new NotImplementedException();
                //isExpanded = value;
            }
        }

        #endregion

        #region custom dependency properties

        public static readonly DependencyProperty ContentForegroundProperty = DependencyProperty.RegisterAttached(
              "ContentForeground",
              typeof(Brush),
              typeof(LoopingSelector),
              new PropertyMetadata(null, onContentForegroundChanged)
          );
        public static Brush GetContentForeground(DependencyObject source)
        {
            return (Brush)source.GetValue(ContentForegroundProperty);
        }
        public static void SetContentForeground(DependencyObject source, Brush value)
        {
            source.SetValue(ContentForegroundProperty, value);
        }
        private static void onContentForegroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            foreach (var text in obj.FindVisualChildren<TextBlock>())
            {
                text.Foreground = (Brush)e.NewValue;
            }
        }

        #endregion

        private async void Initialize()
        {
            await Task.Delay(100);
            isInitialized = true;
            ContentPanel.Width = this.ActualWidth;

            if (dataSource != null)
            {
                var listItemTemplate = (DataTemplate)Resources["ListItemTemplate"];
                foreach (var item in dataSource)
                {
                    item.Opacity = 0.0;
                    item.MarkUnselected();

                    var element = (Border)listItemTemplate.LoadContent();
                    element.Height = dataContext.Height;
                    element.Margin = dataContext.ItemMargin;
                    element.Child = (FrameworkElement)ItemTemplate.LoadContent();
                    element.DataContext = item;

                    ContentPanel.Children.Add(element);
                }

                this.itemHeight = this.ItemSize.Height + this.ItemMargin.Top + this.ItemMargin.Bottom;
                this.topPadding = this.ActualHeight / 2 - itemHeight * 0.5;
                ContentPanel.Margin = new Thickness(0, topPadding, 0, 0);

                select(currentSelectedItem ?? dataSource.First());
                currentSelectedItem.Opacity = 1.0;
                scrollIntoView(currentSelectedItem);

                addScrollingHandlers();
            }
        }

        #region User scrolling handlers

        private void addScrollingHandlers()
        {
            RootFrame.PointerPressed += ContentPanel_PointerPressed;
            speedMonitorTask = new PeriodicTask(50, monitorSpeed);
            speedMonitorTask.Run();

            var page = ContentPanel.FirstVisualParent<Page>();
            page.PointerMoved += Page_PointerMoved;
            page.PointerReleased += Page_PointerReleased;
            this.Unloaded += (sender, args) =>
            {
                page.PointerMoved -= Page_PointerMoved;
                page.PointerReleased -= Page_PointerReleased;
                speedMonitorTask.Cancel();
            };
        }

        private void monitorSpeed()
        {
            if (isHolding)
            {
                TimeSpan now = DateTime.Now.TimeOfDay;
                if (now - lastMeasuredTime >= TimeSpan.FromMilliseconds(50))
                    measureSpeed(getVerticalOffset(), now);
            }
        }

        void ContentPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isHolding = true;
            lastMeasuredTime = lastMovedTime  = DateTime.Now.TimeOfDay;
            holdY = e.GetCurrentPoint(RootFrame).Position.Y;
            holdOffset = lastOffset = getVerticalOffset();
            pauseAnimation();
            expand();
        }

        void Page_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isHolding)
            {
                TimeSpan now = DateTime.Now.TimeOfDay;
                if (now - lastMovedTime > TimeSpan.FromMilliseconds(50))
                {
                    var point = e.GetCurrentPoint(RootFrame);
                    double targetVerticalOffset = holdOffset - (point.Position.Y - holdY);

                    pauseAnimation();
                    scrollTo(targetVerticalOffset, true, 50);
                    unselect();
                    measureSpeed(targetVerticalOffset, now);

                    lastMovedTime = now;
                }
            }
        }

        async void Page_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (isHolding)
            {
                isHolding = false;
                if (Math.Abs(speedInPxPer50ms) > 10)
                {
                    double v = speedInPxPer50ms;
                    double a = v > 0 ? -5.0 : 5.0;
                    double t = -v / a;
                    //double s = v * t / 2;
                    double s = -a / 2 * t * t;

                    double currentOffset = getVerticalOffset();
                    double targetOffset = currentOffset + s;
                    targetOffset = Math.Round(targetOffset / itemHeight) * itemHeight;
                    targetOffset = Math.Max(0.0, targetOffset);
                    targetOffset = Math.Min((dataSource.Count - 1) * itemHeight, targetOffset);

                    double s1 = targetOffset - currentOffset; //s = a/2*t^2 <=> t = sqrt(2s/a)
                    double a1 = s1 < 0 ? -5.0 : 5.0;
                    double t1 = Math.Sqrt(2 * s1 / a1);

                    unselect();
                    if (await scrollTo(targetOffset, true, t1 * 50))
                    {
                        select(getElementAtScrollPosition());
                    }
                }
                else
                {
                    var item = getElementAtScrollPosition();
                    unselect();
                    if (await scrollIntoView(item, true))
                    {
                        select(item);
                    }
                }
            }
        }

        private void measureSpeed(double nextScrollOffset, TimeSpan? now = null)
        {
            now = now ?? DateTime.Now.TimeOfDay;
            TimeSpan ellapsedTime = now.Value - lastMeasuredTime;
            if (ellapsedTime > TimeSpan.FromMilliseconds(10))
            {
                this.speedInPxPer50ms = (nextScrollOffset - lastOffset) / (ellapsedTime.TotalMilliseconds / 50.0);
                this.lastOffset = nextScrollOffset;
                this.lastMeasuredTime = now.Value;
            }
        }
        #endregion

        #region Scrolling functions
        private LoopingSelectorItemBase getElementAtScrollPosition()
        {
            return dataSource.ElementAt((int)Math.Round(getVerticalOffset() / itemHeight));
        }
        private async Task<bool> scrollIntoView(LoopingSelectorItemBase item, bool useAnimation = false)
        {
            double offset = dataSource.IndexOf(item);
            return await scrollTo(offset * itemHeight, useAnimation);
        }
        private double getVerticalOffset()
        {
            return -Canvas.GetTop(ContentPanel);
        }
        private async Task<bool> scrollTo(double verticalOffset, bool useAnimation = false, double durationInMs = 300)
        {
            verticalOffset = Math.Max(0, verticalOffset);
            verticalOffset = Math.Min((dataSource.Count - 1) * itemHeight, verticalOffset);
            pauseAnimation();
            if (!useAnimation)
            {
                Canvas.SetTop(ContentPanel, -verticalOffset);
                return true;
            }
            else
            {
                var story = this.scrollingStory = animateTo(verticalOffset, durationInMs);
                var ret = await scrollingStory.AsTask();
                if (story == this.scrollingStory)
                    this.scrollingStory = null;
                return ret;
            }
        }
        private AsyncStoryboard animateTo(double verticalOffset, double durationInMs)
        {
            bool ret = false;
            Task<bool> task = new Task<bool>(() => ret);

            DoubleAnimation anima = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(durationInMs),
                From = Canvas.GetTop(ContentPanel),
                To = -verticalOffset,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(anima, ContentPanel);
            Storyboard.SetTargetProperty(anima, "(Canvas.Top)");

            var scrollingStory = new Storyboard();
            scrollingStory.Children.Add(anima);
            scrollingStory.Begin();
            return new AsyncStoryboard(scrollingStory);
        }
        private void pauseAnimation()
        {
            if (scrollingStory != null)
            {
                scrollingStory.Pause();
                scrollingStory = null;
            }
        }
        private void scrollToRelative(double verticalOffsetDelta)
        {
            scrollTo(getVerticalOffset() + verticalOffsetDelta);
        }
        #endregion

        #region List manipulation methods
        private void expand()
        {
            PerformOtherClick(this);
            foreach (var item in dataSource)
                item.Opacity = 1.0;
            isExpanded = true;
        }

        private void dexpand()
        {
            foreach (var item in dataSource.Except(new LoopingSelectorItemBase[] { currentSelectedItem }))
                item.Opacity = 0.0;
            isExpanded = false;
        }

        private void unselect()
        {
            if (currentSelectedItem != null)
            {
                currentSelectedItem.MarkUnselected();
                currentSelectedItem = null;
            }
        }

        private void select(LoopingSelectorItemBase selectedItem)
        {
            this.currentSelectedItem = selectedItem;
            selectedItem.MarkSelected();
        }
        #endregion

        private class LoopingSelectorModel
        {
            public double Height { get; private set; }
            public double Width { get; private set; }
            public Thickness ItemMargin { get; set; }
            public Thickness ListMargin { get; set; }
            //public object SelectedItem { get { return Get<object>(); } set { Set(value); } }

            public void SetSize(Size size) { Height = size.Height; Width = size.Width; }
        }
    }

    public abstract class LoopingSelectorItemBase : Bindable
    {
        public double Opacity { get { return Get<double>(); } set { Set(value); } }
        public Thickness BorderThickness { get { return Get<Thickness>(); } set { Set(value); } }
        public Brush Background { get { return Get<Brush>(); } set { Set(value); } }
        public Brush Foreground { get { return Get<Brush>(); } set { Set(value); } }

        public void MarkUnselected()
        {
            Background = new SolidColorBrush(Colors.Transparent);
            BorderThickness = new Thickness(2);
            Foreground = new SolidColorBrush(Colors.Gray);
        }

        internal void MarkSelected()
        {
            BorderThickness = new Thickness(0);
            Background = (Brush)App.Current.Resources["PhoneAccentBrush"];
            Foreground = new SolidColorBrush(Colors.White);
        }
    }
}
