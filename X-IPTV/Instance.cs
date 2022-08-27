using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV
{
    public static class Instance
    {
        public static PlayerInfo PlayerInfo = null;

        //Contains ChannelEntry obj data
        public static ChannelEntry[] ChannelsArray = null;

        //key = xui_id, value = PlaylistData obj
        public static Dictionary<string, PlaylistData> playlistDataMap = null;

        //key = category name, value = Dictionary (key = xui_id, value = PlaylistData obj)
        public static Dictionary<string, Dictionary<string, PlaylistData>> categories = null;

        public static string selectedCategory { get; set; }
    }
}
