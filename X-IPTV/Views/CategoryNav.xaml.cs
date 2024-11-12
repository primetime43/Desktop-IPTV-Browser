using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using X_IPTV.Models;
using X_IPTV.Navigation;
using Xceed.Wpf.Toolkit;
using static X_IPTV.M3UPlaylist;
using static X_IPTV.XtreamCodes;

namespace X_IPTV.Views
{
    /// <summary>
    /// Interaction logic for CategoryNav.xaml
    /// </summary>
    /// 
    public partial class CategoryNav : Page
    {
        private System.Timers.Timer _updateCheckTimer;
        private CancellationTokenSource _cts;
        public CategoryNav()
        {
            InitializeComponent();
            loadCategories();//loads the categories into the listbox view
            InitializeUpdateCheckTimer(); // Initialize the update check timer
            if(Instance.XtreamCodesChecked)
                loadUserInfo();//displays the user's info in the text box
            else
                userInfoTxtBox.Visibility = Visibility.Collapsed;
        }

        #region Auto updating EPG Data Functions
        private void InitializeUpdateCheckTimer()
        {
            // Create and configure the timer
            _updateCheckTimer = new System.Timers.Timer(60000); // Trigger every minute (60000 milliseconds) Maybe make this a config in settings
            _updateCheckTimer.Elapsed += UpdateCheckTimer_Elapsed;
            _updateCheckTimer.AutoReset = true;
            _updateCheckTimer.Start();
        }

        private void UpdateCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check if it's time to update EPG data based on your logic
            if (Instance.ShouldUpdateOnInterval(DateTime.Now))
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    // Perform the update on the UI thread if needed
                    await UpdateEpgData();
                });
            }
        }

        private async Task UpdateEpgData()
        {
            _cts = new CancellationTokenSource();
            try
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("EPG Updated!");
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error updating EPG: {ex.Message}");
            }
            finally
            {
                _cts.Dispose();
            }
        }

        #endregion
        private void loadCategories()
        {
            if (Instance.XtreamCodesChecked)
            {
                var groups = Instance.XtreamCategoryList.OrderBy(x => x.CategoryName).ToList();
                listViewTest.ItemsSource = groups;
            }
            else if (Instance.M3uChecked)
            {
                var groups = Instance.M3UCategoryList.OrderBy(x => x.CategoryName).ToList();
                listViewTest.ItemsSource = groups;
            }
        }

        private void loadUserInfo()
        {
            if (Instance.XtreamCodesChecked)
            {
                userInfoTxtBox.Document.Blocks.Clear();

                // User Information Header
                var userInfoTitle = new Paragraph(new Run("User Information"))
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"))
                };
                userInfoTxtBox.Document.Blocks.Add(userInfoTitle);

                // Add user_info properties
                foreach (var prop in typeof(User_Info).GetProperties())
                {
                    string text = FormatUserInfoText(prop);
                    userInfoTxtBox.Document.Blocks.Add(new Paragraph(new Run(text)) { Margin = new Thickness(0) });
                }

                // Server Information Header
                var serverInfoTitle = new Paragraph(new Run("Server Information"))
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                    Margin = new Thickness(0, 10, 0, 0)
                };
                userInfoTxtBox.Document.Blocks.Add(serverInfoTitle);

                // Add server_info properties
                foreach (var prop in typeof(Server_Info).GetProperties())
                {
                    string text = $"{prop.Name}: {prop.GetValue(Instance.PlayerInfo.server_info)}";
                    userInfoTxtBox.Document.Blocks.Add(new Paragraph(new Run(text)) { Margin = new Thickness(0) });
                }
            }
        }

        private string FormatUserInfoText(PropertyInfo prop)
        {
            // Custom logic for property values
            return $"{prop.Name}: {prop.GetValue(Instance.PlayerInfo.user_info)}";
        }

        private void quitBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void backToLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            /*UserLogin.ReturnToLogin = true;
            this.Close();*/
        }

        private void ListViewItem_Selected(object sender, SelectionChangedEventArgs e)
        {
            var navigationManager = new NavigationManager(this.NavigationService);
            if (e.AddedItems.Count > 0)
            {
                if (Instance.XtreamCodesChecked)
                {
                    var selectedItem = e.AddedItems[0] as XtreamCategory;

                    if (selectedItem != null)
                    {
                        string selectedText = selectedItem.CategoryName;
                        loadSelectedCategory(selectedText);
                        navigationManager.NavigateToPage("XtreamChannelsPage");
                    }
                }
                else if(Instance.M3uChecked)
                {
                    var selectedItem = e.AddedItems[0] as M3UCategory;

                    if (selectedItem != null)
                    {
                        string selectedText = selectedItem.CategoryName;
                        loadSelectedCategory(selectedText);
                        navigationManager.NavigateToPage("M3UChannelPage");
                    }
                }
                else
                    Xceed.Wpf.Toolkit.MessageBox.Show("Error with the selected category.");
            }
        }


        private async void loadSelectedCategory(string categoryName)
        {
            _cts = new CancellationTokenSource();
            Instance.selectedCategory = categoryName;
            var mw = Application.Current.MainWindow as MainWindow;

            busy_ind.IsBusy = true;
            if (Instance.XtreamCodesChecked)
            {
                XtreamChannelList channelWindow = new XtreamChannelList();
                if (!_cts.IsCancellationRequested)
                {
                    busy_ind.IsBusy = false;
                    //channelWindow.ShowDialog();
                    //mw.ContentFrame.Navigate(channelWindow);
                }

                //this.NavigationService.Navigate(new Uri("XtreamChannelList.xaml", UriKind.Relative));
                busy_ind.IsBusy = false;
            }
            else if (Instance.M3uChecked)
            {
                var channels = Instance.M3UChannels.Where(c => c.CategoryName == categoryName).ToList();
                if (channels.Count > 0)
                {
                    await M3UPlaylist.PairEPGTOChannelM3U(channels);
                    busy_ind.IsBusy = false;

                    M3UChannelList channelWindow = new M3UChannelList();
                    //channelWindow.ShowDialog();
                    /*mw.CategoriesItem.IsSelected = true;
                    mw.ContentFrame.Navigate(channelWindow);*/

                    //this.NavigationService.Navigate(new Uri("M3UChannelsList.xaml", UriKind.Relative));
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("No channels available in " + Instance.selectedCategory);
                }
            }
            else
                Xceed.Wpf.Toolkit.MessageBox.Show("Error loading playlist. Please report issue on Github.");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
            busy_ind.IsBusy = false;
        }
    }
}
