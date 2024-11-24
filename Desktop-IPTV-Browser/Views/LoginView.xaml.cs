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

            LoadSavedLogins(); // Initial load of saved logins
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginMethodSelector.SelectedValue is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag?.ToString() == "Xtream")
                {
                    // Xtream Login Logic
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
                        MessageBox.Show(loginSuccess ? "Xtream login successful!" : "Xtream login failed.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during Xtream login: {ex.Message}");
                    }
                }
                else if (selectedItem.Tag?.ToString() == "M3U")
                {
                    // M3U Login Logic
                    string m3uUrl = M3UUrlTxt.Text;
                    string epgUrl = M3UEpgUrlTxt.Text;

                    if (string.IsNullOrWhiteSpace(m3uUrl))
                    {
                        MessageBox.Show("Please provide a valid M3U URL.");
                        return;
                    }

                    try
                    {
                        await _m3uLoginService.ConnectM3U(m3uUrl, epgUrl, _cts.Token);
                        MessageBox.Show("M3U connection successful!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during M3U connection: {ex.Message}");
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginMethodSelector.SelectedValue is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag?.ToString() == "Xtream")
                {
                    // Save Xtream Login
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
                        MessageBox.Show($"Error saving Xtream login: {ex.Message}");
                    }
                }
                else if (selectedItem.Tag?.ToString() == "M3U")
                {
                    // Save M3U Login
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
                        MessageBox.Show($"Error saving M3U login: {ex.Message}");
                    }
                }
            }
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
            SavedLoginsComboBox?.Items.Clear();

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
                            foreach (var login in _xtreamLoginService.GetSavedLogins())
                            {
                                SavedLoginsComboBox.Items.Add(login);
                            }
                            break;

                        case "M3U":
                            XtreamFields.Visibility = Visibility.Collapsed;
                            M3UFields.Visibility = Visibility.Visible;
                            foreach (var login in _m3uLoginService.GetSavedLogins())
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
            SavedLoginsComboBox.Items.Clear();
            if (LoginMethodSelector.SelectedValue is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag?.ToString() == "Xtream")
                {
                    foreach (var login in _xtreamLoginService.GetSavedLogins())
                    {
                        SavedLoginsComboBox.Items.Add(login);
                    }
                }
                else if (selectedItem.Tag?.ToString() == "M3U")
                {
                    foreach (var login in _m3uLoginService.GetSavedLogins())
                    {
                        SavedLoginsComboBox.Items.Add(login);
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            MessageBox.Show("Operation canceled.");
        }

        private string PromptForFileName(string message)
        {
            var inputDialog = new InputDialog(message);
            return inputDialog.ShowDialog() == true ? inputDialog.ResponseText : null;
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
    }
}
