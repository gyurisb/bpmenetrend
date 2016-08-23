using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CityTransitApp.WPSilverlight.Effects
{
    public class RotateEffect : DependencyObject
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
              "IsEnabled",
              typeof(bool),
              typeof(RotateEffect),
              new PropertyMetadata(onIsRotateEnabledChanged)
          );

        public static bool GetIsEnabled(DependencyObject source)
        {
            return (bool)source.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject source, bool value)
        {
            source.SetValue(IsEnabledProperty, value);
        }

        private static void onIsRotateEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FrameworkElement)target;
            obj.ManipulationStarted += obj_ManipulationStarted;
            obj.ManipulationCompleted += obj_ManipulationCompleted;
        }

        static void obj_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            Rotate((UIElement)sender, 0, 30, false);
        }

        static void obj_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Rotate((UIElement)sender, 30, 0, false);
        }


        public static void Rotate(UIElement e, double from = 0, double to = 30, bool autoReverse = true)
        {
            e.Projection = new PlaneProjection { RotationX = from };
            DoubleAnimation anima = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(150),
                From = from,
                To = to,
                AutoReverse = autoReverse,
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(anima, e);
            Storyboard.SetTargetProperty(anima, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }
    }
}
