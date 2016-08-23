using CityTransitApp.CityTransitElements.BaseElements;
using CityTransitApp.CityTransitElements.PageParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CityTransitApp.Pages.Dialogs
{
    class DateTimePhoneDialog : IDateTimePickerDialog
    {
        public async Task<DateTimeModel> ShowAsync()
        {
            var currentFrame = (Frame)Window.Current.Content;
            currentFrame.Navigate(typeof(DateTimePhonePage));
            return await DateTimePhonePage.Current.CalculateValueAsync(DateTimeModel);
        }

        public DateTimeModel DateTimeModel { get; set; }
    }
}
