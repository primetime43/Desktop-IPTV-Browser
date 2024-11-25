using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_IPTV_Browser.Models
{
    public class Current_User
    {
        public string username { get; set; }
        public string password { get; set; }
        public string server { get; set; }
        public string port { get; set; }
        public bool useHttps { get; set; }
    }
}
