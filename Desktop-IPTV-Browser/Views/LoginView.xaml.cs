using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Desktop_IPTV_Browser.Services;

namespace Desktop_IPTV_Browser.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        private readonly XtreamLoginService _xtreamLoginService;
        private readonly M3ULoginService _m3uLoginService;
        private CancellationTokenSource _cts;

        public LoginView()
        {
            InitializeComponent();
            _xtreamLoginService = new XtreamLoginService();
            _m3uLoginService = new M3ULoginService();
            _cts = new CancellationTokenSource();

            LoadSavedLogins();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginMethodSelector.SelectedValue is ComboBoxItem selectedItem && selectedItem.Tag?.ToString() == "Xtream")
            {
                string server = ServerUrlTxt.Text;
                string username = XtreamUserTxt.Text;
                string password = XtreamPassBox.Password;
                string port = PortTxt.Text;
                bool useHttps = HttpsCheckBox.IsChecked == true;

                try
                {
                    string connectionString = useHttps
                        ? $"https://{server}:{port}/player_api.php?username={username}&password={password}"
                        : $"http://{server}:{port}/player_api.php?username={username}&password={password}";

                    bool loginSuccess = await _xtreamLoginService.CheckLoginConnection(server, username, password, port, useHttps, _cts.Token);
                    MessageBox.Show(loginSuccess ? "Login successful!" : "Login failed. Please check your credentials.");
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show("Login process was canceled.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else if (LoginMethodSelector.SelectedValue is ComboBoxItem m3uItem && m3uItem.Tag?.ToString() == "M3U")
            {
                string m3uUrl = M3UUrlTxt.Text;
                string epgUrl = M3UEpgUrlTxt.Text;

                if (string.IsNullOrWhiteSpace(m3uUrl))
                {
                    MessageBox.Show("Please provide a valid M3U URL.");
                    return;
                }

                MessageBox.Show("M3U connection logic to be implemented.");
            }
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            XtreamPassTxt.Visibility = Visibility.Visible;
            XtreamPassBox.Visibility = Visibility.Collapsed;
            XtreamPassTxt.Text = XtreamPassBox.Password;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            XtreamPassTxt.Visibility = Visibility.Collapsed;
            XtreamPassBox.Visibility = Visibility.Visible;
            XtreamPassBox.Password = XtreamPassTxt.Text;
        }

        private void XtreamPassBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (XtreamPassTxt.Visibility == Visibility.Visible)
            {
                XtreamPassTxt.Text = XtreamPassBox.Password;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginMethodSelector.SelectedValue is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag?.ToString() == "Xtream")
                {
                    string server = ServerUrlTxt.Text;
                    string username = XtreamUserTxt.Text;
                    string password = XtreamPassBox.Password;
                    string port = PortTxt.Text;
                    bool useHttps = HttpsCheckBox.IsChecked == true;

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        MessageBox.Show("Please enter a username.");
                        return;
                    }

                    try
                    {
                        _xtreamLoginService.SaveUserData(username, password, server, port, useHttps);
                        MessageBox.Show("Xtream login saved successfully.");
                        LoadSavedLogins();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save Xtream login: {ex.Message}");
                    }
                }
                else if (selectedItem.Tag?.ToString() == "M3U")
                {
                    string fileName = PromptForFileName("Enter the name for the M3U login:");
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        MessageBox.Show("File name cannot be empty.");
                        return;
                    }

                    string m3uUrl = M3UUrlTxt.Text;
                    string epgUrl = M3UEpgUrlTxt.Text;

                    if (string.IsNullOrWhiteSpace(m3uUrl))
                    {
                        MessageBox.Show("Please enter the M3U Playlist URL.");
                        return;
                    }

                    try
                    {
                        _m3uLoginService.SaveM3UData(fileName, m3uUrl, epgUrl);
                        MessageBox.Show("M3U login saved successfully.");
                        LoadSavedLogins();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save M3U login: {ex.Message}");
                    }
                }
            }
        }

        private string PromptForFileName(string message)
        {
            var inputDialog = new InputDialog(message);
            if (inputDialog.ShowDialog() == true)
            {
                return inputDialog.ResponseText;
            }
            return null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            MessageBox.Show("Operation canceled.");
        }

        private void LoadSavedLogin_Click(object sender, RoutedEventArgs e)
        {
            if (SavedLoginsComboBox.SelectedItem is not string selectedLogin)
            {
                MessageBox.Show("Please select a saved login.");
                return;
            }

            if (LoginMethodSelector.SelectedValue is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag?.ToString() == "Xtream")
                {
                    var userData = _xtreamLoginService.LoadUserData(selectedLogin);
                    if (userData != null)
                    {
                        ServerUrlTxt.Text = userData["Server"]?.ToString();
                        XtreamUserTxt.Text = userData["Username"]?.ToString();
                        XtreamPassBox.Password = userData["Password"]?.ToString();
                        PortTxt.Text = userData["Port"]?.ToString();
                        HttpsCheckBox.IsChecked = userData["UseHttps"]?.ToObject<bool>();
                    }
                }
                else if (selectedItem.Tag?.ToString() == "M3U")
                {
                    var m3uData = _m3uLoginService.LoadM3UData(selectedLogin);
                    if (m3uData != null)
                    {
                        M3UUrlTxt.Text = m3uData["M3UUrl"]?.ToString();
                        M3UEpgUrlTxt.Text = m3uData["EPGUrl"]?.ToString();
                    }
                }
            }
        }

        private void LoginMethodSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SavedLoginsComboBox?.Items.Clear(); // Clear saved logins to avoid mix-ups
            if (e.AddedItems.Count > 0)
            {
                var selectedItem = e.AddedItems[0] as ComboBoxItem;

                if (selectedItem != null)
                {
                    switch (selectedItem.Tag?.ToString())
                    {
                        case "Xtream":
                            XtreamFields.Visibility = Visibility.Visible;
                            M3UFields.Visibility = Visibility.Collapsed;

                            // Load only Xtream logins
                            var savedXtreamLogins = _xtreamLoginService.GetSavedLogins();
                            foreach (var login in savedXtreamLogins)
                            {
                                SavedLoginsComboBox.Items.Add(login);
                            }
                            break;

                        case "M3U":
                            XtreamFields.Visibility = Visibility.Collapsed;
                            M3UFields.Visibility = Visibility.Visible;

                            // Load only M3U logins
                            var savedM3ULogins = _m3uLoginService.GetSavedLogins();
                            foreach (var login in savedM3ULogins)
                            {
                                SavedLoginsComboBox.Items.Add(login);
                            }
                            break;
                    }
                }
            }
        }

        private void LoadSavedLogins()
        {
            SavedLoginsComboBox.Items.Clear(); // Clear the ComboBox to refresh the data

            if (LoginMethodSelector.SelectedValue is ComboBoxItem selectedItem)
            {
                switch (selectedItem.Tag?.ToString())
                {
                    case "Xtream":
                        // Load Xtream saved logins
                        var savedXtreamLogins = _xtreamLoginService.GetSavedLogins();
                        foreach (var login in savedXtreamLogins)
                        {
                            SavedLoginsComboBox.Items.Add(login);
                        }
                        break;

                    case "M3U":
                        // Load M3U saved logins
                        var savedM3ULogins = _m3uLoginService.GetSavedLogins();
                        foreach (var login in savedM3ULogins)
                        {
                            SavedLoginsComboBox.Items.Add(login);
                        }
                        break;
                }
            }
        }
    }
}
