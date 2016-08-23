using CityTransitApp.CityTransitElements.PageElements.SearchPanels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase.Entities;
using UserBase.BusinessLogic;
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
using CityTransitApp.Common.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageParts
{
    public sealed partial class SearchResultPart : UserControl
    {
        public event EventHandler<StopGroup> StopSelected;
        public event EventHandler<RouteGroup> RouteGroupSelected;
        public event EventHandler<Route> RouteSelected;

        public SearchViewModel ViewModel { get; private set; }

        public SearchResultPart()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel = new SearchViewModel();
        }

        void SearchResult_ResultCategorySelected(object sender, SearchResultCategory e)
        {
            ViewModel.SelectResultCategory(e);
        }

        #region Item selection event handlers
        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchList.SelectedItem != null)
            {
                var selected = SearchList.SelectedItem as StopModel;
                if (selected == null) return;
                if (StopSelected != null)
                    StopSelected(this, selected.Stop);
                SearchList.SelectedItem = null;
            }
        }
        private void RouteGroupClicked(object sender, EventArgs e)
        {
            RouteGroup selected = sender as RouteGroup;
            if (RouteGroupSelected != null)
                RouteGroupSelected(this, selected);
        }
        private void routeClicked(object sender, EventArgs e)
        {
            Route selected = sender as Route;
            if (sender == null) return;
            if (RouteSelected != null)
                RouteSelected(this, selected);
        }
        #endregion
        
        #region forwarded operations to VM

        public void SetContent(IEnumerable<RouteGroup> routeSelection)
        {
            ViewModel.SetContent(routeSelection);
        }
        public async Task SetContent(string searchText, bool full)
        {
            await ViewModel.SetContent(searchText, full);
        }
        public void ResetSearchResult()
        {
            ViewModel.ResetSearchResult();
        }
        public bool ClearCategorySelection()
        {
            return ViewModel.ClearCategorySelection();
        }

        #endregion
    }


    public class SearchListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RouteItemTemplate { get; set; }
        public DataTemplate StopItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is RouteGroup)
                return RouteItemTemplate;
            if (item is StopModel)
                return StopItemTemplate;

            throw new NotSupportedException();
        }
    }
}
