using CityTransitApp.CityTransitElements.PageParts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CityTransitApp.Pages.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DateTimePhonePage : Page
    {
        public static DateTimePhonePage Current { get; set; }

        private Task currentTask;
        private bool okClicked = false;
        private bool goBackProgramatically = false;

        public DateTimePhonePage()
        {
            Current = this;
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (!goBackProgramatically)
                endTask(false);
        }

        public async Task<DateTimeModel> CalculateValueAsync(DateTimeModel model)
        {
            DateTimePickerPart.DateTimeModel = model;
            currentTask = new Task(() => { });
            await currentTask;
            if (okClicked)
                return DateTimePickerPart.DateTimeModel;
            return null;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            endTask(true);
            goBackProgramatically = true;
            this.Frame.GoBack();
            goBackProgramatically = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            endTask(false);
            goBackProgramatically = true;
            this.Frame.GoBack();
            goBackProgramatically = false;
        }

        private void endTask(bool isOk)
        {
            this.okClicked = isOk;
            if (currentTask != null)
                currentTask.Start();
            currentTask = null;
        }
    }
}
