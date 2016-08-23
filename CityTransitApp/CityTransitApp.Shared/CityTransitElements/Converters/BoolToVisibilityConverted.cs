using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CityTransitElements.Converters
{
    class BoolToVisibilityConverted : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool val = (bool)value;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
