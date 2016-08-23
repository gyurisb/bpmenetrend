using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Linq;
using CityTransitApp;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;

namespace CityTransitElements.Effects
{
    public class HoverEffects : DependencyObject
    {
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
              "Foreground",
              typeof(Brush),
              typeof(HoverEffects),
              new PropertyMetadata(null, foregroundValueChanged)
          );

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.RegisterAttached(
              "BorderBrush",
              typeof(Brush),
              typeof(HoverEffects),
              new PropertyMetadata(null, borderBrushValueChanged)
          );

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
              "Background",
              typeof(Brush),
              typeof(HoverEffects),
              new PropertyMetadata(null, backgroundBrushValueChanged)
          );

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.RegisterAttached(
              "BorderThickness",
              typeof(Thickness),
              typeof(HoverEffects),
              new PropertyMetadata(null, borderThicknessValueChanged)
          );


        private static void foregroundValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = (UIElement)target;
            Brush value = (Brush)e.NewValue;
            List<TextBlockWithOriginalForeground> texts = null;
            if (target is TextBlock)
            {
                texts = new List<TextBlockWithOriginalForeground>();
                texts.Add(new TextBlockWithOriginalForeground { TextBlock = (TextBlock)target });
            }

            element.PointerEntered += (sender, args) =>
            {
                if (texts == null)
                {
                    texts = target.FindVisualChildren<TextBlock>().Select(t => new TextBlockWithOriginalForeground { TextBlock = t }).ToList();
                }

                foreach (var text in texts)
                {
                    //text.OriginalBinding = text.TextBlock.GetBindingExpression(TextBlock.ForegroundProperty);
                    //if (text.OriginalBinding == null)
                    text.OriginalForeground = text.TextBlock.Foreground;
                    text.TextBlock.Foreground = value;
                }
            };
            element.PointerExited += (sender, args) =>
            {
                foreach (var text in texts)
                {
                    //if (text.OriginalBinding != null)
                    //    BindingOperations.SetBinding(text.TextBlock, TextBlock.ForegroundProperty, text.OriginalBinding.ParentBinding);
                    //else
                    text.TextBlock.Foreground = text.OriginalForeground;
                }
            };
            element.PointerCanceled += (sender, args) =>
            {
                foreach (var text in texts)
                {
                    text.TextBlock.Foreground = text.OriginalForeground;
                }
            };
        }

        private static void borderBrushValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Border border = (Border)target;
            Brush originalValue = null;
            Brush newValue = (Brush)e.NewValue;
            border.PointerEntered += (sender, args) =>
            {
                originalValue = (sender as Border).BorderBrush;
                (sender as Border).BorderBrush = newValue;
            };
            border.PointerExited += (sender, args) =>
            {
                (sender as Border).BorderBrush = originalValue;
            };
            border.PointerCanceled += (sender, args) =>
            {
                (sender as Border).BorderBrush = originalValue;
            };
        }

        private static void backgroundBrushValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is Border)
            {
                Border border = (Border)target;
                Brush originalValue = null;
                Brush newValue = (Brush)e.NewValue;
                border.PointerEntered += (sender, args) =>
                {
                    originalValue = (sender as Border).Background;
                    (sender as Border).Background = newValue;
                };
                border.PointerExited += (sender, args) =>
                {
                    (sender as Border).Background = originalValue;
                };
                border.PointerCanceled += (sender, args) =>
                {
                    (sender as Border).Background = originalValue;
                };
            }
            else if (target is Grid)
            {
                Grid grid = (Grid)target;
                Brush originalValue = null;
                Brush newValue = (Brush)e.NewValue;
                grid.PointerEntered += (sender, args) =>
                {
                    originalValue = (sender as Grid).Background;
                    (sender as Grid).Background = newValue;
                };
                grid.PointerExited += (sender, args) =>
                {
                    (sender as Grid).Background = originalValue;
                };
                grid.PointerCanceled += (sender, args) =>
                {
                    (sender as Grid).Background = originalValue;
                };
            }
            else if (target is Ellipse)
            {
                Ellipse elipse = (Ellipse)target;
                Brush originalValue = null;
                Brush newValue = (Brush)e.NewValue;
                elipse.PointerEntered += (sender, args) =>
                {
                    originalValue = (sender as Ellipse).Fill;
                    (sender as Ellipse).Fill = newValue;
                };
                elipse.PointerExited += (sender, args) =>
                {
                    (sender as Ellipse).Fill = originalValue;
                };
                elipse.PointerCanceled += (sender, args) =>
                {
                    (sender as Ellipse).Fill = originalValue;
                };
            }
        }

        private static void borderThicknessValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Border border = (Border)target;
            Thickness originalValue;
            Thickness newValue = (Thickness)e.NewValue;
            border.PointerEntered += (sender, args) =>
            {
                originalValue = (sender as Border).BorderThickness;
                (sender as Border).BorderThickness = newValue;
            };
            border.PointerExited += (sender, args) =>
            {
                (sender as Border).BorderThickness = originalValue;
            };
            border.PointerCanceled += (sender, args) =>
            {
                (sender as Border).BorderThickness = originalValue;
            };
        }

        #region property getters/setters
        public static Brush GetForeground(DependencyObject source)
        {
            return (Brush)source.GetValue(ForegroundProperty);
        }

        public static void SetForeground(DependencyObject source, Brush value)
        {
            source.SetValue(ForegroundProperty, value);
        }

        public static Brush GetBorderBrush(DependencyObject source)
        {
            return (Brush)source.GetValue(BorderBrushProperty);
        }

        public static void SetBorderBrush(DependencyObject source, Brush value)
        {
            source.SetValue(BorderBrushProperty, value);
        }

        public static Brush GetBackground(DependencyObject source)
        {
            return (Brush)source.GetValue(BackgroundProperty);
        }

        public static void SetBackground(DependencyObject source, Brush value)
        {
            source.SetValue(BackgroundProperty, value);
        }

        public static Thickness GetBorderThickness(DependencyObject source)
        {
            return (Thickness)source.GetValue(BorderThicknessProperty);
        }

        public static void SetBorderThickness(DependencyObject source, Thickness value)
        {
            source.SetValue(BorderThicknessProperty, value);
        }
        #endregion

        private class TextBlockWithOriginalForeground
        {
            public TextBlock TextBlock;
            public Brush OriginalForeground;
            //public BindingExpression OriginalBinding;
        }
    }
}
