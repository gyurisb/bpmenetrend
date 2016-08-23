using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using System.Collections.ObjectModel;
using TransitBase.BusinessLogic.Helpers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.BaseElements
{
    public sealed partial class AutoCompleteBox : UserControl
    {
        private WaitingTimer waitingTimer;
        private object resultSelected;
        private bool ignoreLostFocus;
        private bool ignoreTextBoxChanged;
        private Page page;
        private bool loaded = false;

        #region public properties

        public event SelectionChangedEventHandler SelectionChanged;

        public IComparer<object> PriorityComparer { get; set; }

        public IList ItemsSource { get; set; }


        private IList defaultItems;
        public IList DefaultItems
        {
            get { return defaultItems; }
            set
            {
                defaultItems = value;
                //listCurrentValues();
            }
        }

        private object selected;
        public object Selected
        {
            get
            {
                if (TextBox.Text.Length > 0)
                {
                    var possibleItems = getPossibleItems();
                    if (possibleItems != null && possibleItems.Count > 0)
                    {
                        if (!possibleItems.Contains(selected))
                            selected = possibleItems[0];
                        changeTextWithoutOpening(((AutoCompleteBoxItemBase)selected).Title);
                        return selected;
                    }
                }
                return null;
            }
        }

        #endregion

        public AutoCompleteBox()
        {
            InitializeComponent();
            this.Loaded += AutoCompleteBox_Loaded;

            TextBox.GotFocus += (sender, args) => listCurrentValues();
            waitingTimer = new WaitingTimer(400, () => listCurrentValues());
        }

        void AutoCompleteBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
#if !WINDOWS_PHONE_APP
                this.page = TextBox.FirstVisualParent<Page>();
                page.Tapped += page_Tapped;
                TextBox.KeyUp += TextBox_KeyUp;
                ResultList.KeyUp += ResultList_KeyUp;
                TextBox.LostFocus += TextBox_LostFocus;
#endif
#if WINDOWS_PHONE_APP
                ResultBorder.Height = App.GetAppInfo().GetScreenHeight()*0.4 - TextBox.ActualHeight;
#endif
                ResultBorder.Width = TextBox.ActualWidth;
                loaded = true;
            }
        }

        void page_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Point p = e.GetPosition(page);
            Rect rect = new Rect(p.X - 50, p.Y, 100, 100);
            var elementsAtTap = VisualTreeHelper.FindElementsInHostCoordinates(rect, page);
            if (!elementsAtTap.Contains(this))
                closePopup();
        }

        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ignoreLostFocus)
                closePopup();
            else ignoreLostFocus = false;
        }

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (ResultList.ItemsSource().Count > 0)
                    selectResultElement(ResultList.ItemsSource()[0]);
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                page.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            }
            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                if (ResultList.ItemsSource().Count > 0 && ResultPopup.IsOpen)
                {
                    if (TextBox.FocusState != Windows.UI.Xaml.FocusState.Unfocused)
                        ignoreLostFocus = true;
                    ResultList.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                }
            }
        }

        void ResultList_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (resultSelected != null)
                    selectResultElement(resultSelected);
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                page.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                closePopup();
            }
        }

        private void ResultList_ItemSelected(object sender, TappedRoutedEventArgs e)
        {
            selectResultElement(((FrameworkElement)sender).DataContext);
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResultList.SelectedItem = null;
            if (e.AddedItems.Any())
                this.resultSelected = e.AddedItems.First();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ignoreTextBoxChanged)
            {
                waitingTimer.Restart();
            }
            else ignoreTextBoxChanged = false;
        }

        #region helpers

        private void selectResultElement(object resultSelected)
        {
            this.selected = resultSelected;
            closePopup();
            changeTextWithoutOpening(selected.ToString());
            if (SelectionChanged != null)
                SelectionChanged(this, new SelectionChangedEventArgs(new object[0], new object[] { selected }));
        }

        private void changeTextWithoutOpening(string newText)
        {
            if (TextBox.Text != newText)
            {
                ignoreTextBoxChanged = true;
                TextBox.Text = newText;
            }
        }

        private void listCurrentValues()
        {
            IList newResultList;
            ResultList.ItemsSource = newResultList = getPossibleItems();

            if (newResultList.Count > 0)
                openPopup();
            else
                closePopup();
        }

        private IList getPossibleItems()
        {
            if (TextBox.Text == "")
                return defaultItems;
            else
            {
                string[] searchWords = TextBox.Text.Normalize().GetWords().ToArray();
                var result = ItemsSource.Cast<object>()
                    .Where(x => x.ToString().WordsStartWith(searchWords))
                    .ToList();
                result.Sort(PriorityComparer);
                return result;
            }
        }

        private void closePopup()
        {
            this.resultSelected = null;
            ResultPopup.IsOpen = false;
        }
        private void openPopup()
        {
            ResultPopup.IsOpen = true;
        }

        #endregion
    }

    public class WaitingTimer
    {
        private DispatcherTimer timer;

        public WaitingTimer(int delayTime, Action action)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(delayTime);
            timer.Tick += (x, args) =>
            {
                timer.Stop();
                action();
            };
        }
        public void Restart()
        {
            timer.Stop();
            timer.Start();
        }
    }

    public abstract class AutoCompleteBoxItemBase : Bindable
    {
        public bool IsSelected { get { return Get<bool>(); } set { Set(value); } }

        public abstract string Title { get; }
        public abstract string Footer { get; }
    }
}
