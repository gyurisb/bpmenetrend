using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CityTransitApp.WPSilverlight.Tools
{
    public abstract class TemplateSelector : ContentControl
    {
        public TemplateSelector()
        {
            this.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.SetBinding(ContentControl.ContentProperty, new System.Windows.Data.Binding());
        }

        public abstract DataTemplate SelectTemplate(object item, int index, int totalCount, DependencyObject container);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            var parent = GetParentByType<LongListSelector>(this);
            var index = parent.ItemsSource.IndexOf(newContent);
            var totalCount = parent.ItemsSource.Count;

            ContentTemplate = SelectTemplate(newContent, index, totalCount, this);
        }

        private static T GetParentByType<T>(DependencyObject element) where T : FrameworkElement
        {
            T result = null;
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null)
            {
                result = parent as T;

                if (result != null)
                {
                    return result;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}
