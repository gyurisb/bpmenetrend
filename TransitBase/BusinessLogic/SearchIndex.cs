using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace TransitBase.BusinessLogic
{
    internal class SearchIndex
    {
        Tuple<string, RouteGroup>[] routeIndex;
        Tuple<string, RouteGroup>[] routeLongIndex;
        Tuple<string, List<StopGroup>>[] stopIndex;

        public SearchIndex(IEnumerable<StopGroup> stops, IEnumerable<RouteGroup> routes)
        {
            routeIndex = routes
                .SelectMany(rg => wordTuples(rg.Name, rg))
                .OrderBy(rg => rg.Item1)
                .ToArray();
            routeLongIndex = routes
                .SelectMany(rg => wordTuples(rg.Description, rg).Concat(rg.Routes.SelectMany(r => wordTuples(r.Name, rg))))
                .OrderBy(rg => rg.Item1)
                .ToArray();

            stopIndex = stops
                .SelectMany(s => wordTuples(s.Name, s))
                .GroupBy(t => t.Item1)
                .Select(g => Tuple.Create(g.Key, g.Select(t => t.Item2).ToList()))
                .OrderBy(t => t.Item1)
                .ToArray();
        }

        private static IEnumerable<Tuple<string, T>> wordTuples<T>(string text, T value)
        {
            return text.GetWords().Select(w => Tuple.Create(w.Normalize(), value));
        }


        public IList<RouteGroup> SearchRoute(string text)
        {
            IEnumerable<RouteGroup> list = null;
            foreach (string word in text.Normalize().GetWords())
            {
                var wordItems = searchStringStart(word, routeIndex);
                if (word.Length >= 3)
                    wordItems.AddRange(searchStringStart(word, routeLongIndex));
                if (list == null)
                    list = wordItems.Distinct();
                else
                    list = list.Intersect(wordItems);
            }
            return (list ?? new RouteGroup[0]).ToList();
        }

        public IList<StopGroup> SearchStop(string text)
        {
            IEnumerable<StopGroup> list = null;
            foreach (string word in text.Normalize().GetWords())
            {
                var wordItems = searchStringStart(word, stopIndex).SelectMany(x => x);
                if (list == null)
                    list = wordItems.Distinct();
                else
                    list = list.Intersect(wordItems);
            }
            return (list ?? new StopGroup[0]).ToList();
        }

        private class TupleComparer<T> : IComparer
        {
            public int Compare(object x0, object y0)
            {
                var x = (Tuple<string, T>)x0;
                var y = (string)y0;
                return x.Item1.CompareTo(y);
            }
        }

        private static IList<T> searchStringStart<T>(string text, Tuple<string, T>[] array)
        {
            int index = Array.BinarySearch(array, text, new TupleComparer<T>());
            var list = new List<T>();

            foreach (int incr in new int[] { -1, 1 })
            {
                for (int i = index < 0 ? -index - 1 : index; i < array.Length && i >= 0; i += incr)
                {
                    if (array[i].Item1.StartsWith(text))
                    {
                        list.Add(array[i].Item2);
                    }
                    else break;
                }
            }
            return list.Distinct().ToList();
        }


    }
}
