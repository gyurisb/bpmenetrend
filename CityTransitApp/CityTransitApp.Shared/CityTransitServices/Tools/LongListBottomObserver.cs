using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using CityTransitApp;

namespace CityTransitServices.Tools
{
    public class LongListBottomObserver : IDisposable
    {
        private bool isAtBottom = false;
        private ListView longList;
        private ScrollViewer scrollViewer;
        private bool isInitialized = false;

        public event EventHandler ScrollToBottom;

        public LongListBottomObserver(ListView longList)
        {
            this.longList = longList;
            Initialize();
        }

        public void Dispose()
        {
            if (isInitialized)
                scrollViewer.ViewChanged -= scrollViewer_ViewChanged;
        }

        public async void Initialize()
        {
            while (scrollViewer == null)
            {
                await Task.Delay(10);
                this.scrollViewer = longList.FindVisualChildren<ScrollViewer>().FirstOrDefault();
            }
            while (longList.ActualHeight == 0)// || scrollViewer.ScrollableHeight == 0)
                await Task.Delay(100);
            scrollViewer_ViewChanged(null, null);
            scrollViewer.ViewChanged += scrollViewer_ViewChanged;
            isInitialized = true;
        }

        void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (IsAtBottom())
            {
                if (!isAtBottom && ScrollToBottom != null)
                    ScrollToBottom(longList, new EventArgs());
                isAtBottom = true;
            }
            else
            {
                isAtBottom = false;
            }
        }

        public bool IsAtBottom()
        {
            return scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - longList.ActualHeight;
        }
    }
}
