using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using CityTransitApp.Common.Processes;
using CityTransitApp.WPSilverlight.Resources;

namespace CityTransitApp.WPSilverlight.Dialogs
{
    public partial class DownloadDBDialog : UserControl
    {
        public DownloadDBDialog()
        {
            InitializeComponent();
        }

        public event EventHandler<DatabaseDownloadResult> DownloadDone;

        private async void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            var downloadRes = await DatabaseDownloader.RunAsync(progress =>
                {
                    ProgressTextBlock.Text = progress.Message;
                    ProgressBar.Value = progress.Percentage;
                }
            );
            if (DownloadDone != null)
                DownloadDone(this, downloadRes);
        }

        public static Task<DatabaseDownloadResult> Show()
        {
            DatabaseDownloadResult result = DatabaseDownloadResult.NoAccess;
            var resultTask = new Task<DatabaseDownloadResult>(() => result);
            var content = new DownloadDBDialog();
            var messageBoxDownload = new CustomMessageBox
            {
                Message = AppResources.MainDownloadProgress,
                Content = content
            };
            content.DownloadDone += (sender, downloadResult) =>
            {
                messageBoxDownload.Dismiss();
                result = downloadResult;
                resultTask.Start();
            };
            messageBoxDownload.Show();
            return resultTask;
        }
    }
}
