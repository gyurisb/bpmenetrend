using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;

namespace CityTransitApp.CityTransitElements.BaseElements
{
    public class AsyncStoryboard
    {
        private Storyboard storyboard;
        private bool returnValue;
        private Task<bool> task;
        private bool started = false;
        public AsyncStoryboard(Storyboard storyboard)
        {
            this.storyboard = storyboard;
            this.task = new Task<bool>(() => returnValue);
            storyboard.Completed += storyboard_Completed;
        }

        private void storyboard_Completed(object sender, object e)
        {
            if (!started)
            {
                returnValue = true;
                task.Start();
                started = true;
            }
        }

        public void Pause()
        {
            if (!started)
            {
                storyboard.Pause();
                returnValue = false;
                task.Start();
                started = true;
            }
        }

        public Task<bool> AsTask()
        {
            return task;
        }
    }
}
