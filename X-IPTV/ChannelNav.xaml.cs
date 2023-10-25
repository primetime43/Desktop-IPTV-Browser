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


namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for ChannelNav.xaml
    /// </summary>
    /// 
    public partial class ChannelNav : Window
    {
        private CancellationTokenSource cts;
        public ChannelNav()
        {
            InitializeComponent();
            loadCategories();//loads the categories into the listbox view
            loadUserInfo();//displays the user's info in the text box
        }

        private void loadCategories()
        {
            List<ChannelGroups> itemsTest = Instance.ChannelGroupsArray.OrderBy(x => x.category_name).ToList();
            listViewTest.ItemsSource = itemsTest;


            //Could eventually use an api call each time to just get the channels that are in the selected category.
            //player_api.php?username=X&password=X&action=get_live_streams&category_id=X (This will get All LIVE Streams in the selected category ONLY)
        }

        private void searchForChannelByCurrentShow()
        {

        }

        private static void searchForChannelByChannelName()
        {

        }

        private void loadUserInfo()
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
                var selectedItem = e.AddedItems[0] as ChannelGroups;

                if (selectedItem != null)
                {
                    string selectedText = selectedItem.category_name;
                    loadSelectedCategory(selectedText);
                }
            }
        }


        private async void loadSelectedCategory(string categoryName)
        {
            cts = new CancellationTokenSource();

            Instance.selectedCategory = categoryName;

            busy_ind.IsBusy = true;
            int counter = 0;
            foreach (ChannelGroups entry in Instance.ChannelGroupsArray)
            {
                if (Instance.selectedCategory == entry.category_name)
                {
                    string selectedCategoryID = entry.category_id.ToString();

                    List<ChannelEntry> channels = Instance.categoryToChannelMap[selectedCategoryID];

                    foreach (ChannelEntry channel in channels)
                    {
                        if (!cts.IsCancellationRequested)//checks if its been canceled
                        {
                            busy_ind.BusyContent = $"Loading epg data for {entry.category_name}... ({counter + 1}/{channels.Count})";
                            await REST_Ops.GetEPGDataForIndividualChannel(channel, cts.Token);
                            counter++;
                        }
                        else
                            break;
                    }
                }
            }

            busy_ind.IsBusy = false;
            ChannelList channelWindow = new ChannelList();
            if (counter > 0 && !cts.IsCancellationRequested)
            {
                channelWindow.ShowDialog();
                this.Close();
            }
            else if(counter == 0)
                Xceed.Wpf.Toolkit.MessageBox.Show("No channels available in " + Instance.selectedCategory);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
            busy_ind.IsBusy = false;
        }
    }
}
