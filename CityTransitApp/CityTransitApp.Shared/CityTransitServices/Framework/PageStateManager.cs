using CityTransitApp.CityTransitServices.Tools;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CityTransitApp
{
    public class PageStateManager : PageStateManagerBase
    {
        private PageState state;
        protected override PageState State { get { return state; } }

        public event Func<object> SaveState;
        public event Action<object> RestoreState;
        public event Action<object> InitializeState;

        public PageStateManager(Page page, bool isInLoop = false)
        {
            checkFrame(page.Frame);
            this.state = new PageState
            {
                PageType = page.GetType()
            };
            this.Page = page;
            Page.NavigationCacheMode = isInLoop ? NavigationCacheMode.Disabled : NavigationCacheMode.Required;
        }

        private static void checkFrame(Frame frame)
        {
            if (PageStateStack.Frame == null)
                PageStateStack.Frame = frame;
            else if (PageStateStack.Frame != frame)
                throw new InvalidOperationException("Multiple frames in the application.");
        }
#if WINDOWS_PHONE_APP
        public static void SetStatusBarColor(FrameworkElement element, Color? bgColor = null)
        {
            if (element.RequestedTheme == ElementTheme.Light)
                StatusBar.GetForCurrentView().ForegroundColor = Colors.Black;
            else if (element.RequestedTheme == ElementTheme.Dark)
                StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            //else StatusBar.GetForCurrentView().ForegroundColor = null;

            if (bgColor != null)
            {
                StatusBar.GetForCurrentView().BackgroundColor = bgColor;
                StatusBar.GetForCurrentView().BackgroundOpacity = 1.0;
            }
        }
#endif

        public override void OnNavigatedTo(NavigationEventArgs e, bool setStatusBar = true)
        {
#if WINDOWS_PHONE_APP
            if (setStatusBar) SetStatusBarColor((FrameworkElement)e.Content);
#endif

            if (e.NavigationMode == NavigationMode.New)
            {
                PageStateStack.Push(this.State);
                if (InitializeState != null)
                    InitializeState(e.Parameter);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                var currentState = PageStateStack.Top();
                if (State.PageType != currentState.PageType)
                    throw new InvalidOperationException();

                if (currentState != this.State)
                {
                    this.state = currentState;
                    State.UpdateTasks(this.Page);
                    PageStateStack.Pop();
                    PageStateStack.Push(State);
                    if (RestoreState != null)
                        RestoreState(State.StateObject);
                }
                State.ResumeTasks();
            }
            else throw new NotImplementedException("Not allowed navigation mode:" + e.NavigationMode);
        }

        public override void OnNavigatedFrom(NavigationMode mode, Type sourceType)
        {
            State.StopTasks();

            if (mode == NavigationMode.Back)
            {
                Page.NavigationCacheMode = NavigationCacheMode.Disabled;
                PageStateStack.Pop();
            }
            else if (mode == NavigationMode.New)
            {
                if (SaveState != null)
                    State.StateObject = SaveState();

                //var cachedPage = PageStateStack.SameCachedPage(sourceType);
                //if (cachedPage != null)
                //{
                //    cachedPage.Page.NavigationCacheMode = NavigationCacheMode.Disabled;
                //}
            }
            else throw new NotImplementedException("Not allowed navigation mode:" + mode);
            
#if WINDOWS_PHONE_APP
            StatusBar.GetForCurrentView().ForegroundColor = null;
            StatusBar.GetForCurrentView().BackgroundOpacity = 0.0;
#endif
            Logging.Save();
        }

        public PageState GetState()
        {
            return State;
        }
    }
}
