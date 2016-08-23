using CityTransitElements.Controllers;
using CityTransitServices;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TransitBase.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityTransitApp.CityTransitElements.PageElements.MapMarkers
{
    public sealed partial class PlanItemPopup : UserControl
    {
        private IMapControl mapParent;

        public PlanItemPopup(IMapControl mapParent)
        {
            InitializeComponent();
            this.mapParent = mapParent;
            Loaded += PlanItemPopup_Loaded;
        }

        void PlanItemPopup_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutRoot.MaxWidth = mapParent.Element.ActualWidth * 0.8;
        }

        public void SetModel(PlanItemPopupModel model)
        {
            this.DataContext = model;
        }
    }
    public enum PlanItemPopupType { Start, MidStart, MidFinish, Finish };

    public class PlanItemPopupModel
    {
        public Route Route { get; set; }
        public Stop Stop { get; set; }
        public TimeSpan Time { get; set; }
        public PlanItemPopupType Type { get; set; }
        public int StopCount { get; set; }

        public string CurrentTime { get { return (DateTime.Today + Time).ToString("t"); } }
        public string HelpLine1
        {
            get
            {
                switch (Type)
                {
                    case PlanItemPopupType.Start: return App.Common.Services.Resources.LocalizedStringOf("PlanItemStart1");
                    case PlanItemPopupType.MidStart: return App.Common.Services.Resources.LocalizedStringOf("PlanItemMidStart1");
                    case PlanItemPopupType.MidFinish:
                    case PlanItemPopupType.Finish: return App.Common.Services.Resources.LocalizedStringOf("PlanItemFinish1");
                    default: throw new ArgumentException();
                }
            }
        }
        public string HelpLine2
        {
            get
            {
                switch (Type)
                {
                    case PlanItemPopupType.Start:
                    case PlanItemPopupType.MidStart: return StringFactory.Format(App.Common.Services.Resources.LocalizedStringOf("PlanItemStart2"), false, StopCount);
                    case PlanItemPopupType.MidFinish: return App.Common.Services.Resources.LocalizedStringOf("PlanItemMidFinish2");
                    case PlanItemPopupType.Finish: return App.Common.Services.Resources.LocalizedStringOf("PlanItemFinish2");
                    default: throw new ArgumentException();
                }
            }
        }
    }
}
