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
        private CancellationTokenSource _cts;

        public LoginView()
        {
            InitializeComponent();
            _xtreamLoginService = new XtreamLoginService(); // Initialize the service
            _cts = new CancellationTokenSource();

            LoadSavedLogins();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
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

                // Attempt to connect
                bool loginSuccess = await _xtreamLoginService.CheckLoginConnection(server, username, password, port, useHttps, _cts.Token);
                if (loginSuccess)
                {
                    MessageBox.Show("Login successful!");
                }
                else
                {
                    MessageBox.Show("Login failed. Please check your credentials.");
                }
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
                // Save user data using the XtreamLoginService
                _xtreamLoginService.SaveUserData(username, password, server, port, useHttps);
                MessageBox.Show("User data saved successfully.");
                LoadSavedLogins(); // Refresh saved logins
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save user data: {ex.Message}");
            }
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

            var userData = _xtreamLoginService.LoadUserData(selectedLogin);
            if (userData != null)
            {
                ServerUrlTxt.Text = userData["Server"]?.ToString();
                XtreamUserTxt.Text = userData["Username"]?.ToString();
                XtreamPassBox.Password = userData["Password"]?.ToString();
                PortTxt.Text = userData["Port"]?.ToString();
                HttpsCheckBox.IsChecked = userData["UseHttps"]?.ToObject<bool>();
            }
            else
            {
                MessageBox.Show("Failed to load the saved login.");
            }
        }

        private void LoginMethodSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedItem = e.AddedItems[0] as ComboBoxItem;
                if (selectedItem != null)
                {
                    switch (selectedItem?.Tag?.ToString())
                    {
                        case "Xtream":
                            XtreamFields.Visibility = Visibility.Visible;
                            M3UFields.Visibility = Visibility.Collapsed;
                            break;
                        case "M3U":
                            XtreamFields.Visibility = Visibility.Collapsed;
                            M3UFields.Visibility = Visibility.Visible;
                            break;
                    }
                }
            }
        }

        private void LoadSavedLogins()
        {
            var savedLogins = _xtreamLoginService.GetSavedLogins();
            SavedLoginsComboBox.Items.Clear();

            foreach (var login in savedLogins)
            {
                SavedLoginsComboBox.Items.Add(login);
            }
        }
    }
}
