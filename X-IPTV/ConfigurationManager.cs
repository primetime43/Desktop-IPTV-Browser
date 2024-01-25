using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace X_IPTV
{
    class ConfigurationManager
    {
        private static JObject _configuration;

        public static void InitializeConfiguration()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _configuration = JObject.Parse(json);
            }
        }

        public static string GetSetting(string key)
        {
            return _configuration?[key]?.ToString();
        }
    }
}