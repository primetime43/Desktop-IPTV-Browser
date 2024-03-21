using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using X_IPTV.Models;
using static X_IPTV.M3UPlaylist;
using static X_IPTV.XtreamCodes;

namespace X_IPTV
{
    public static class Instance
    {
        #region XtreamCodes Data Storage
        //User's login info to use throughout the program
        public static Current_User currentUser = new Current_User();

        //Contains the User_Info and Server_Info objects
        public static PlayerInfo PlayerInfo = null;

        public static string allXtreamEpgData { get; set; }

        // Property to store the Xtream categories
        public static List<XtreamCategory> XtreamCategoryList { get; set; } = new List<XtreamCategory>();

        // Property to store the Xtream channels
        public static List<XtreamChannel> XtreamChannels { get; set; } = new List<XtreamChannel>();

        // Property to store the Xtream epg data
        public static List<IEPGData> XtreamEPGDataList { get; set; } = new List<IEPGData>();
        #endregion

        #region M3U Data Storage
        // Property to store the M3U channels
        public static List<M3UChannel> M3UChannels { get; set; } = new List<M3UChannel>();

        // Property to store the M3U categories
        public static List<M3UCategory> M3UCategoryList { get; set; } = new List<M3UCategory>();

        // Property to store the M3U epg data
        public static List<IEPGData> M3UEPGDataList { get; set; } = new List<IEPGData>();

        //check this
        public static List<M3UChannel> M3UChannelToEPGMap { get; set; } = new List<M3UChannel>();

        public static string allM3uEpgData { get; set; }
        #endregion

        #region Global helpers
        public static bool M3uChecked { get; set; }
        public static bool XtreamCodesChecked { get; set; }

        public static string selectedCategory { get; set; }

        public static string programVersion = "v3.1.0";

        public static bool ShouldUpdateOnInterval(DateTime currentTime) // make this a settings config eventually maybe?
        {
            return currentTime.Minute % 15 == 0 && currentTime.Second == 0;
        }
        #endregion
    }
}
