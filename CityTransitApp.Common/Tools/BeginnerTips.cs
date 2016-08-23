using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CityTransitServices.Tools
{
    public class BeginnerTips
    {

        public static void CheckTileTip()
        {
            if (!CommonComponent.Current.Services.Settings.ContainsKey("TileTip"))
            {
                try
                {
                    string message = CommonComponent.Current.Services.Resources.LocalizedStringOf("TipLiveTile");
                    CommonComponent.Current.Services.MessageBox.Show(message);
                    CommonComponent.Current.Services.Settings["TileTip"] = true;
                    //CommonComponent.Current.Services.Settings.Save();
                }
                catch (Exception) { }
            }
        }


        public static void CheckFavoriteTip()
        {
            if (!CommonComponent.Current.Services.Settings.ContainsKey("FavoriteTip"))
            {
                string message = CommonComponent.Current.Services.Resources.LocalizedStringOf("TipAddFavs");
                CommonComponent.Current.Services.MessageBox.Show(message);
                CommonComponent.Current.Services.Settings["FavoriteTip"] = true;
                //CommonComponent.Current.Services.Settings.Save();
            }
        }
    }
}
