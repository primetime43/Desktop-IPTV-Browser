using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace X_IPTV.Utilities
{
    class ConfigurationManager
    {
        private static JObject _configuration;
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.json");

        public static void InitializeConfiguration()
        {
            if (!File.Exists(filePath))
            {
                // If the configuration file doesn't exist, create it with default values
                _configuration = new JObject
                {
                    ["vlcLocationPath"] = "",
                    ["usersFolderPath"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XtreamUsers"),
                    ["M3UFolderPath"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "M3U"),
                    ["epgDataFolderPath"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EPGData"),
                    ["lastEpgDataLoadTime"] = ""
                };

                // Save the new configuration to file
                File.WriteAllText(filePath, _configuration.ToString());
            }
            else
            {
                // Read and parse the existing JSON file
                string json = File.ReadAllText(filePath);
                _configuration = JObject.Parse(json);

                // Ensure default values if not set
                _configuration["usersFolderPath"] = string.IsNullOrEmpty(_configuration["usersFolderPath"]?.ToString()) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XtreamUsers") : _configuration["usersFolderPath"];
                _configuration["M3UFolderPath"] = string.IsNullOrEmpty(_configuration["M3UFolderPath"]?.ToString()) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "M3U") : _configuration["M3UFolderPath"];
                _configuration["epgDataFolderPath"] = string.IsNullOrEmpty(_configuration["epgDataFolderPath"]?.ToString()) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EPGData") : _configuration["epgDataFolderPath"];

                // Save any updates back to the file
                File.WriteAllText(filePath, _configuration.ToString());
            }
        }

        public static string GetSetting(string key)
        {
            return _configuration?[key]?.ToString();
        }

        // Update a specific setting in the configuration
        public static void UpdateSetting(string key, string value)
        {
            // Check if _configuration is null and initialize it if necessary
            if (_configuration == null)
            {
                InitializeConfiguration();
            }

            // Update or add the setting
            _configuration[key] = value;

            // Save the updated configuration back to file
            File.WriteAllText(filePath, _configuration.ToString());
        }

        // Dedicated method for VLC Path logic
        public static string GetVLCPath()
        {
            // Attempt to get the path from the configuration file
            string vlcPath = GetSetting("vlcLocationPath");

            // Check if the path is not null/empty and if the file exists
            if (!string.IsNullOrEmpty(vlcPath) && File.Exists(vlcPath))
            {
                return vlcPath;
            }
            else
            {
                // Path is invalid or not set, find and update the path
                vlcPath = FindVLCPath();
                if (vlcPath != null)
                {
                    // Update the configuration with the found path
                    _configuration["vlcLocationPath"] = vlcPath;
                    File.WriteAllText(filePath, _configuration.ToString());
                }
                return vlcPath;
            }
        }

        // Method to find VLC installation path
        public static string FindVLCPath()
        {
            // Registry keys to check
            string[] registryKeys = new string[]
            {
                @"SOFTWARE\VideoLAN\VLC",
                @"SOFTWARE\WOW6432Node\VideoLAN\VLC"
            };

            foreach (var keyPath in registryKeys)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        var installDir = key.GetValue("InstallDir") as string;
                        if (!string.IsNullOrEmpty(installDir))
                        {
                            return Path.Combine(installDir, "vlc.exe");
                        }
                    }
                }
            }

            return null; // VLC not found
        }
    }
}