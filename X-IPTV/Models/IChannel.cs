using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV.Models
{
    public interface IChannel
    {
        string DisplayName { get; }
        string IconUrl { get; }
        string Title { get; }
        string Description { get; }
    }
}
