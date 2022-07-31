using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

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
                            MessageBox.Show("Error trying to set " + words[0] + " property");
                            break;
                    }
                }
            }
            return loadedUser;
        }
        //Call locally to this class
        //private void loadUsersFromDirectory()
        //{
        //    string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    string saveDir = assemblyFolder + @"\Users";
        //    DirectoryInfo DI = new DirectoryInfo(saveDir);
        //    FileInfo[] files = DI.GetFiles("*.txt");
        //    //Read files from dir
        //    foreach (var file in files)
        //    {
        //        MessageBox.Show(file.Name);
        //    }
        //}

        //private static void ProcessDirectory(string targetDirectory)
        //{
        //    // Process the list of files found in the directory.
        //    string[] fileEntries = Directory.GetFiles(targetDirectory);
        //    foreach (string fileName in fileEntries)
        //        ProcessFile(fileName);

        //    // Recurse into subdirectories of this directory.
        //    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        //    foreach (string subdirectory in subdirectoryEntries)
        //        ProcessDirectory(subdirectory);
        //}

        // Insert logic for processing found files here.
        //public static void ProcessFile(string path)
        //{
        //    Console.WriteLine("Processed file '{0}'.", path);
        //}
    }
}
