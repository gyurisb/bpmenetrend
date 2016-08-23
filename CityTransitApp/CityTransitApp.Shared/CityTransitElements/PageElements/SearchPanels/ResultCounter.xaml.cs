using CityTransitApp.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.BusinessLogic;
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

namespace CityTransitApp.CityTransitElements.PageElements.SearchPanels
{
    public sealed partial class ResultCounter : UserControl
    {
        public ResultCounter()
        {
            InitializeComponent();
            ResultModel = SearchResultModel.Empty;
        }

        public event EventHandler<SearchResultCategory> ResultCategorySelected;
        private void fireResultCategorySelected(SearchResultCategoryType type, params int[] routeTypes)
        {
            if (ResultCategorySelected != null)
                ResultCategorySelected(this, new SearchResultCategory { Type = type, RouteTypes = routeTypes });
        }

        public static readonly DependencyProperty ResultModelProperty = DependencyProperty.RegisterAttached(
              "ResultModel",
              typeof(SearchResultModel),
              typeof(ResultCounter),
              new PropertyMetadata(SearchResultModel.Empty, resultModelValueChanged)
          );
        public SearchResultModel ResultModel
        {
            get { return (SearchResultModel)this.GetValue(ResultModelProperty); }
            set { this.SetValue(ResultModelProperty, value); }
        }

        private static void resultModelValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ResultCounter)d).setModel((SearchResultModel)e.NewValue);
        }

        //public void SetModel(SearchResultModel model)
        //{
        //    ResultModel = model;
        //}

        private void setModel(SearchResultModel model)
        {
            LayoutRoot.DataContext = model;
        }

        //public void SetResult(IEnumerable<StopGroup> stops, IEnumerable<RouteGroup> routes)
        //{
        //    LayoutRoot.DataContext = CreateModel(stops, routes);
        //}

        //internal void Reset()
        //{
        //    SetResult(null, null);
        //}

        #region Border tap handlers
        private void Stop_Tap(object sender, TappedRoutedEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Stop);
        }

        private void Subway_Tap(object sender, TappedRoutedEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Metro);
        }

        private void Train_Tap(object sender, TappedRoutedEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.RailRoad);
        }

        private void Tram_Tap(object sender, TappedRoutedEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Tram);
        }

        private void Bus_Tap(object sender, TappedRoutedEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Bus, RouteType.CableCar);
        }

        private void Ship_Tap(object sender, TappedRoutedEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Ferry);
        }
        #endregion
    }
}
