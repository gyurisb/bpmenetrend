using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using TransitBase.BusinessLogic.Helpers;
using System.Threading.Tasks;
using TransitBase.Entities;
using TransitBase;
using CityTransitApp;
using TransitBase.BusinessLogic;
using CityTransitApp.Common;

namespace CityTransitApp.Common.ViewModels
{
    public class SearchViewModel : ViewModel<string>
    {
        public IList ResultItems { get { return Get<IList>(); } set { Set(value); } }
        public SearchResultModel ResultCounterData { get { return Get<SearchResultModel>(); } set { Set(value); } }
        public string SearchKey { get { return Get<string>(); } set { Set(value); } }

        public override void Initialize(string initialData)
        {
            SetContent(initialData, true);
        }

        public void SetContent(IEnumerable<RouteGroup> routeSelection)
        {
            ResultItems = routeSelection.OrderByText(r => r.Name).ToList();
        }

        public async Task SetContent(string searchText, bool full, Func<object, bool> searchFilter = null)
        {
            searchFilter = searchFilter ?? (o => true);

            this.SearchKey = searchText;

            IEnumerable<StopModel> stops = new StopModel[0];
            IEnumerable<RouteGroup> routes = new RouteGroup[0];

            if (searchText.Length > 0)
            {
                routes = await Task.Run(() =>
                    CommonComponent.Current.TB.Logic.FindRoutes(searchText)
                    .OrderByText(r => r.Name)
                    .ToList()
                );
            }

            if (searchText.Length >= 3)
            {
                //elvileg nem teljesen pontos, mert lehetnek azonos route-ok a szummában de közelítésnek megteszi
                //+ A megálló végállomásként is számít
                stops = await Task.Run(() =>
                    CommonComponent.Current.TB.Logic
                    .FindStops(searchText)
                    .Select(s => new StopModel(s, true, true))
                    .OrderByText(s => s.Name)
                    .OrderByDescending(s => s.RouteCount)
                    .OrderBy(s => s.HighestPriority)
                    .ToList()
                );
            }

            if (stops.Take(1).Count() == 0)
            {
                ResultItems = full ? routes.Where(o => searchFilter(o)).ToList() : routes.Take(5).ToList();
            }
            else if (routes.Take(1).Count() == 0)
            {
                var stops1 = full ? stops.ToList() : stops.Take(5).ToList();
                ResultItems = stops1.Where(searchFilter).ToList();
            }
            else
            {
                var firstRoutes = routes.Where(r => r.Name.Normalize().Contains(searchText.Normalize())).Cast<object>().ToList();
                var lastRoutes = routes.Cast<object>().Except(firstRoutes);
                var routesAndStops = firstRoutes.Concat(stops).Concat(lastRoutes).ToList();
                ResultItems = full ? routesAndStops.Where(searchFilter).ToList() : routesAndStops.Take(5).ToList();
            }
            SetSearchResult(stops.Select(s => s.Stop), routes);
        }

        public void SetSearchResult(IEnumerable<StopGroup> stops, IEnumerable<RouteGroup> routes)
        {
            ResultCounterData = new SearchResultModel(stops, routes);
        }

        public async Task SelectResultCategory(SearchResultCategory category)
        {
            await Task.Delay(150);
            Func<object, bool> searchFilter = null;
            if (category.Type == SearchResultCategoryType.Stop)
            {
                searchFilter = (o => o is StopModel);
            }
            else
            {
                searchFilter = (o => o is RouteGroup && category.RouteTypes.Contains((o as RouteGroup).Type));
            }
            await SetContent(SearchKey, true, searchFilter);
            categorySelected = true;
            ResetSearchResult();
        }

        private bool categorySelected = false;
        public bool ClearCategorySelection()
        {
            if (categorySelected)
            {
                categorySelected = false;
                return true;
            }
            return false;
        }

        public void ResetSearchResult()
        {
            ResultCounterData = SearchResultModel.Empty;
        }
    }


    public class SearchResultModel
    {
        public int StopCount { get; set; }
        public int MetroCount { get; set; }
        public int TrainCount { get; set; }
        public int TramCount { get; set; }
        public int BusCount { get; set; }
        public int FerryCount { get; set; }

        public bool IsStopVisible { get { return StopCount > 0; } }
        public bool IsMetroVisible { get { return MetroCount > 0; } }
        public bool IsTrainVisible { get { return TrainCount > 0; } }
        public bool IsTramVisible { get { return TramCount > 0; } }
        public bool IsBusVisible { get { return BusCount > 0; } }
        public bool IsFerryVisible { get { return FerryCount > 0; } }

        public static SearchResultModel Empty { get { return new SearchResultModel(); } }

        public SearchResultModel() { }
        public SearchResultModel(IEnumerable<StopGroup> stops, IEnumerable<RouteGroup> routes)
        {
            stops = stops ?? new StopGroup[0];
            routes = routes ?? new RouteGroup[0];
            StopCount = stops.Count();
            MetroCount = routes.Count(r => r.Type == RouteType.Metro);
            TrainCount = routes.Count(r => r.Type == RouteType.RailRoad);
            TramCount = routes.Count(r => r.Type == RouteType.Tram);
            BusCount = routes.Count(r => r.Type == RouteType.Bus);
            FerryCount = routes.Count(r => r.Type == RouteType.Ferry);
        }
    }

    public enum SearchResultCategoryType { Stop, Route };
    public class SearchResultCategory
    {
        public SearchResultCategoryType Type;
        public int[] RouteTypes;
    }
}
