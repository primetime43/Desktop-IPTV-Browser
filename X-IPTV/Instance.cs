using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static X_IPTV.M3UPlaylist;

namespace X_IPTV
{
    public static class Instance
    {
        //User's login info to use throughout the program
        public static Current_User currentUser = new Current_User();

        //Contains the User_Info and Server_Info objects
        public static PlayerInfo PlayerInfo = null;

        //Contains the array of each channel's data. This uses action=get_live_streams; will contain the stream_id
        public static ChannelEntry[] ChannelsArray = null;

        //Contains the categories/groups for the channels
        public static ChannelGroups[] ChannelGroupsArray = null;

        //testing dictionary channel group: Key: category_id Value: Array of channels by channel num
        public static Dictionary<string, List<ChannelEntry>> categoryToChannelMap = new Dictionary<string, List<ChannelEntry>>();

        //key = xui_id, value = PlaylistData obj
        public static Dictionary<string, ChannelStreamData> playlistDataMap = null;

        public static string selectedCategory { get; set; }

        //testing rewrite stuff
        public static EpgListing individualChannelEPG_24HRS = null; //this can get an individual channel's entire 24hrs epg

        public static List<EpgListing> now_playingEPG_AllChannels = null; //this can get all channels currently playing epg

        public static Dictionary<string, List<EpgListing>> allChannelEPG_24HRS_Dict = new Dictionary<string, List<EpgListing>>(); //stores the channel id as key and the list is all 24hrs epg data for each channel

        // Property to store the M3U channels
        public static List<M3UChannel> M3UChannels { get; set; } = new List<M3UChannel>();

        public static List<M3UEPGData> M3UEPGDataList { get; set; } = new List<M3UEPGData>();

        public static List<M3UChannel> M3UChannelToEPGMap { get; set; } = new List<M3UChannel>();

        public static bool M3uChecked { get; set; }

        public static bool XstreamCodesChecked { get; set; }
    }
}
