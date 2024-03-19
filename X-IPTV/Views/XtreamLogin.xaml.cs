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
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static X_IPTV.Service.UserDataSaver;
using X_IPTV.Navigation;
using Microsoft.Win32;
using X_IPTV.Service;
using X_IPTV.Utilities;

namespace X_IPTV.Views
{
    /// <summary>
    /// Interaction logic for XtreamLogin.xaml
    /// </summary>
    public partial class XtreamLogin : Page
    {
        private static User _currentUser = new User();
        private static string assemblyFolder, saveDir, userFileFullPath;
        private static bool updateCheckDone = false;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public XtreamLogin()
        {
            InitializeComponent();

            // Loading users using the usersFolderPath from the configuration
            var usersFolderPath = ConfigurationManager.GetSetting("usersFolderPath");
            if (string.IsNullOrEmpty(usersFolderPath))
            {
                // Fallback to default directory next to the executable if not specified
                assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                saveDir = Path.Combine(assemblyFolder, "XtreamUsers");
            }
            else
            {
                // Use the specified directory from the configuration
                saveDir = usersFolderPath;
            }

            loadUsersFromDirectory();
        }

        private void loadUsersFromDirectory()
        {
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);
            DirectoryInfo DI = new DirectoryInfo(saveDir);
            FileInfo[] files = DI.GetFiles("*.json"); // Assuming user data is stored in JSON files
                                                      // Clear existing items
            if (UsercomboBox.Items != null)
                UsercomboBox.Items.Clear();
            // Add user files to the combobox, removing the file extension for display
            foreach (var file in files)
            {
                UsercomboBox.Items.Add(Path.GetFileNameWithoutExtension(file.Name));
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
            protocolCheckBox.IsChecked = _currentUser.UseHttps;
            serverTxt.Text = _currentUser.Server;
            portTxt.Text = _currentUser.Port;
        }

        private void loadUserDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UsercomboBox.SelectedItem == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You must select a user to load");
                return;
            }

            // Assuming UsercomboBox.SelectedValue.ToString() gives the username
            string userName = UsercomboBox.SelectedValue.ToString();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                _currentUser = UserDataSaver.GetUserData(userName);
                if (_currentUser != null)
                {
                    loadDataIntoTextFields();
                    // Optional: Clear the selection if needed or keep it based on your UI logic
                    // UsercomboBox.SelectedItem = null;
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Failed to load user data. User file might be missing or corrupted.");
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Invalid user selection.");
            }
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
            _currentUser.UseHttps = (bool)protocolCheckBox.IsChecked;
            _currentUser.Server = serverTxt.Text;
            _currentUser.Port = portTxt.Text;
            SaveXtreamUserData(_currentUser);
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
            try
            {
                busy_ind.BusyContent = "Attempting to connect...";
                if (await XtreamCodes.CheckLoginConnection(_cts.Token)) // Connect to the server
                {
                    busy_ind.BusyContent = "Loading groups/categories data...";
                    await XtreamCodes.RetrieveCategories(_cts.Token); // Load epg into the channels array

                    busy_ind.BusyContent = "Loading channel data...";
                    await XtreamCodes.RetrieveXtreamPlaylistData(busy_ind, _cts.Token);

                    busy_ind.BusyContent = "Downloading playlist epg...";
                    Instance.allXtreamEpgData = await XtreamCodes.DownloadEPGAndSaveToFile(_cts.Token);
                    await XtreamCodes.UpdateChannelsEpgData(Instance.XtreamChannels);

                    // Update the lastEpgDataLoadTime setting with the current date and time
                    ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.Now.ToString("o"));

                    busy_ind.IsBusy = false;
                    if (!_cts.IsCancellationRequested)
                    {
                        var navigationManager = new NavigationManager(this.NavigationService);
                        navigationManager.NavigateToPage("CategoriesPage");
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
                _cts = new CancellationTokenSource(); // Reset the token
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            busy_ind.IsBusy = false;
            _cts.Cancel();
        }
    }
}
