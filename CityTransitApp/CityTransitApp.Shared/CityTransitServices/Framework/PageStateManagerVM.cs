using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CityTransitApp
{
    public class PageStateManager<TViewModel> : PageStateManagerBase where TViewModel : ViewModel, new()
    {
        PageStateManager innerManager;
        bool hasCommandBar;

        public Action<object> InitializeState;
        public Action SaveState;
        public Action RestoreState;

        public TViewModel ViewModel { get; private set; }

        public PageStateManager(Page page, bool hasCommandBar = false)
        {
            this.Page = page;
            this.hasCommandBar = hasCommandBar;
            this.innerManager = new PageStateManager(page, true);
            innerManager.InitializeState += innerManager_InitializeState;
            innerManager.SaveState += innerManager_SaveState;
            innerManager.RestoreState += innerManager_RestoreState;
        }

        void innerManager_RestoreState(object obj)
        {
            ViewModel = (TViewModel)obj;
            if (Page.DataContext != ViewModel)
            {
                Page.DataContext = ViewModel;
                if (hasCommandBar)
                    Page.BottomAppBar.DataContext = ViewModel;
            }
            if (RestoreState != null)
                RestoreState();
        }

        object innerManager_SaveState()
        {
            if (SaveState != null)
                SaveState();
            return ViewModel;
        }

        void innerManager_InitializeState(object initialData)
        {
            ViewModel = new TViewModel();
            ViewModel.Initialize(initialData);
            foreach (var task in ViewModel.TasksToSchedule)
                ScheduleTaskEveryMinute(task);
            Page.DataContext = ViewModel;
            if (hasCommandBar)
                Page.BottomAppBar.DataContext = ViewModel;
            if (InitializeState != null)
                InitializeState(initialData);
        }

        public override void OnNavigatedTo(NavigationEventArgs e, bool setStatusBar = true)
        {
            innerManager.OnNavigatedTo(e, setStatusBar);
        }

        public override void OnNavigatedFrom(NavigationMode mode, Type sourceType)
        {
            innerManager.OnNavigatedFrom(mode, sourceType);
        }

        protected override PageState State
        {
            get { return innerManager.GetState(); }
        }
    }
}
