using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace CityTransitElements.Converters
{
    class DateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var dateTime = (DateTime)value;
            return (DateTimeOffset)dateTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var dateTimeOffset = (DateTimeOffset)value;
            return dateTimeOffset.DateTime;
        }
    }
}
