using CityTransitApp.CityTransitElements.Dialogs;
using CityTransitApp.ContentPages;
using CityTransitApp.Common.ViewModels;
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
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CityTransitApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a ContentFrame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        private Brush originalHeaderBrush;
        private Brush originalHeaderFontBrush;

        public MainPage()
        {
            Current = this;
            this.InitializeComponent();
            this.originalHeaderBrush = Header.Background;
            this.originalHeaderFontBrush = HeaderText.Foreground;
            ContentFrame.Navigating += (sender, args) => { ResetHeaderBrush(); ResetHeaderFontBrush(); };
            //CategorySelector.TreeModel = App.Config.CategoryTree;
            //ContentFrame.Height = App.GetAppInfo().GetScreenHeight() - 52;
        }

        public void SetHeaderBrush(Brush brush)
        {
            Header.Background = brush;
        }
        public void ResetHeaderBrush()
        {
            Header.Background = originalHeaderBrush;
        }
        public void SetHeaderFontBrush(Brush brush)
        {
            HeaderText.Foreground = brush;
        }
        public void ResetHeaderFontBrush()
        {
            HeaderText.Foreground = originalHeaderFontBrush;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                //adatbázis inicializálása első induláskor
                if (!App.TB.DatabaseExists || AppFields.ForceUpdate)
                {
                    await DatabaseDownloader.RunAsync();
                }

                ContentFrame.Navigate(typeof(DefaultPage));

                //frissítés keresése
                var checkUpdateResult = await UpdateMonitor.CheckUpdate();
                if (checkUpdateResult == UpdateMonitor.Result.Found)
                {
                    var dialog = new MessageDialog("A menetrend adatbázis elavult lehet.", "Frissítés elérhető");
                    dialog.Commands.Add(new UICommand { Label = "Frissítés most", Id = true });
                    dialog.Commands.Add(new UICommand { Label = "Emlékeztessen később", Id = false });
                    var res = await dialog.ShowAsync();
                    if ((bool)res.Id == true)
                    {
                        await DatabaseDownloader.RunAsync();
                        var finalDialog = new MessageDialog("A frissítés kész, kérem indítsa újra az alkalmazást.");
                        finalDialog.Commands.Add(new UICommand { Label = "Rendben", Id = true });
                        await finalDialog.ShowAsync();
                        throw new Exception("A frissítés kész, kérem indítsa újra az alkalmazást!");
                    }
                }

                //if (await ApplicationData.Current.LocalFolder.ContainsFileAsync("error.log"))
                //{
                //    var logFile = await ApplicationData.Current.LocalFolder.GetFileAsync("error.log");
                //    string logData;
                //    using (var logStream = await logFile.OpenStreamForReadAsync())
                //    {
                //        logData = new StreamReader(logStream).ReadToEnd();
                //    }
                //    new MessageDialog(logData).ShowAsync();
                //    await logFile.DeleteAsync();
                //}
            }
        }

        private void SearchBox_QueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
        {
            //processQuery(args.QueryText);
        }

        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            processQuery(args.QueryText);
        }
        
        private void processQuery(string text)
        {
            if (ContentFrame.Content is ListPage)
            {
                if (text != "")
                    ((ListPage)ContentFrame.Content).SetText(text);
                else
                    ContentFrame.GoBack();
            }
            else
            {
                if (text != "")
                    ContentFrame.Navigate(typeof(ListPage), text);
            }
        }
    }
}
