using System.Collections.Generic;
using Desktop_IPTV_Browser.Services;
using Desktop_IPTV_Browser.Models;

namespace Desktop_IPTV_Browser
{
    public static class GlobalData
    {
        public static List<XtreamChannel> XtreamChannels { get; set; } = new List<XtreamChannel>();
        public static List<IEPGData> XtreamEPGDataList { get; set; } = new List<IEPGData>();
        public static string? XtreamEPGData { get; set; }
    }
}
