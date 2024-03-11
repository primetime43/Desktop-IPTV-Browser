using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV.Models
{

    public class PlayerInfo
    {
        public User_Info user_info { get; set; }
        public Server_Info server_info { get; set; }
    }

    public class User_Info
    {
        public string username { get; set; }
        public string password { get; set; }
        public string message { get; set; }
        public int auth { get; set; }
        public string status { get; set; }
        public string exp_date { get; set; }
        public string is_trial { get; set; }
        public string active_cons { get; set; }
        public string created_at { get; set; }
        public string max_connections { get; set; }
        public string[] allowed_output_formats { get; set; }
        public string port { get; internal set; }
        public string server { get; internal set; }
    }

    public class Server_Info
    {
        public string url { get; set; }
        public string port { get; set; }
        public string https_port { get; set; }
        public string server_protocol { get; set; }
        public string rtmp_port { get; set; }
        public string timezone { get; set; }
        public int timestamp_now { get; set; }
        public string time_now { get; set; }
        public bool process { get; set; }
    }

}
