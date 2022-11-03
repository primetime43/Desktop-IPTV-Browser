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

        //use a map with tvg_id from get.php call as id rather than xui_id
        //key: tvg_uid (AmericanHeroesChannel.us), value:PlaylistData Obj
        public static Dictionary<string, PlaylistData> testMap = null;

        public static EPGData tempTestEPG = null;

        public static string selectedCategory { get; set; }
    }
}
