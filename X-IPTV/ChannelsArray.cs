﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV
{


    public class ChannelsArray
    {
        public ChannelEntry[] Entries { get; set; }
    }

    public class ChannelEntry
    {
        public int num { get; set; }
        public string name { get; set; }
        public string stream_type { get; set; }
        public int stream_id { get; set; }
        public string stream_icon { get; set; }
        public string epg_channel_id { get; set; }
        public string added { get; set; }
        public string category_id { get; set; }
        public string custom_sid { get; set; }
        public int tv_archive { get; set; }
        public string direct_source { get; set; }
        public object tv_archive_duration { get; set; }
    }

    public class PlaylistData
    {
        public string xui_id { get; set; }
        public string stream_url { get; set; }
        
        //testing below
        public string tvg_name { get; set; }
        public string tvg_logo { get; set; }
        //public string group_title { get; set; }
        public string tvg_id { get; set; }
    }
}
