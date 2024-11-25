using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_IPTV_Browser.Models
{
    public interface IEPGData
    {
        string ChannelId { get; set; }
        string ProgramTitle { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        string Description { get; set; }
    }
}
