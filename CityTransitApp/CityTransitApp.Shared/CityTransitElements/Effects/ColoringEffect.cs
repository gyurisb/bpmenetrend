using CityTransitApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CityTransitElements.Effects
{
    public class ColoringEffect : DependencyObject
    {
        public static readonly DependencyProperty ApplyOnTappedProperty = DependencyProperty.RegisterAttached(
              "ApplyOnTapped",
              typeof(bool),
              typeof(ColoringEffect),
              new PropertyMetadata(false, propertyValueChanged)
          );
        
        public static readonly DependencyProperty OriginalBackgroundProperty = DependencyProperty.RegisterAttached(
              "OriginalBackground",
              typeof(Brush),
              typeof(ColoringEffect),
              null
          );

        public static bool GetApplyOnTapped(DependencyObject source)
        {
            return (bool)source.GetValue(ApplyOnTappedProperty);
        }

        public static void SetApplyOnTapped(DependencyObject source, bool value)
        {
            source.SetValue(ApplyOnTappedProperty, value);
        }

        private static void propertyValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FrameworkElement)target;
            var property = getBackgroundProperty(obj.GetType());
            Brush bgBrush = null;
            obj.PointerEntered += (sender, args) =>
            {
                bgBrush = (Brush)obj.GetValue(property);
                obj.SetValue(property, (Brush)App.Current.Resources["PhoneAccentBrush"]);
            };
            obj.ManipulationStarted += (sender, args) =>
            {
                bgBrush = (Brush)obj.GetValue(property);
                obj.SetValue(property, (Brush)App.Current.Resources["PhoneAccentBrush"]);
            };
            obj.PointerCaptureLost += (sender, args) =>
            {
                obj.SetValue(property, bgBrush);
            };
            obj.PointerExited += (sender, args) =>
            {
                obj.SetValue(property, bgBrush);
            };
            obj.ManipulationCompleted += (sender, args) =>
            {
                obj.SetValue(property, bgBrush);
            };
        }

        private static void setTapColor(FrameworkElement element)
        {
            var prop = getBackgroundProperty(element.GetType());
            var bg = element.GetValue(prop);
            element.SetValue(OriginalBackgroundProperty, bg);
            element.SetValue(prop, App.Current.Resources["PhoneAccentBrush"]);
        }
        private static void setOriginalColor(FrameworkElement element)
        {
            var prop = getBackgroundProperty(element.GetType());
            var bg = element.GetValue(OriginalBackgroundProperty);
            element.SetValue(prop, bg);
            element.SetValue(OriginalBackgroundProperty, null);
        }

        private static DependencyProperty getBackgroundProperty(Type type)
        {
            if (type == typeof(Border))
                return Border.BackgroundProperty;
            else if (type == typeof(Grid))
                return Grid.BackgroundProperty;
            else if (type == typeof(StackPanel))
                return StackPanel.BackgroundProperty;
            else throw new NotImplementedException("Background property is not registered in ColoringEffect for type: " + type.ToString());
        }
    }
}
