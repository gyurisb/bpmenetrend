using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CityTransitApp.WPSilverlight.Effects
{
    public class Animations
    {
        public static void OnMouseColorChange(Border border)
        {
            onMouseColorChange(border, Border.BackgroundProperty);
        }
        public static void OnMouseColorChange(StackPanel border)
        {
            onMouseColorChange(border, StackPanel.BackgroundProperty);
        }
        private static void onMouseColorChange(UIElement e, DependencyProperty property)
        {
            Brush bgBrush = null;
            e.MouseEnter += (sender, args) =>
            {
                bgBrush = (Brush)e.GetValue(property);
                e.SetValue(property, (Brush)App.Current.Resources["PhoneAccentBrush"]);
            };
            e.LostMouseCapture += (sender, args) =>
            {
                e.SetValue(property, bgBrush);
            };
            e.MouseLeave += (sender, args) =>
            {
                e.SetValue(property, bgBrush);
            };
        }

        public static void FadeInFromRight(FrameworkElement e, PhoneApplicationPage page)
        {
            e.RenderTransform = new TranslateTransform { X = page.ActualWidth };
            DoubleAnimation anima = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                From = page.ActualWidth,
                To = 0,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(anima, e);
            Storyboard.SetTargetProperty(anima, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }

        public static async void FadeInFromBottomAfter(FrameworkElement e, PhoneApplicationPage page, int milliseconds)
        {
            e.RenderTransform = new TranslateTransform { Y = page.ActualHeight };
            await Task.Delay(milliseconds);
            FadeInFromBottom(e, page);
        }
        public static void FadeInFromBottom(FrameworkElement e, PhoneApplicationPage page)
        {
            e.RenderTransform = new TranslateTransform { Y = page.ActualHeight };
            DoubleAnimation anima = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(150),
                From = page.ActualHeight,
                To = 0,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(anima, e);
            Storyboard.SetTargetProperty(anima, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }

        public static void Press(FrameworkElement e)
        {
            e.Projection = new PlaneProjection { GlobalOffsetX = 0, GlobalOffsetZ = 0 };
            DoubleAnimation anima1 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(100),
                From = 0,
                To = 10,
                AutoReverse = true,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(anima1, e);
            Storyboard.SetTargetProperty(anima1, new PropertyPath("(UIElement.Projection).(PlaneProjection.GlobalOffsetX)"));

            DoubleAnimation anima2 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(100),
                From = 0,
                To = -100,
                AutoReverse = true,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(anima2, e);
            Storyboard.SetTargetProperty(anima2, new PropertyPath("(UIElement.Projection).(PlaneProjection.GlobalOffsetZ)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima2);
            storyboard.Children.Add(anima1);
            storyboard.Begin();
        }
    }
}
