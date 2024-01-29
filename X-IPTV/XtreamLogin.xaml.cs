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
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static X_IPTV.UserDataSaver;
using Microsoft.Win32;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for XtreamLogin.xaml
    /// </summary>
    public partial class XtreamLogin : Page
    {
        private static User _currentUser = new User();
        private static string assemblyFolder, saveDir, userFileFullPath;
        private static bool updateCheckDone = false;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public XtreamLogin()
        {
            InitializeComponent();

            //loading users
            assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            saveDir = assemblyFolder + @"\Users\";
            loadUsersFromDirectory();
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

        private void showUpdatedConnectionString(object sender, RoutedEventArgs e)
        {
            if ((bool)protocolCheckBox.IsChecked && textBoxServerConnectionString != null)
            {
                textBoxServerConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
                textBoxPlaylistDataConnectionString.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/xmltv.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
            }
            else if (textBoxServerConnectionString != null)
            {
                textBoxServerConnectionString.Text = "http://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
                textBoxPlaylistDataConnectionString.Text = "http://" + serverTxt.Text + ":" + portTxt.Text + "/xmltv.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
            }
        }

        private async void Con_btn_Click(object sender, RoutedEventArgs e)
        {
            //testing
            Instance.XtreamCodesChecked = true;

            // Access the current MainWindow instance
            var mw = Application.Current.MainWindow as MainWindow;

            Instance.currentUser.username = usrTxt.Text;
            Instance.currentUser.password = passTxt.Text;
            Instance.currentUser.server = serverTxt.Text;
            Instance.currentUser.port = portTxt.Text;
            Instance.currentUser.useHttps = (bool)protocolCheckBox.IsChecked;

            busy_ind.IsBusy = true;
            UserLogin.ReturnToLogin = false;
            try
            {
                busy_ind.BusyContent = "Attempting to connect...";
                if (await XtreamCodes.CheckLoginConnection(cts.Token)) // Connect to the server
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
                        // Code to navigate or show dialogs after successful connection
                    }
                }
                else
                {
                    busy_ind.IsBusy = false;
                    busy_ind.BusyContent = ""; // Clear the busy content if the connection fails
                }
            }
            catch (ArgumentNullException)
            {
                busy_ind.IsBusy = false;
                Xceed.Wpf.Toolkit.MessageBox.Show("Please ensure all fields are filled out before attempting to connect.");
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
            }
            catch (Exception ex)
            {
                busy_ind.IsBusy = false;
                busy_ind.BusyContent = ""; // Clear the busy content
                Xceed.Wpf.Toolkit.MessageBox.Show($"Failed to connect. Error: {ex.Message}");
            }
            finally
            {
                cts = new CancellationTokenSource(); // Reset the token
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            busy_ind.IsBusy = false;
            cts.Cancel();
        }
    }
}
