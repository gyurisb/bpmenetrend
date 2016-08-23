using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Data;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace CityTransitElements.Converters
{
    public class ToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            uint color = (uint)value;
            return new SolidColorBrush(ConvertToColor(color));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }


        public static Color ConvertToColor(uint value)
        {
            byte[] a = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                a = a.Reverse().ToArray();
            a[0] = 255;
            return Color.FromArgb(a[0], a[1], a[2], a[3]);
        }
    }
}
