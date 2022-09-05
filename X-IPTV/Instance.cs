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

        //Contains the array of each channel's data
        public static ChannelEntry[] ChannelsArray = null;

        //Contains the categories/groups for the channels
        public static ChannelGroups[] ChannelGroupsArray = null;

        //key = xui_id, value = PlaylistData obj
        public static Dictionary<string, PlaylistData> playlistDataMap = null;

        public static EPGData tempTestEPG = null;

        public static string selectedCategory { get; set; }
    }
}
