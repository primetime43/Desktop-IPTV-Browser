using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace X_IPTV
{
    public class UserDataSaver : UserLogin
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

        //Called when clicked save user info button
        public static void SaveUserData(User currentUser)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string saveDir = assemblyFolder + @"\Users\";
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            //Delete the file so the writer wont append text
            if (File.Exists(saveDir + currentUser.UserName + ".txt"))
                File.Delete(saveDir + currentUser.UserName + ".txt");

            using (StreamWriter w = File.AppendText(saveDir + "\\" + currentUser.UserName + ".txt"))
            {
                w.WriteLine("Username," + currentUser.UserName);
                w.WriteLine("Password," + currentUser.Password);
                w.WriteLine("Server," + currentUser.Server);
                w.WriteLine("Port," + currentUser.Port);
            }
        }

        //Called on program load to load all user data
        public static User GetUserData(string fileName, string localPath)
        {
            User loadedUser = new User();
            using (StreamReader r = new StreamReader(localPath))
            {
                string line;
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = r.ReadLine()) != null)
                {
                    string[] words = line.Split(',');
                    //build the user and return the User obj

                    switch (words[0])
                    {
                        case "Username":
                            loadedUser.UserName = words[1];
                            break;
                        case "Password":
                            loadedUser.Password = words[1];
                            break;
                        case "Server":
                            loadedUser.Server = words[1];
                            break;
                        case "Port":
                            loadedUser.Port = words[1];
                            break;
                        default:
                            Xceed.Wpf.Toolkit.MessageBox.Show("Error trying to set " + words[0] + " property");
                            break;
                    }
                }
            }
            return loadedUser;
        }

        public static void SaveM3UData(M3UData m3uData, string playlistName)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string saveDir = Path.Combine(assemblyFolder, "M3U");
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            string sanitizedPlaylistName = GetSafeFilename(playlistName);
            string filePath = Path.Combine(saveDir, $"{sanitizedPlaylistName}_m3u.txt");

            File.WriteAllText(filePath, $"PlaylistURL,{m3uData.PlaylistURL}\nEPGURL,{m3uData.EPGURL}");
        }

        public static M3UData GetM3UData(string playlistName)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string sanitizedPlaylistName = GetSafeFilename(playlistName).Replace("_m3u", ""); // Ensure "_m3u" is not doubled.
            string filePath = Path.Combine(assemblyFolder, "M3U", $"{sanitizedPlaylistName}_m3u.txt");

            if (!File.Exists(filePath))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("M3U data file not found.");
                return null;
            }

            M3UData m3uData = new M3UData();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2)
                {
                    switch (parts[0])
                    {
                        case "PlaylistURL":
                            m3uData.PlaylistURL = parts[1];
                            break;
                        case "EPGURL":
                            m3uData.EPGURL = parts[1];
                            break;
                        default:
                            Xceed.Wpf.Toolkit.MessageBox.Show($"Unknown key when loading M3U data: {parts[0]}");
                            break;
                    }
                }
            }

            return m3uData;
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
