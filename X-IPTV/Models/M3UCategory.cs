using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV.Models
{
    public class M3UCategory
    {
        [JsonProperty("group-title")]
        public string CategoryName { get; set; }
    }
}
