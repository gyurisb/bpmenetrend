using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Media;
using System.Windows.Data;

namespace CityTransitApp.WPSilverlight.Converters
{
    public class ToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            uint color = (uint)value;
            return new SolidColorBrush(ConvertToColor(color));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
