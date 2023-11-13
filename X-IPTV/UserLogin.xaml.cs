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
using Newtonsoft.Json.Linq;
using static X_IPTV.UserDataSaver;
using Microsoft.Win32;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    /// 
    public partial class UserLogin : Window
    {
        private static string programVersion = "v3.0.0";
        private static User _currentUser = new User();
        private static string assemblyFolder, saveDir, userFileFullPath;
        private static bool updateCheckDone = false;
        private CancellationTokenSource cts = new CancellationTokenSource();
        public static bool ReturnToLogin { get; set; } = false;

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
            if (XtreamCodescheckBox?.IsChecked == true)
            {
                Instance.currentUser.username = usrTxt.Text;
                Instance.currentUser.password = passTxt.Text;
                Instance.currentUser.server = serverTxt.Text;
                Instance.currentUser.port = portTxt.Text;
                Instance.currentUser.useHttps = (bool)protocolCheckBox.IsChecked;

                if (string.IsNullOrWhiteSpace(Instance.currentUser.username) ||
                    string.IsNullOrWhiteSpace(Instance.currentUser.password) ||
                    string.IsNullOrWhiteSpace(Instance.currentUser.server) ||
                    string.IsNullOrWhiteSpace(Instance.currentUser.port))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please ensure all fields are filled out before attempting to connect.");
                    return; // Exit early as not all fields are filled.
                }

                busy_ind.IsBusy = true;
                UserLogin.ReturnToLogin = false;
                try
                {
                    busy_ind.BusyContent = "Attempting to connect...";
                    if (await XtreamCodes.CheckLoginConnection(cts.Token))//Connect to the server
                    {
                        busy_ind.BusyContent = "Loading groups/categories data...";
                        await XtreamCodes.RetrieveCategories(cts.Token); // Load epg into the channels array

                        busy_ind.BusyContent = "Loading channel data...";
                        await XtreamCodes.RetrieveXtreamPlaylistData(busy_ind, cts.Token);

                        busy_ind.BusyContent = "Downloading playlist epg...";
                        Instance.allXtreamEpgData = await XtreamCodes.DownloadEPGAndSaveToFile(cts.Token);
                        await XtreamCodes.UpdateChannelsEpgData(Instance.XtreamChannels);

                        busy_ind.IsBusy = false;
                        if (!cts.IsCancellationRequested)
                        {
                            CategoryNav nav = new CategoryNav();
                            nav.ShowDialog();
                        }
                    }
                    else
                    {
                        busy_ind.IsBusy = false;
                        busy_ind.BusyContent = ""; // Clear the busy content if the connection fails
                    }
                }
                catch (OperationCanceledException) { }

                cts = new CancellationTokenSource();//reset the token
            }
            else if(m3uCheckBox?.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(m3uEpgUrlTxtbox.Text))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please enter the M3U playlist url.");
                    return;
                }
                busy_ind.IsBusy = true;
                UserLogin.ReturnToLogin = false;
                busy_ind.BusyContent = "Attempting to connect...";
                try
                {
                    busy_ind.BusyContent = "Loading playlist channel data...";
                    await M3UPlaylist.RetrieveM3UPlaylistData(m3uURLTxtbox.Text, cts.Token); // Load epg into the channels array

                    busy_ind.BusyContent = "Loading playlist epg data...";
                    Instance.allM3uEpgData = await M3UPlaylist.DownloadAndParseEPG(m3uEpgUrlTxtbox.Text, cts.Token);
                    if (Instance.allM3uEpgData != null)
                    {
                        await M3UPlaylist.UpdateChannelsEpgData(Instance.M3UChannels);
                    }

                    //var epgDataForChannel = Instance.M3UEPGDataList.Where(e => e.ChannelId == "someChannelId").ToList();

                    busy_ind.IsBusy = false;
                    if (!cts.IsCancellationRequested)
                    {
                        CategoryNav nav = new CategoryNav();
                        nav.ShowDialog();
                    }
                    else
                    {
                        busy_ind.IsBusy = false;
                        busy_ind.BusyContent = ""; // Clear the busy content if the connection fails
                    }
                }
                catch (OperationCanceledException) { }

                cts = new CancellationTokenSource();//reset the token
            }
            else
                Xceed.Wpf.Toolkit.MessageBox.Show("You must select a checkbox option.");

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
                Xceed.Wpf.Toolkit.MessageBox.Show("User data is missing, unable to load " + UsercomboBox.SelectedValue.ToString());
                return;
            }

            usrTxt.Text = _currentUser.UserName;
            passTxt.Text = _currentUser.Password;
            serverTxt.Text = _currentUser.Server;
            portTxt.Text = _currentUser.Port;
        }

        private void XtreamCodes_Checked(object sender, RoutedEventArgs e)
        {
            m3uCheckBox.IsChecked = false;
            Instance.M3uChecked = false;
            Instance.XtreamCodesChecked = true;
        }

        private void M3UPlaylist_Checked(object sender, RoutedEventArgs e)
        {
            XtreamCodescheckBox.IsChecked = false;
            Instance.M3uChecked = true;
            Instance.XtreamCodesChecked = false;
        }
        private string getUserFileLocalPath()
        {
            string? selectedUser = UsercomboBox.SelectedValue.ToString();
            if (selectedUser != null && selectedUser.Length > 0)
            {
                userFileFullPath = saveDir + selectedUser + ".txt";
                return userFileFullPath;
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You must select a user");
                return null;
            }
        }

        private void loadUserDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UsercomboBox.SelectedItem == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You must select a user to load");
                return;
            }

            _currentUser = GetUserData(UsercomboBox.SelectedValue.ToString(), getUserFileLocalPath());
            loadDataIntoTextFields();   
            UsercomboBox.SelectedItem = null;
        }

        private void saveUserDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (usrTxt.Text == null || usrTxt.Text.Length <= 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Username input field is blank");
                return;
            }

            _currentUser.UserName = usrTxt.Text;
            _currentUser.Password = passTxt.Text;
            _currentUser.Server = serverTxt.Text;
            _currentUser.Port = portTxt.Text;
            SaveUserData(_currentUser);
            loadUsersFromDirectory();
            Xceed.Wpf.Toolkit.MessageBox.Show(_currentUser.UserName + "'s data saved");
        }

        private void M3U_loadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "M3U Files (*.txt)|*.txt",
                InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "M3U")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                M3UData m3uData = GetM3UData(filenameWithoutExtension);

                if (m3uData != null)
                {
                    m3uURLTxtbox.Text = m3uData.PlaylistURL;
                    m3uEpgUrlTxtbox.Text = m3uData.EPGURL;
                    Xceed.Wpf.Toolkit.MessageBox.Show(filenameWithoutExtension + " loaded successfully.");
                }
            }
        }

        private void M3U_saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(m3uURLTxtbox.Text))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("The playlist URL is empty.");
                return;
            }

            string playlistName = Microsoft.VisualBasic.Interaction.InputBox("Please enter a name for the playlist:", "Save Playlist", "DefaultPlaylistName");

            // Check if the InputBox was cancelled by the user.
            if (playlistName == "")
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(playlistName))
            {
                M3UData m3uData = new M3UData
                {
                    PlaylistURL = m3uURLTxtbox.Text,
                    EPGURL = m3uEpgUrlTxtbox.Text
                };

                SaveM3UData(m3uData, playlistName);
                MessageBox.Show("Playlist saved successfully as " + playlistName);
            }
            else
            {
                MessageBox.Show("You must enter a name for the playlist.");
            }
        }

        private void showUpdatedConnectionString(object sender, RoutedEventArgs e)
        {
            if ((bool)protocolCheckBox.IsChecked && textBoxServerConnectionString != null)
            {
                textBoxServerConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
                textBoxPlaylistDataConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/xmltv.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
            }
            else if(textBoxServerConnectionString != null)
            {
                textBoxServerConnectionString.Text = "http://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
                textBoxPlaylistDataConnectionString.Text = "http://" + serverTxt.Text + ":" + portTxt.Text + "/xmltv.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            busy_ind.IsBusy = false;
            cts.Cancel();
        }

        private async void checkForUpdate()
        {
            var release = await ReleaseChecker.CheckForNewRelease("primetime43", "Xtream-Browser");

            if (release != null)
            {
                int latestReleaseInt = ReleaseChecker.convertVersionToInt(release.tag_name);
                int localProgramVersionInt = ReleaseChecker.convertVersionToInt(programVersion);

                if (latestReleaseInt > localProgramVersionInt)
                {
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Current version: " + programVersion + "\nNew release available: " + release.name + " (" + release.tag_name + ")\nDo you want to download it?", "New Release", MessageBoxButton.YesNo, MessageBoxImage.Question);

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
                            Xceed.Wpf.Toolkit.MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
                Debug.WriteLine("Release null, skipping check.");

            }
            else
            {
                Debug.WriteLine("No new releases available.");
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

            if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "request");
            }

            try
            {
                var releases = await client.GetFromJsonAsync<List<Release>>(url);
                return releases.FirstOrDefault(); // Returns the first or default if no releases found
            }
            catch (HttpRequestException ex)
            {
                // If it's a rate limit error or other 4xx/5xx errors, handle it here
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show($"Error checking for new release: {ex.Message}\nPlease try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show($"Error checking for new release: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return null;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
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

