using CityTransitApp.Common.Processes;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.Dialogs
{
    public sealed partial class DownloadDBDialog : UserControl
    {
        public DownloadDBDialog()
        {
            this.InitializeComponent();
        }

        private async void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async Task<DatabaseDownloadResult> downloadAsync()
        {
            return await DatabaseDownloader.RunAsync(progress =>
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                {
                    ProgressTextBlock.Text = progress.Message;
                    ProgressBar.Value = progress.Percentage;
                });
            });
        }

        public async Task<DatabaseDownloadResult> ShowAsync()
        {
#if WINDOWS_PHONE_APP
            var messageBoxDownload = new ContentDialog
            {
                Title = App.Common.Services.Resources.LocalizedStringOf("MainDownloadProgress"),
                Content = this,
                Foreground = (Brush)App.Current.Resources["ApplicationForegroundThemeBrush"]
            };
            messageBoxDownload.ShowAsync();
            var result = await downloadAsync();
            messageBoxDownload.Hide();
            return result;
#endif
            var res = await downloadAsync();
            new MessageDialog("Adatbázis frissítése készen.").ShowAsync();
            return res;
        }

        public static async Task<DatabaseDownloadResult> Show()
        {
            return await new DownloadDBDialog().ShowAsync();
        }
    }
}
