using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Net.Http.Json;
using GitHubReleaseChecker;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    /// 
    public partial class UserLogin : Window
    {
        private static string programVersion = "v1.5.1";
        private static UserDataSaver.User _currentUser = new UserDataSaver.User();
        private static string assemblyFolder, saveDir, userFileFullPath;
        private static bool updateCheckDone = false;

        public UserLogin()
        {
            InitializeComponent();
            this.Title = "User Login " + programVersion;
            if (!updateCheckDone)
            {
                checkForUpdate();
                updateCheckDone = true;
            }
            assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            saveDir = assemblyFolder + @"\Users\";
            loadUsersFromDirectory();
        }

        private async void Con_btn_Click(object sender, RoutedEventArgs e)
        {
            Instance.currentUser.username = usrTxt.Text;
            Instance.currentUser.password = passTxt.Text;
            Instance.currentUser.server = serverTxt.Text;
            Instance.currentUser.port = portTxt.Text;
            Instance.currentUser.useHttps = (bool)protocolCheckBox.IsChecked;


            busy_ind.IsBusy = true;

            busy_ind.BusyContent = "Attempting to connect...";

            if (await REST_Ops.CheckLoginConnection())//Connect to the server
            {
                busy_ind.BusyContent = "Loading groups/categories data...";

                await REST_Ops.RetrieveCategories();//Load epg it into the channels array

                busy_ind.BusyContent = "Loading channel data...";

                await REST_Ops.RetrieveChannelData(busy_ind);

                while (true)
                {
                    ChannelNav nav = new ChannelNav();
                    nav.ShowDialog();

                    foreach (ChannelGroups entry in Instance.ChannelGroupsArray)
                    {
                        //load epg data for select category here
                        if (Instance.selectedCategory == entry.category_name)
                        {
                            //busy_ind.BusyContent = "Loading epg data for " + entry.category_name + "...";
                            
                            string selectedCategoryID = entry.category_id.ToString();

                            List<ChannelEntry> channels = Instance.categoryToChannelMap[selectedCategoryID];

                            int counter = 0;
                            foreach (ChannelEntry channel in channels)
                            {
                                busy_ind.BusyContent = $"Loading epg data for {entry.category_name}... ({counter + 1}/{channels.Count})";
                                await REST_Ops.GetEPGDataForIndividualChannel(channel);
                                counter++;
                            }

                            //MessageBox.Show("Done with epg");
                        }
                    }

                    ChannelList channelWindow = new ChannelList();
                    channelWindow.ShowDialog();
                }
            }
            else
                busy_ind.IsBusy = false;

            //this.Close();
        }

        private void loadUsersFromDirectory()
        {
            //string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //string saveDir = assemblyFolder + @"\Users";
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);
            DirectoryInfo DI = new DirectoryInfo(saveDir);
            FileInfo[] files = DI.GetFiles("*.txt");
            //Read files from dir
            if (UsercomboBox.Items != null)
                UsercomboBox.Items.Clear();
            foreach (var file in files)
            {
                UsercomboBox.Items.Add(file.Name.Remove(file.Name.IndexOf('.')));
            }
        }
        private void loadDataIntoTextFields()
        {
            if (_currentUser?.UserName == null || _currentUser?.Password == null || _currentUser?.Server == null || _currentUser?.Port == null)
            {
                MessageBox.Show("User data is missing, unable to load " + UsercomboBox.SelectedValue.ToString());
                return;
            }

            usrTxt.Text = _currentUser.UserName;
            passTxt.Text = _currentUser.Password;
            serverTxt.Text = _currentUser.Server;
            portTxt.Text = _currentUser.Port;
        }
        private void loadUserDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UsercomboBox.SelectedItem == null)
            {
                MessageBox.Show("You must select a user to load");
                return;
            }

            _currentUser = UserDataSaver.GetUserData(UsercomboBox.SelectedValue.ToString(), getUserFileLocalPath());
            loadDataIntoTextFields();
            UsercomboBox.SelectedItem = null;
        }
        private string getUserFileLocalPath()
        {
            string? selectedUser = UsercomboBox.SelectedValue.ToString();
            if (selectedUser != null && selectedUser.Length > 0)
            {
                return userFileFullPath = saveDir + selectedUser + ".txt";
            }
            else
            {
                MessageBox.Show("You must select a user");
                return null;
            }
        }

        private void saveUserDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (usrTxt.Text == null || usrTxt.Text.Length <= 0)
            {
                MessageBox.Show("Username input field is blank");
                return;
            }

            _currentUser.UserName = usrTxt.Text;
            _currentUser.Password = passTxt.Text;
            _currentUser.Server = serverTxt.Text;
            _currentUser.Port = portTxt.Text;
            UserDataSaver.SaveUserData(_currentUser);
            loadUsersFromDirectory();
            MessageBox.Show(_currentUser.UserName + "'s data saved");
        }

        private void usrTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServerConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;

            textBoxPlaylistDataConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/get.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }

        private void passTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServerConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;

            textBoxPlaylistDataConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/get.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }

        private void serverTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServerConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;

            textBoxPlaylistDataConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/get.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }

        private void portTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServerConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;

            textBoxPlaylistDataConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/get.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }

        private async void checkForUpdate()
        {
            var release = await ReleaseChecker.CheckForNewRelease("primetime43", "Xtream-Browser");
            int latestReleaseInt = ReleaseChecker.convertVersionToInt(release.tag_name);
            int localProgramVersionInt = ReleaseChecker.convertVersionToInt(programVersion);

            if (release != null && latestReleaseInt > localProgramVersionInt)
            {
                MessageBoxResult result = MessageBox.Show("Current version: " + programVersion + "\nNew release available: " + release.name + " (" + release.tag_name + ")\nDo you want to download it?", "New Release", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = ReleaseChecker.releaseURL,
                            UseShellExecute = true
                        };

                        Process.Start(startInfo);
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }

            }
            else
            {
                MessageBox.Show("No new releases available.");
            }
        }
    }
}


namespace GitHubReleaseChecker
{
    public class Release
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public DateTime published_at { get; set; }
    }

    public class ReleaseChecker
    {
        private static readonly HttpClient client = new HttpClient();
        private const string apiUrl = "https://api.github.com/repos/{0}/{1}/releases";
        public static string releaseURL = "https://github.com/{0}/{1}/releases";

        public static async Task<Release> CheckForNewRelease(string owner, string repo)
        {
            var url = string.Format(apiUrl, owner, repo);
            releaseURL = string.Format(releaseURL, owner, repo);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
            var releases = await client.GetFromJsonAsync<List<Release>>(url);
            return releases[0];
        }

        public static int convertVersionToInt(string version)
        {
            string[] parts = version.Split('.');
            int major = int.Parse(parts[0].TrimStart('v'));
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);
            int versionInt = major * 10000 + minor * 100 + patch;
            return versionInt;
        }
    }
}

