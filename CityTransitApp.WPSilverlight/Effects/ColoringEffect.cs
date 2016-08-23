using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CityTransitApp.WPSilverlight.Effects
{
    public class ColoringEffect : DependencyObject
    {
        public static readonly DependencyProperty ApplyOnTapProperty = DependencyProperty.RegisterAttached(
              "ApplyOnTap",
              typeof(bool),
              typeof(ColoringEffect),
              new PropertyMetadata(propertyValueChanged)
          );

        public static readonly DependencyProperty OriginalBackgroundProperty = DependencyProperty.RegisterAttached(
              "OriginalBackground",
              typeof(Brush),
              typeof(ColoringEffect),
              null
          );

        public static bool GetApplyOnTap(DependencyObject source)
        {
            return (bool)source.GetValue(ApplyOnTapProperty);
        }

        public static void SetApplyOnTap(DependencyObject source, bool value)
        {
            source.SetValue(ApplyOnTapProperty, value);
        }

        private static void propertyValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FrameworkElement)target;
            obj.ManipulationStarted += obj_ManipulationStarted;
            obj.ManipulationCompleted += obj_ManipulationCompleted;
        }

        private static void obj_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            var obj = (FrameworkElement)sender;
            var prop = getBackgroundProperty(sender.GetType());
            var bg = obj.GetValue(prop);
            obj.SetValue(OriginalBackgroundProperty, bg);
            obj.SetValue(prop, App.Current.Resources["PhoneAccentBrush"]);
        }

        private static void obj_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            var obj = (FrameworkElement)sender;
            var prop = getBackgroundProperty(sender.GetType());
            var bg = obj.GetValue(OriginalBackgroundProperty);
            obj.SetValue(prop, bg);
            obj.SetValue(OriginalBackgroundProperty, null);
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
