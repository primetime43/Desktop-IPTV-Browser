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
        private static readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.json");

        public static void InitializeConfiguration()
        {
            if (!File.Exists(filePath))
            {
                // If the configuration file doesn't exist, create it with default values
                _configuration = new JObject
                {
                    ["vlcLocationPath"] = "", // setting for vlc player
                    ["genericPlayerPath"] = "", // setting for any generic
                    ["defaultPlayer"] = "vlc", // Default player is VLC
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
                _configuration["defaultPlayer"] = string.IsNullOrEmpty(_configuration["defaultPlayer"]?.ToString()) ? "vlc" : _configuration["defaultPlayer"]; // Default to VLC if not set

                // Save any updates back to the file
                File.WriteAllText(filePath, _configuration.ToString());
            }
        }

        public static string GetConfigFilePath()
        {
            return filePath;
        }

        public static string GetSetting(string key)
        {
            return _configuration?[key]?.ToString();
        }

        public static string GetDefaultPlayer()
        {
            return GetSetting("defaultPlayer") ?? "vlc"; // Default to VLC if the setting is missing
        }

        public static void SetDefaultPlayer(string player)
        {
            if (player != "vlc" && player != "generic")
            {
                throw new ArgumentException("Invalid player type. Only 'vlc' or 'generic' are allowed.");
            }

            UpdateSetting("defaultPlayer", player);
        }

        // Update a specific setting in the configuration
        public static void UpdateSetting(string key, string value)
        {
            if (_configuration == null)
            {
                InitializeConfiguration();
            }

            // Update or add the setting
            _configuration[key] = value;
            File.WriteAllText(filePath, _configuration.ToString());
        }

        // Generalized method for retrieving any player path
        public static string GetPlayerPath(string key)
        {
            string playerPath = GetSetting(key);

            if (!string.IsNullOrEmpty(playerPath) && File.Exists(playerPath))
            {
                return playerPath;
            }
            else
            {
                // If looking for VLC path, try to find it in the registry
                if (key == "vlcLocationPath")
                {
                    playerPath = FindVLCPath();
                    if (!string.IsNullOrEmpty(playerPath))
                    {
                        UpdateSetting("vlcLocationPath", playerPath);
                        return playerPath;
                    }
                }

                // For any other or missing path, prompt user to select
                playerPath = PromptUserForPlayerPath();
                if (playerPath != null)
                {
                    UpdateSetting(key, playerPath);
                }
                return playerPath;
            }
        }

        // Prompts the user to select a video player path
        private static string PromptUserForPlayerPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe",
                Title = "Select a Video Player"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        // Method to find VLC installation path
        public static string FindVLCPath()
        {
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
