using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV.Models
{
    public class XtreamEPGData : IEPGData
    {
        public string ChannelId { get; set; }
        public string ProgramTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Description { get; set; }
    }
}
