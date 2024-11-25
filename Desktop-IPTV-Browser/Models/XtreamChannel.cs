using Newtonsoft.Json;

namespace Desktop_IPTV_Browser.Models
{
    public class XtreamChannel : IChannel
    {
        // JSON Properties for deserialization
        [JsonProperty("num")]
        public string ChannelNumber { get; set; }

        [JsonProperty("epg_channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("name")]
        public string ChannelName { get; set; }

        [JsonProperty("stream_icon")]
        public string LogoUrl { get; set; }

        [JsonProperty("stream_id")]
        public int StreamId { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; } // Single category ID

        [JsonProperty("category_ids")]
        public List<int> CategoryIds { get; set; } // List of category IDs (some channels belong to more than one category)

        [JsonProperty("stream_type")]
        public string StreamType { get; set; }

        // Additional Properties
        public string CategoryName { get; set; }
        public IEPGData EPGData { get; set; }

        public string DisplayName => ChannelName;
        public string IconUrl => LogoUrl;
        public string Title => EPGData?.ProgramTitle;
        public string Description => EPGData?.Description;

        public string FormattedTimeRange
        {
            get
            {
                if (EPGData != null)
                {
                    return $"{EPGData.StartTime:hh:mm tt} - {EPGData.EndTime:hh:mm tt}";
                }
                return string.Empty;
            }
        }

        // Stream URL construction logic
        private static string _server;
        private static string _port;
        private static string _user;
        private static string _pass;
        private static bool _useHttps;

        public string StreamUrl
        {
            get
            {
                return $"{(_useHttps ? "https" : "http")}://{_server}:{_port}/{_user}/{_pass}/{StreamId}.ts";
            }
        }

        // Initialization logic if required
        private bool isInitialized = false;

        public void Initialize()
        {

        }
    }
}
