using System;
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
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string server = ServerUrlTxt.Text;
            string username = XtreamUserTxt.Text;
            string password = XtreamPassTxt.Text;
            string port = PortTxt.Text;
            bool useHttps = HttpsCheckBox.IsChecked == true;

            try
            {
                // Attempt to connect using the XtreamLoginService
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
                // Handle cancellation
                MessageBox.Show("Login process was canceled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string server = ServerUrlTxt.Text;
            string username = XtreamUserTxt.Text;
            string password = XtreamPassTxt.Text;
            string port = PortTxt.Text;
            bool useHttps = HttpsCheckBox.IsChecked == true;

            // Validate username
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save user data: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel any ongoing login operation
            _cts.Cancel();
            MessageBox.Show("Operation canceled.");
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
                            // Set visibility for Xtream login fields
                            XtreamFields.Visibility = Visibility.Visible;
                            M3UFields.Visibility = Visibility.Collapsed;
                            break;
                        case "M3U":
                            // Set visibility for M3U login fields
                            XtreamFields.Visibility = Visibility.Collapsed;
                            M3UFields.Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
