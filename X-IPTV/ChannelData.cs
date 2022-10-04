using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV
{


    /*public class ChannelsArray
    {
        public ChannelEntry[] channelData { get; set; }
    }*/

    //Use this one for most data info, missing stream_url, which is why the PlaylistData obj is needed 
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

        //epg data test stuff
        //non attributes
        [JsonProperty("title")]
        public string title { get; set; }
        [JsonProperty("desc")]
        public string desc { get; set; }

    }

    //Need this for stream_url
    public class PlaylistData
    {
        public string xui_id { get; set; }
        public string stream_url { get; set; }
        
        public string tvg_name { get; set; }
        public string tvg_logo { get; set; }
        public string group_title { get; set; }
        public string tvg_id { get; set; }
    }

    public class ChannelGroups
    {
        public string category_id { get; set; }
        public string category_name { get; set; }
        public int parent_id { get; set; }
    }

    public class EPGData
    {
        public Channel channelEPG { get; set; }
        public Programme programmeEPG { get; set; }
    }

    public class Channel
    {
        //attribute
        [JsonProperty("@id")]
        public string id { get; set; }

        //non attributes
        [JsonProperty("@display-name")]
        public string display_name { get; set; }

        [JsonProperty("icon")]
        public Icon icon { get; set; }
    }
    public class Icon
    {
        [JsonProperty("@src")]
        public string Src { get; set; }
    }

    public class Programme
    {
        //attributes
        [JsonProperty("@start")]
        public string start { get; set; }
        [JsonProperty("@stop")]
        public string stop { get; set; }
        [JsonProperty("@start_timestamp")]
        public string start_timestamp { get; set; }
        [JsonProperty("@stop_timestamp")]
        public string stop_timestamp { get; set; }
        [JsonProperty("@channel")]
        public string channel { get; set; }

        //non attributes
        [JsonProperty("title")]
        public string title { get; set;  }
        [JsonProperty("desc")]
        public string desc { get; set; }
    }
}
