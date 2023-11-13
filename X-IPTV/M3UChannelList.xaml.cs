using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static X_IPTV.M3UPlaylist;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for M3UChannelList.xaml
    /// </summary>
    public partial class M3UChannelList : Window
    {
        private M3UChannelModel model;
        private DateTime windowOpenTime; // Store the window open time
        public M3UChannelList()
        {
            InitializeComponent();

            this.model = new M3UChannelModel();
            this.model.Initialize();
            this.windowOpenTime = DateTime.Now;

            //the model is the array that holds all of the M3UChannel Objects set in the M3UChannelModel class
            if (Instance.M3uChecked)
            {
                M3UChannelLst.DataContext = this.model;
            }
        }

        private void M3USearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox != null && this.model != null)
            {
                var filterText = SearchTextBox.Text.ToLower();
                var filteredItems = this.model.MyListBoxItems
                    .Where(channel => (channel.ChannelName?.ToLower().Contains(filterText) ?? false) ||
                                      (channel.EPGData?.Description?.ToLower().Contains(filterText) ?? false))
                    .ToList();

                M3UChannelLst.ItemsSource = filteredItems; // Update the ItemsSource of your ListBox
            }
        }

        private void M3UChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Create an instance of ChannelOptions.
            ChannelOptions channelOp = new ChannelOptions();

            // Check if there is at least one item selected and the M3U checkbox is checked.
            if (e.AddedItems.Count > 0 && Instance.M3uChecked)
            {
                // Attempt to cast the selected item to an M3UChannel.
                M3UChannel m3uChannel = e.AddedItems[0] as M3UChannel;

                // If the cast is successful (i.e., the selected item is indeed an M3UChannel).
                if (m3uChannel != null)
                {
                    // Set the tempChannel property of the ChannelOptions instance.
                    channelOp.tempChannel = m3uChannel;

                    // Now display the selected channel data in the ChannelOptions window.
                    if (channelOp.DisplaySelectedChannelData(m3uChannel))
                    {
                        // If the data is successfully displayed, show the ChannelOptions window.
                        channelOp.Show();
                    }
                }
            }
        }

        private void listBox1_MouseDown(object sender, RoutedEventArgs e)
        {
            /*if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                listBox1.SelectedIndex = listBox1.IndexFromPoint(e.Location);
                if (listBox1.SelectedIndex != -1)
                {
                    listboxContextMenu.Show();
                }
            }*/
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //only auto update the epg on 15 min increments if the window has been open that long
            //(actually this needs fixed because if no windows aren't open longer than 15 min increments, it won't
            //ever auto update; unless manually update is clicked)
            //Add an check eventually that checks the last time the epg data was updated and update it
            DateTime now = DateTime.Now;
            if (now.Subtract(windowOpenTime).TotalMinutes >= 15 || ShouldUpdateOnInterval(now))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await M3UPlaylist.UpdateChannelsEpgData(Instance.M3UChannels);
                        Debug.WriteLine("EPG updated...");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating EPG: {ex.Message}");
                    }
                });
            }
            else
                Debug.WriteLine("EPG not updated...");

            CategoryNav categoryNavWindow = new CategoryNav();
            categoryNavWindow.Show();
        }

        private bool ShouldUpdateOnInterval(DateTime currentTime)
        {
            return currentTime.Minute % 15 == 0 && currentTime.Second == 0;
        }
    }

    public class M3UChannelModel
    {
        private bool isInitialized = false;

        public M3UChannelModel()
        {
            MyListBoxItems = new ObservableCollection<M3UChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.M3uChecked)
            {
                // Load M3U channels
                var channels = Instance.M3UChannels.Where(c => c.CategoryName == Instance.selectedCategory).ToList();
                foreach (var channel in channels)
                {
                    MyListBoxItems.Add(channel);
                }
            }
            isInitialized = true;
        }

        public ObservableCollection<M3UChannel> MyListBoxItems { get; set; }
    }
    public class M3UMyMockClass
    {
        public M3UMyMockClass()
        {
            MyListBoxItems = new ObservableCollection<M3UChannel>
        {
            new M3UChannel
            {
                ChannelName = "|FR| TF1 UHD",
                LogoUrl = "http://f.iptv-pure.com/tf14k.png",
                EPGData = new M3UEPGData
                {
                    ProgramTitle = "Title 1",
                    Description = "Description 1",
                    StartTime = DateTime.Now,
                }
            },
            new M3UChannel
            {
                ChannelName = "|FR| CSTAR FHD",
                LogoUrl = "http://f.iptv-pure.com/cstar.png",
                EPGData = new M3UEPGData
                {
                    ProgramTitle = "Title 2",
                    Description = "Description 2",
                    StartTime = DateTime.Now.AddHours(1)
                }
            }
        };
        }
        public ObservableCollection<M3UChannel> MyListBoxItems { get; set; }
    }
}
