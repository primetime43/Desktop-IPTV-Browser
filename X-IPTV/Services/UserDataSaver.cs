using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace X_IPTV.Service
{
    public class UserDataSaver
    {
        public class User
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Server { get; set; }
            public string Port { get; set; }
        }

        public class M3UData
        {
            public string PlaylistURL { get; set; }
            public string EPGURL { get; set; }
        }

        public static void SaveUserData(User currentUser)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string saveDir = Path.Combine(assemblyFolder, "Users");
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            string filePath = Path.Combine(saveDir, currentUser.UserName + ".json");

            string json = JsonConvert.SerializeObject(currentUser, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static User GetUserData(string userName)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(assemblyFolder, "Users", userName + ".json");

            if (!File.Exists(filePath))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("User data file not found.");
                return null;
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<User>(json);
        }

        public static void SaveM3UData(M3UData m3uData, string playlistName)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string saveDir = Path.Combine(assemblyFolder, "M3U");
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            string sanitizedPlaylistName = GetSafeFilename(playlistName);
            string filePath = Path.Combine(saveDir, $"{sanitizedPlaylistName}.json");

            string json = JsonConvert.SerializeObject(m3uData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static M3UData GetM3UData(string playlistName)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string sanitizedPlaylistName = GetSafeFilename(playlistName);
            string filePath = Path.Combine(assemblyFolder, "M3U", $"{sanitizedPlaylistName}.json");

            if (!File.Exists(filePath))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("M3U data file not found.");
                return null;
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<M3UData>(json);
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
