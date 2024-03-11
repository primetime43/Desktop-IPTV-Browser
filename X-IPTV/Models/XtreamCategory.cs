using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV.Models
{
    public class XtreamCategory
    {
        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("category_name")]
        public string CategoryName { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }

        public string CategoryNameId => $"{CategoryName} - {CategoryId}";
    }
}
