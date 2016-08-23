using CityTransitApp.WPSilverlight.Converters;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TransitBase.Entities;

namespace CityTransitApp.WPSilverlight.Tools
{
    public class RouteGroupColors : RouteGroupNames
    {
        public RouteGroupColors(RouteGroup outer) : base(outer) { }

        // = AppBackgroundBrush
        public static Color Overlay = Colors.White;

        public Color Color
        {
            get { return ToBrushConverter.ConvertToColor(outer.BgColor); }
        }

        public Brush BgColorBrush
        {
            get
            {
                return new SolidColorBrush(ToBrushConverter.ConvertToColor(outer.BgColor));
            }
        }
        public Brush FontColorBrush
        {
            get
            {
                return new SolidColorBrush(ToBrushConverter.ConvertToColor(outer.FontColor));
            }
        }

        public SolidColorBrush PrimaryColorBrush
        {
            get
            {
                //Color color = ToBrushConverter.ConvertToColor(BgColor);
                Color color = MainColor;
                color.R = (byte)((color.R + Overlay.R * 3) / 4);
                color.G = (byte)((color.G + Overlay.G * 3) / 4);
                color.B = (byte)((color.B + Overlay.B * 3) / 4);
                return new SolidColorBrush(color);
            }
        }

        public SolidColorBrush SecondaryColorBrush
        {
            get
            {
                //Color color = ToBrushConverter.ConvertToColor(BgColor);
                Color color = MainColor;
                color.R = (byte)((color.R + Overlay.R) / 2);
                color.G = (byte)((color.G + Overlay.G) / 2);
                color.B = (byte)((color.B + Overlay.B) / 2);
                return new SolidColorBrush(color);
            }
        }

        public SolidColorBrush LightColorBrush
        {
            get
            {
                //Color color = ToBrushConverter.ConvertToColor(BgColor);
                Color color = MainColor;
                color.R = (byte)((color.R + Overlay.R * 10) / 11);
                color.G = (byte)((color.G + Overlay.G * 10) / 11);
                color.B = (byte)((color.B + Overlay.B * 10) / 11);
                return new SolidColorBrush(color);
            }
        }

        public Color MainColor
        {
            get
            {
                Color color = ToBrushConverter.ConvertToColor(outer.BgColor);
                if (color == Colors.White)
                    return Colors.Gray;
                return color;
            }
        }
        public SolidColorBrush MainColorBrush { get { return new SolidColorBrush(MainColor); } }

        public bool IsRoute { get { return true; } }
        public bool IsStop { get { return false; } }

        #region Visibility conversion
        public Visibility LongNameVisiblity { get { return IsLongNameVisible ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility OutOfServiceVisibility { get { return IsOutOfServiceVisible ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility RoutesVisibility { get { return HasAnyRoute ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility LabelVisibility { get { return IsLabelVisible ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility VeryLongNameVisiblity { get { return IsVeryLongNameVisible ? Visibility.Visible : Visibility.Collapsed; } }
        #endregion
    }
}
