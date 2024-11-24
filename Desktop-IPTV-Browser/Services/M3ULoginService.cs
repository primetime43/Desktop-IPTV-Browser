using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Desktop_IPTV_Browser.Services
{
    public class M3ULoginService
    {
        private readonly string _saveDir;

        public M3ULoginService()
        {
            // Define the directory to store M3U login data
            _saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "M3ULogins");
        }

        /// <summary>
        /// Saves M3U login data to a file.
        /// </summary>
        public void SaveM3UData(string fileName, string m3uUrl, string epgUrl)
        {
            if (!Directory.Exists(_saveDir))
                Directory.CreateDirectory(_saveDir);

            string filePath = Path.Combine(_saveDir, $"{fileName}.json");
            var loginData = new
            {
                M3UUrl = m3uUrl,
                EPGUrl = epgUrl
            };

            File.WriteAllText(filePath, JObject.FromObject(loginData).ToString());
        }

        /// <summary>
        /// Loads M3U login data from a file.
        /// </summary>
        public JObject LoadM3UData(string fileName)
        {
            string filePath = Path.Combine(_saveDir, $"{fileName}.json");
            if (File.Exists(filePath))
            {
                return JObject.Parse(File.ReadAllText(filePath));
            }

            return null;
        }

        /// <summary>
        /// Retrieves a list of saved M3U logins.
        /// </summary>
        public IEnumerable<string> GetSavedLogins()
        {
            if (!Directory.Exists(_saveDir))
                Directory.CreateDirectory(_saveDir);

            var files = Directory.GetFiles(_saveDir, "*.json");
            foreach (var file in files)
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }
    }
}
