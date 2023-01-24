using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV
{
    public static class Instance
    {
        //Contains the User_Info and Server_Info objects
        public static PlayerInfo PlayerInfo = null;

        //Contains the array of each channel's data. This uses action=get_live_streams; will contain the stream_id
        public static ChannelEntry[] ChannelsArray = null;

        //Contains the categories/groups for the channels
        public static ChannelGroups[] ChannelGroupsArray = null;

        //key = xui_id, value = PlaylistData obj
        public static Dictionary<string, PlaylistEPGData> playlistDataMap = null;

        public static string selectedCategory { get; set; }

        //testing rewrite stuff
        public static EpgListing individualChannelEPG_24HRS = null; //this can get an individual channel's entire 24hrs epg

        public static List<EpgListing> now_playingEPG_AllChannels = null; //this can get all channels currently playing epg

        public static Dictionary<string, List<EpgListing>> allChannelEPG_24HRS_Dict = new Dictionary<string, List<EpgListing>>(); //stores the channel id as key and the list is all 24hrs epg data for each channel

    }
}
