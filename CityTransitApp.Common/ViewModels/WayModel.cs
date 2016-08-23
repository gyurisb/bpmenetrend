using CityTransitApp.Common;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.BusinessLogic;
using TransitBase.Entities;

namespace CityTransitApp.Common.ViewModels
{
    public class WayModel : List<EntryModel>
    {
        private static ICommonServices Services { get { return CommonComponent.Current.Services; } }

        public WayModel(Way way, StopGroup source, StopGroup target, DateTime startDateTime)
            : base(way.Select(entry => new EntryModel(entry, startDateTime.Date)))
        {
            Way = way;
            EndStop = target;
            StartStop = source;
            StartDateTime = startDateTime;

            Colors = way.Select(e => e.Route.RouteGroup.BgColor).Distinct().ToArray();

            if (way.TotalTransferCount >= 0)
            {
                RoutesText = String.Join(" " + (char)8594 + " ", Way.Select(x => x.Route.RouteGroup.Name));
            }
            else
            {
                RoutesText = Services.Resources.LocalizedStringOf("PlanningWalk");
            }
        }

        public Way Way { get; private set; }
        public StopGroup StartStop { get; private set; }
        public StopGroup EndStop { get; private set; }
        public DateTime StartDateTime { get; private set; }
        public bool HasHeader { get; set; }

        public string WalkAfterText { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanDetailsWalk"), Way.LastWalkDistance); } }
        public bool IsWalkAfterVisible { get { return Way.LastWalkDistance > 0; } }
        public string TimeLabel { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanTimeLabel"), (int)Way.TotalTime.TotalMinutes); } }
        public string WalkLabel { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanWalkLabel"), StringFactory.LocalizeDistance(Way.TotalWalkDistance)); } }
        public string TransferLabel { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanTransferLabel"), Way.TotalTransferCount); } }

        public int TransferCount { get { return Math.Max(0, Way.TotalTransferCount); } }
        public String TimeText { get { return Way.TotalTime.TotalMinutes.ToString(); } }
        public string WalkText { get { return "" + StringFactory.LocalizeDistance(Way.TotalWalkDistance); } }

        public string RoutesText { get; private set; }
        public uint[] Colors { get; private set; }

        public string StartTimeTextLong { get { return (StartDateTime.Date + Way.DepartTime).ToString("t"); } }
        public string EndTimeTextLong { get { return (StartDateTime.Date + Way.ArrivalTime).ToString("t"); } }
        public string StartTimeText { get { return (StartDateTime.Date + Way.DepartTime).ShortTimeString(); } }
        public string EndTimeText { get { return (StartDateTime.Date + Way.ArrivalTime).ShortTimeString(); } }
    }

    public class EntryModel
    {
        private static ICommonServices Services { get { return CommonComponent.Current.Services; } }

        private DateTime date;
        public EntryModel(Way.Entry entry, DateTime date)
        {
            this.Entry = entry;
            this.date = date;
        }

        public Way.Entry Entry { get; private set; }
        public RouteGroup RouteGroup { get { return Entry.Route.RouteGroup; } }

        public string WalkText { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanDetailsWalk"), StringFactory.LocalizeDistance(Entry.WalkBeforeMeters)); } }
        public string WaitText { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanDetailsWait"), Entry.WaitMinutes); } }
        //TODO trip.Name megjelenítése
        public string RouteText { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanDetailsRoute"), false, Entry.Route.RouteGroup.Name, Entry.Route.Name); } }
        public string CountText { get { return StringFactory.Format(Services.Resources.LocalizedStringOf("PlanDetailsTrip"), Entry.StopCount); } }

        public bool IsWalkVisible { get { return Entry.WalkBeforeMeters > 0; } }
        public bool IsWaitVisible { get { return Entry.WaitMinutes > 0; } }

        public string StartTimeText { get { return (date + Entry.StartTime).ShortTimeString(); } }
        public string EndTimeText { get { return (date + Entry.EndTime).ShortTimeString(); } }

        public bool IsMetro { get { return Entry.Route.RouteGroup.Type == RouteType.Metro; } }
    }
}
