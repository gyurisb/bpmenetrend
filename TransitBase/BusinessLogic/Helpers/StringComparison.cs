using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase
{
    public class ComparableList<T> : List<T>, IComparable<ComparableList<T>> where T : IComparable<T>
    {
        public int CompareTo(ComparableList<T> other)
        {
            var enu1 = this.GetEnumerator();
            var enu2 = other.GetEnumerator();
            bool enu1HasNext = true, enu2HasNext = true;
            while (true)
            {
                if (!enu1.MoveNext())
                    enu1HasNext = false;
                if (!enu2.MoveNext())
                {
                    enu2HasNext = false;
                    break;
                }
                if (!enu1HasNext)
                    break;
                int val = enu1.Current.CompareTo(enu2.Current);
                if (val != 0)
                    return val;
            }
            if (enu1HasNext && !enu2HasNext)
                return 1;
            if (!enu1HasNext && enu2HasNext)
                return -1;
            return 0;
        }
    }

    public class ComparableString : IComparable<ComparableString>
    {
        ComparableList<StringElement> value;
        public ComparableString(string value)
        {
            this.value = convertString(value);
        }
        public int CompareTo(ComparableString other)
        {
            return value.CompareTo(other.value);
        }


        private class StringElement : IComparable<StringElement>
        {
            char character;
            int number;
            byte priority;
            public StringElement(char ch) { character = ch; priority = 1; }
            public StringElement(int nr) { number = nr; priority = 0; }
            //public override string ToString()
            //{
            //    if (priority == 0)
            //        return number.ToString();
            //    return character.ToString();
            //}


            public int CompareTo(StringElement other)
            {
                int val1 = priority - other.priority;
                if (val1 != 0) return val1;
                if (priority == 0)
                {
                    return number - other.number;
                }
                return character.CompareTo(other.character);
            }
        }

        private static ComparableList<StringElement> convertString(string str)
        {
            var result = new ComparableList<StringElement>();
            StringBuilder numberBuilder = new StringBuilder();
            foreach (char ch in str)
            {
                if (!Char.IsDigit(ch))
                {
                    if (numberBuilder.Length > 0)
                    {
                        result.Add(new StringElement(int.Parse(numberBuilder.ToString())));
                        numberBuilder.Clear();
                    }
                    if (ch != ' ')
                    {
                        result.Add(new StringElement(ch));
                    }
                }
                else
                {
                    numberBuilder.Append(ch);
                }
            }
            if (numberBuilder.Length > 0)
            {
                result.Add(new StringElement(int.Parse(numberBuilder.ToString())));
            }
            return result;
        }
    }

    //class StringComparator : IComparer<String>
    //{
    //    public int Compare(string x, string y)
    //    {
    //        return x.CompareTo(y);
    //    }
    //}
}
