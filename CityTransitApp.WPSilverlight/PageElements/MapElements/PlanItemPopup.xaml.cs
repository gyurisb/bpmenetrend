using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TransitBase.Entities;
using CityTransitApp.WPSilverlight.Resources;
using CityTransitServices.Tools;

namespace CityTransitApp.WPSilverlight.PageElements.MapElements
{
    public partial class PlanItemPopup : UserControl
    {
        public PlanItemPopup()
        {
            InitializeComponent();
            LayoutRoot.MaxWidth = Application.Current.Host.Content.ActualWidth * 0.8;
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
                    case PlanItemPopupType.Start: return AppResources.PlanItemStart1;
                    case PlanItemPopupType.MidStart: return AppResources.PlanItemMidStart1;
                    case PlanItemPopupType.MidFinish:
                    case PlanItemPopupType.Finish: return AppResources.PlanItemFinish1;
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
                    case PlanItemPopupType.MidStart: return StringFactory.Format(AppResources.PlanItemStart2, false, StopCount);
                    case PlanItemPopupType.MidFinish: return AppResources.PlanItemMidFinish2;
                    case PlanItemPopupType.Finish: return AppResources.PlanItemFinish2;
                    default: throw new ArgumentException();
                }
            }
        }
    }
}
