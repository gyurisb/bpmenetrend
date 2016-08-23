using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase
{
    public static class Extensions
    {
        public static int IndexOfMin<T, TKey>(this IList<T> list, Func<T, TKey> selector) where TKey : IComparable<TKey>
        {
            if (list.Count == 0) throw new InvalidOperationException();
            int minIndex = 0;
            for (int i = 1; i < list.Count; i++)
                if (selector(list[i]).CompareTo(selector(list[minIndex])) < 0)
                    minIndex = i;
            return minIndex;
        }
        public static int IndexOfMin<T>(this IList<T> list) where T : IComparable<T>
        {
            return list.IndexOfMin(x => x);
        }
        public static T MinBy<T, TKey>(this IEnumerable<T> coll, Func<T, TKey> pred) where TKey : IComparable<TKey>
        {
            T min = default(T);
            bool isSet = false;
            foreach (T e in coll)
                if (!isSet || pred(e).CompareTo(pred(min)) < 0)
                {
                    isSet = true;
                    min = e;
                }
            return min;
        }
        public static T MaxBy<T, TKey>(this IEnumerable<T> coll, Func<T, TKey> pred) where TKey : IComparable<TKey>
        {
            T max = default(T);
            bool isSet = false;
            foreach (T e in coll)
                if (!isSet || pred(e).CompareTo(pred(max)) > 0)
                {
                    isSet = true;
                    max = e;
                }
            return max;
        }

        public static int SetBits(this int target, int upperPos, int lowerPos, int value)
        {
            int n = upperPos - lowerPos;
            for (int i = lowerPos; i <= upperPos; i++)
                target &= ~(1 << i);
            target |= value << lowerPos;
            return target;
        }
        public static int GetBits(this int target, int upperPos, int lowerPos)
        {
            int value = 0;
            int k = 0;
            for (int i = lowerPos; i <= upperPos; i++)
                value |= ((target >> i) & 1) << (k++);
            return value;
        }

        public static string HourString(this DateTime dateTime, CultureInfo cultureInfo = null)
        {
            cultureInfo = cultureInfo ?? CultureInfo.CurrentUICulture;
            string timeStr = dateTime.ToString("t", cultureInfo);
            return timeStr.Substring(0, timeStr.IndexOf(':'));
        }

        public static DateTime NextDateTimeAt(this DateTime thisTime, TimeSpan time)
        {
            while (time > TimeSpan.FromDays(1))
                time -= TimeSpan.FromDays(1);
            DateTime dateTime = thisTime.Date + time;
            while (dateTime < thisTime)
                dateTime += TimeSpan.FromDays(1);
            return dateTime;
        }

        public static string Normalize(this string str)
        {
            return str.ToLower()
                .Replace('á', 'a')
                .Replace('ä', 'a')
                .Replace('é', 'e')
                .Replace('í', 'i')
                .Replace('ó', 'o')
                .Replace('ö', 'o')
                .Replace('ő', 'o')
                .Replace('ü', 'u')
                .Replace('ű', 'u')
                .Replace('ú', 'u');
        }

        public static IEnumerable<string> GetWords(this string text)
        {
            return text.Split(' ', '/', ',').Where(w => w != "");
        }

        public static bool WordsStartWith(this string text, string[] prefixWords)
        {
            var textWords = text.Normalize().GetWords().ToList();
            return prefixWords.All(pword => textWords.Any(tword => tword.StartsWith(pword)));
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> elements)
        {
            foreach (var e in elements)
                list.Add(e);
        }
        public static void AddObjectRange<T>(this IList list, IEnumerable<T> elements)
        {
            foreach (var e in elements)
                list.Add(e);
        }

        public static IEnumerable<T> OrderByWithCache<T, TKey>(this IEnumerable<T> array, Func<T, TKey> selector)
        {
            return array.Select(e => Tuple.Create(e, selector(e))).OrderBy(e => e.Item2).Select(e => e.Item1);
        }

        public static IEnumerable<T> OrderByText<T>(this IEnumerable<T> array, Func<T, string> selector)
        {
            return array.OrderByWithCache(e => new ComparableString(selector(e)));
        }

        public static T MostFrequent<T>(this IEnumerable<T> coll)
        {
            return coll.GroupBy(element => element).MaxBy(group => group.Count()).Key;
        }
    }
}
