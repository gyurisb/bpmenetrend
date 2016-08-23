using CityTransitApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace CityTransitElements.Converters
{
    public class FontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int maxSize = int.Parse((string)parameter);
            if (value != null)
            {
                string text = (string)value;
                int length = text.Length;
                double newSize = calculate(length, maxSize);
                if (App.Config.TextSizeRateCalculator != null)
                    newSize *= App.Config.TextSizeRateCalculator(text);
                return newSize;
            }
            return maxSize;
        }


        private double calculate(int length, double maxSize)
        {
            if (length <= 3)
                return (double)maxSize;
            else if (length <= 5)
                return maxSize * 22.0 / 30.0;
            else
                return maxSize * 21.0 / 30.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    
}
