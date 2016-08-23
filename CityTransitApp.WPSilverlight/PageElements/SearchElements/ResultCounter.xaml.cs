using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CityTransitApp.Common.ViewModels;
using TransitBase.BusinessLogic;

namespace CityTransitApp.WPSilverlight.PageElements.SearchElements
{
    public partial class ResultCounter : UserControl
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
        private void Stop_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Stop);
        }

        private void Subway_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Metro);
        }

        private void Train_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.RailRoad);
        }

        private void Tram_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Tram);
        }

        private void Bus_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Bus, RouteType.CableCar);
        }

        private void Ship_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            fireResultCategorySelected(SearchResultCategoryType.Route, RouteType.Ferry);
        }
        #endregion
    }
}
