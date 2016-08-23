using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace CityTransitElements.Converters
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string text = (string)value;
            string[] vals = (parameter as string).Split('-');
            if (RouteGroupColors.IsLong(text))
                return int.Parse(vals[0]);
            else return int.Parse(vals[1]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
