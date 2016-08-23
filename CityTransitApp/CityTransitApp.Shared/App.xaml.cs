using CityTransitApp.Common;
using CityTransitApp.NetPortableServicesImplementations;
using CityTransitServices;
using CityTransitApp.Common.Processes;
using PlannerComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransitBase;
using UserBase;
using UserBase.Interface;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using PlannerComponent.Interface;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace CityTransitApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        #region Application Components

        public static event EventHandler<object> ApplicationComponentChanged;
        private static void performApplicationComponentChanged(object sender, object component)
        {
            if (ApplicationComponentChanged != null)
                ApplicationComponentChanged(sender, component);
        }


        public static TransitBaseComponent TB { get { return Common.TB; } }
        public static IUserBase UB { get { return Common.UB; } }
        public static PlannerRuntimeComponent Planner { get { return ((CityTransitApp.PlannerComponentImplementation.PlannerComponentUniversalCpp)Common.Planner).ComposedComponent; } }
        public static Config Config { get { return Common.Config; } }
        public static ICommonServices Services { get { return Common.Services; } }
        public static CommonComponent Common { get; set; }

        #endregion

        public static AppInfo GetAppInfo() { return new AppInfo(); }
        
#if WINDOWS_PHONE_APP
        private static Dictionary<Type, Action<BackKeyHandlerToken>> backKeyActions = new Dictionary<Type, Action<BackKeyHandlerToken>>();
        public class BackKeyHandlerToken { public bool Handled; }
        public static void RegisterBackKeyHandler(Page page, Action<BackKeyHandlerToken> handlerAction)
        {
            backKeyActions.Add(page.GetType(), handlerAction);
        }
        public static void UnregisterBackKeyHandler(Page page)
        {
            backKeyActions.Remove(page.GetType());
        }
#endif

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (sender, e) =>
            {
                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame != null)
                {
                    Action<BackKeyHandlerToken> handlerAction;
                    if (backKeyActions.TryGetValue(rootFrame.CurrentSourcePageType, out handlerAction))
                    {
                        var token = new BackKeyHandlerToken();
                        handlerAction(token);
                        if (token.Handled)
                        {
                            e.Handled = true;
                            return;
                        }
                    }

                    if (rootFrame.CanGoBack)
                    {
                        e.Handled = true;
                        rootFrame.GoBack();
                    }
                    else
                    {
                        Exit();
                    }
                }
            };
#endif
        }

        public static int? SourceTileId { get; private set; }
        
#if !WINDOWS_PHONE_APP
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            Windows.UI.ApplicationSettings.SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        void OnCommandsRequested(Windows.UI.ApplicationSettings.SettingsPane sender, Windows.UI.ApplicationSettings.SettingsPaneCommandsRequestedEventArgs args)
        {
            var setting = new Windows.UI.ApplicationSettings.SettingsCommand("MySetting", "Alkalmazás beállításai", onHandleClick);
            args.Request.ApplicationCommands.Add(setting);
        }

        private void onHandleClick(IUICommand command)
        {
            new CityTransitApp.Flyouts.AppSettingsFlyout().Show();
        }
#endif

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Common = new CommonComponent(new WindowsUniversalServices(), new ComponentFactory());
            Common.ComposedComponentChanged += performApplicationComponentChanged;
            if (Common.TB.DatabaseExists)
                performApplicationComponentChanged(this, Common.TB);
            this.UnhandledException += App_UnhandledException;
#if WINDOWS_PHONE_APP
            if (e.TileId != null && e.TileId != "App")
            {
                SourceTileId = int.Parse(e.TileId);
                MainPage.NavigateToTile(SourceTileId.Value);
            }
#endif

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 10;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //new MessageDialog(e.Exception.Message).ShowAsync().AsTask().Wait();
            //unhandledException(sender, e).Wait();
        }

        private async Task unhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StorageFile errorFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("error.log");
            using (var stream = await errorFile.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("from " + sender);
                writer.WriteLine();
                Exception ex = e.Exception;
                while (ex != null)
                {
                    writer.WriteLine("type " + ex.GetType());
                    writer.WriteLine("message " + ex.Message);
                    writer.WriteLine(ex.StackTrace);
                    writer.WriteLine();
                    ex = ex.InnerException;
                }
            }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}