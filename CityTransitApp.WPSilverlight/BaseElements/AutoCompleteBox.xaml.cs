using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using TransitBase;

namespace CityTransitApp.WPSilverlight.BaseElements
{
    public partial class AutoCompleteBox : UserControl
    {
        private WaitingTimer waitingTimer;

        public AutoCompleteBox()
        {
            InitializeComponent();
            waitingTimer = new WaitingTimer(400, () =>
            {
                listCurrentValues();
            });
            //ResultList.ItemRealized += ResultList_ItemRealized;
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public IComparer<object> PriorityComparer { get; set; }

        public IEnumerable ItemsSource { get; set; }

        private IList defaultItems;
        public IList DefaultItems
        {
            get { return defaultItems; }
            set
            {
                defaultItems = value;
                listCurrentValues();
                ResultBorder.Height = App.RootFrame.ActualHeight * 0.5 - TextBox.ActualHeight;
            }
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listCurrentValues();
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        private void listCurrentValues()
        {
            if (TextBox.Text == "")
                ResultList.ItemsSource = defaultItems;
            else
            {
                string[] searchWords = TextBox.Text.Normalize().GetWords().ToArray();
                var result = ItemsSource.Cast<object>()
                    .Where(x => x.ToString().WordsStartWith(searchWords))
                    .ToList();
                result.Sort(PriorityComparer);
                ResultList.ItemsSource = result;
                //addItemsPaged(result);
            }

            ResultBorder.Visibility = ResultList.ItemsSource.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Paging deprecated
        private List<object> items = new List<object>();
        private void addItemsPaged(List<object> items, bool recursive = false)
        {
            if (!recursive)
            {
                ResultList.ItemsSource = new ObservableCollection<object>();
                //TotalProgress.Text = "" + items.Count;
                this.items = items;
            }
            var currentList = items.Take(10).ToList();
            lastItem = currentList.LastOrDefault();
            foreach (var item in currentList)
                ResultList.ItemsSource.Add(item);
            items.RemoveRange(0, Math.Min(10, items.Count));
            //VisibleProgress.Text = (int.Parse(TotalProgress.Text) - items.Count).ToString();
        }

        private object lastItem;
        void ResultList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (lastItem != null && items.Count > 0 && e.Container.DataContext == lastItem)
            {
                addItemsPaged(this.items, true);
            }
        }
        #endregion

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            waitingTimer.Restart();
        }

        private class WaitingTimer
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
    }
}
