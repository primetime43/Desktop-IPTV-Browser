using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;
using static X_IPTV.M3UPlaylist;
using static X_IPTV.XtreamCodes;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for CategoryNav.xaml
    /// </summary>
    /// 
    public partial class CategoryNav : Window
    {
        private CancellationTokenSource cts;
        public CategoryNav()
        {
            InitializeComponent();
            loadCategories();//loads the categories into the listbox view
            if(Instance.XtreamCodesChecked)
                loadUserInfo();//displays the user's info in the text box
            else
                userInfoTxtBox.Visibility = Visibility.Collapsed;
        }

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

                userInfoTxtBox.AppendText("user_info:\r");
                foreach (PropertyInfo ce in typeof(User_Info).GetProperties())
                {
                    if (ce.Name == "exp_date" || ce.Name == "created_at")
                        userInfoTxtBox.AppendText(ce.Name + ": " + ChannelOptions.convertUnixToRealTIme(Convert.ToInt32(ce.GetValue(Instance.PlayerInfo.user_info))) + "\r");
                    else if (ce.Name == "allowed_output_formats")
                    {
                        userInfoTxtBox.AppendText(ce.Name + ": ");
                        string[] formats = (string[])ce.GetValue(Instance.PlayerInfo.user_info);
                        for (int i = 0; i < formats.Length; i++)
                        {
                            userInfoTxtBox.AppendText(formats[i]);
                            if (i < formats.Length - 1)
                                userInfoTxtBox.AppendText(", ");
                        }
                        userInfoTxtBox.AppendText("\r");
                    }
                    else
                        userInfoTxtBox.AppendText(ce.Name + ": " + ce.GetValue(Instance.PlayerInfo.user_info) + "\r");
                }

                userInfoTxtBox.AppendText("\rserver_info:\r");
                foreach (PropertyInfo ce in typeof(Server_Info).GetProperties())
                {
                    userInfoTxtBox.AppendText(ce.Name + ": " + ce.GetValue(Instance.PlayerInfo.server_info) + "\r");
                }
            }
        }


        private void quitBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void backToLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            UserLogin.ReturnToLogin = true;
            this.Close();
        }

        private void ListViewItem_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (Instance.XtreamCodesChecked)
                {
                    var selectedItem = e.AddedItems[0] as XtreamCategory;

                    if (selectedItem != null)
                    {
                        string selectedText = selectedItem.CategoryName;
                        loadSelectedCategory(selectedText);
                    }
                }
                else if(Instance.M3uChecked)
                {
                    var selectedItem = e.AddedItems[0] as M3UCategory;

                    if (selectedItem != null)
                    {
                        string selectedText = selectedItem.CategoryName;
                        loadSelectedCategory(selectedText);
                    }
                }
                else
                    Xceed.Wpf.Toolkit.MessageBox.Show("Error with the selected category.");
            }
        }


        private async void loadSelectedCategory(string categoryName)
        {
            cts = new CancellationTokenSource();

            Instance.selectedCategory = categoryName;

            busy_ind.IsBusy = true;
            if (Instance.XtreamCodesChecked)
            {
                XtreamChannelList channelWindow = new XtreamChannelList();
                if (!cts.IsCancellationRequested)
                {
                    channelWindow.ShowDialog();
                    this.Close();
                }
            }
            else if(Instance.M3uChecked)
            {
                // Logic for loading channels from M3U playlists
                var channels = Instance.M3UChannels.Where(c => c.CategoryName == categoryName).ToList();
                if (channels.Count > 0)
                {
                    await M3UPlaylist.PairEPGTOChannelM3U(channels);
                    busy_ind.IsBusy = false;

                    M3UChannelList channelWindow = new M3UChannelList();
                    channelWindow.ShowDialog();
                    this.Close();
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("No channels available in " + Instance.selectedCategory);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
            busy_ind.IsBusy = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            UniversalSearchList searchWindow = new UniversalSearchList();
            searchWindow.Show();
        }
    }
}
