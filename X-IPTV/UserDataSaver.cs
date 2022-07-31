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
            public int Port { get; set; }
        }

        //Called when clicked save user info button
        public void SaveUserData(User user)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string saveDir = assemblyFolder + @"\Users";
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);
            var ser = new XmlSerializer(typeof(User));
            using (StreamWriter w = File.AppendText(saveDir + "\\" + user.UserName + ".txt"))
            {
                w.WriteLine("Username," + user.UserName);
                w.WriteLine("Password," + user.Password);
                w.WriteLine("Server," + user.Server);
                w.WriteLine("Port," + user.Port);
            }

            //loadUsersFromDirectory();
        }

        //Called on program load to load all user data
        public User GetUserData(string fileName, string localPath)
        {
            /*using (StreamReader r = new StreamReader(localPath))
            {
                string line;
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = r.ReadLine()) != null)
                {
                    //build the user and return the User obj
                    Console.WriteLine(line);
                }
            }
            return user;*/
            return new User();
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
