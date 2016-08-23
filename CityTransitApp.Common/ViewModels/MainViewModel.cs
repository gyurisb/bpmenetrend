using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TransitBase.Entities;
using System.Linq;
using System.ComponentModel;
using CityTransitServices.Tools;
using CityTransitApp.Common;
using UserBase.Interface;

namespace CityTransitApp.Common.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public ObservableCollection<RouteStopModel> Favorites { get; private set; }
        public IList<RouteGroup> History { get { return Get<IList<RouteGroup>>(); } set { Set(value); } }
        public IList<RouteGroup> Recent { get { return Get<IList<RouteGroup>>(); } set { Set(value); } }

        public IList<RouteGroup> RecentHistory { get { return Recent.Concat(History).Distinct().Take(rowCount * 4).ToList(); } }

        public bool FavoritesEmpty { get { return !Favorites.Any(); } }
        public bool FavoritesAny { get { return Favorites.Any(); } }
        public bool HistoryAny { get { return History != null && History.Any(); } }
        public bool RecentAny { get { return Recent != null && Recent.Any(); } }
        //public bool RecentEmpty { get { return !Recent.Any(); } }

        private int rowCount;

        public MainViewModel()
        {
            Favorites = new ObservableCollection<RouteStopModel>();
        }

        public override void Initialize(object initialData)
        {
            SetContent();
        }

        public void SetContent()
        {
            var favoriteList = CommonComponent.Current.UB.Favorites
                .OrderBy(fav => fav.Position ?? -1)
                .Select(x => new RouteStopModel { Route = x.Route, Stop = x.Stop })
                .ToList();
            foreach (var item in favoriteList)
                item.UpdateNextTrips();
            Favorites.Clear();
            foreach (var fav in favoriteList)
                Favorites.Add(fav);
            base.OnPropertyChanged("FavoritesEmpty");
            base.OnPropertyChanged("FavoritesAny");

            rowCount = (int)Services.Resources.ValueOf("HistoryRowCount");

            var history = CommonComponent.Current.UB.History.TimetableEntries;
            History = history
                .GroupBy(p => p.Route.RouteGroup)
                .Select(x => new { RouteGroup = x.Key, Rating1 = x.Min(y => HistoryHelpers.DayPartDistance(y)), Rating2 = x.Sum(y => y.RawCount) })
                .OrderByDescending(t => t.Rating2).OrderBy(t => t.Rating1)
                .Select(t0 => t0.RouteGroup)
                .Take(rowCount * 4)
                .ToList();
            base.OnPropertyChanged("HistoryAny");

            var recent = CommonComponent.Current.UB.History.GetRecents();
            Recent = recent
                .Select(x => x.Route.RouteGroup)
                .Distinct()
                .Take(rowCount)
                .ToList();
            base.OnPropertyChanged("RecentAny");
            base.OnPropertyChanged("RecentHistory");
            //base.OnPropertyChanged("RecentEmpty");
        }

        public void UpdateContent()
        {
            if (Favorites != null)
                foreach (var item in Favorites)
                    item.UpdateNextTrips();
        }

        public void FavDown(RouteStopModel favorite)
        {
            int index = Favorites.IndexOf(favorite);
            if (index < Favorites.Count - 1)
            {
                CommonComponent.Current.UB.Favorites.PushForward(favorite.Route, favorite.Stop);
                Favorites.RemoveAt(index);
                Favorites.Insert(index + 1, favorite);
            }
        }
        public void FavUp(RouteStopModel favorite)
        {
            int index = Favorites.IndexOf(favorite);
            if (index > 0)
            {
                CommonComponent.Current.UB.Favorites.PushBack(favorite.Route, favorite.Stop);
                Favorites.RemoveAt(index);
                Favorites.Insert(index - 1, favorite);
            }
        }

        public void RemoveFavorite(RouteStopModel favorite)
        {
            CommonComponent.Current.UB.Favorites.Remove(favorite.Route, favorite.Stop);
            Favorites.Remove(favorite);
        }
    }


    public class RouteStopModel : INotifyPropertyChanged
    {
        public StopGroup Stop { get; set; }
        public Route Route { get; set; }
        public string[] NextTrips { get; set; }
        public bool[] AreNextVisibles { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public int Padding { get; set; }

        public RouteStopModel() { HasNext = HasPrev = true; }

        public void UpdateNextTrips()
        {
            string paddingText = new string(Enumerable.Repeat(' ', Padding).ToArray());
            NextTrips = TransitProvider.GetCurrentTrips(DateTime.Now, Route, Stop, 1, 4)
                .Select(x => x != null ? x.Item1.ShortTimeString() + paddingText : "")
                .ToArray();
            //if (int.Parse(DateTime.Now.ToShortTimeString().Split(':')[0]) >= 20)
            if (!Route.RouteGroup.GetNames().IsVeryLongNameVisible && NextTrips.Count(h => h.Cast<char>().Count(ch => char.IsDigit(ch)) > 3) >= 3)
                NextTrips[4] = "";
            AreNextVisibles = NextTrips.Select(t => t != "").ToArray();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("NextTrips"));
                PropertyChanged(this, new PropertyChangedEventArgs("AreNextVisibles"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
