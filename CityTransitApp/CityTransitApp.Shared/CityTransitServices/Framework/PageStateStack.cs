using CityTransitApp.CityTransitServices.Tools;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CityTransitApp
{
    public static class PageStateStack
    {
        public static Frame Frame;
        public static List<PageState> BackStack = new List<PageState>();

        public static void Pop()
        {
            BackStack.RemoveAt(BackStack.Count - 1);
        }

        public static void Push(PageState state)
        {
            BackStack.Add(state);
        }

        public static PageState Top()
        {
            return BackStack.Last();
        }

        //public static PageState SameCachedPage(Type sourceType)
        //{
        //    return BackStack.SingleOrDefault(entry => entry.PageType == sourceType && entry.IsCached);
        //}
    }
}
