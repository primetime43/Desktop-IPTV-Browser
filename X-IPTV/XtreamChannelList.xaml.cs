using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static X_IPTV.XtreamCodes;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for XtreamChannelList.xaml
    /// </summary>
    public partial class XtreamChannelList : Window
    {
        private XtreamChannelModel model;
        private DateTime windowOpenTime; // Store the window open time
        public XtreamChannelList()
        {
            InitializeComponent();

            this.model = new XtreamChannelModel();
            this.model.Initialize();
            this.windowOpenTime = DateTime.Now;

            if (Instance.XtreamCodesChecked)
            {
                XtreamChannelLst.DataContext = this.model;
            }
        }

        private void XtreamSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox != null && this.model != null)
            {
                var filterText = SearchTextBox.Text.ToLower();
                var filteredItems = this.model.MyListBoxItems
                    .Where(channel => (channel.ChannelName?.ToLower().Contains(filterText) ?? false) ||
                                      (channel.EPGData?.Description.ToLower().Contains(filterText) ?? false))
                    .ToList();

                XtreamChannelLst.ItemsSource = filteredItems; // Update the ItemsSource of your ListBox
            }
        }

        private void XtreamChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Create an instance of ChannelOptions.
            ChannelOptions channelOp = new ChannelOptions();

            // Check if there is at least one item selected and the XtreamCodes checkbox is checked.
            if (e.AddedItems.Count > 0 && Instance.XtreamCodesChecked)
            {
                // Attempt to cast the selected item to a ChannelEntry.
                XtreamChannel xtreamChannel = e.AddedItems[0] as XtreamChannel;

                // If the cast is successful (i.e., the selected item is indeed a ChannelEntry).
                if (xtreamChannel != null)
                {
                    // Set the tempChannel property of the ChannelOptions instance.
                    channelOp.tempChannel = xtreamChannel;

                    // Now display the selected channel data in the ChannelOptions window.
                    if (channelOp.DisplaySelectedChannelData(xtreamChannel))
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

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
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
                        await XtreamCodes.UpdateChannelsEpgData(Instance.XtreamChannels);
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

    public class XtreamChannelModel
    {
        private bool isInitialized = false;

        public XtreamChannelModel()
        {
            MyListBoxItems = new ObservableCollection<XtreamChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.XtreamCodesChecked)
            {
                // Get the category ID from the selected category name
                int selectedCategory = int.Parse(Instance.XtreamCategoryList
                    .FirstOrDefault(category => category.CategoryName == Instance.selectedCategory).CategoryId);

                // Find all channels that belong to the selected category by CategoryId
                var channelsInCategory = Instance.XtreamChannels
                    .Where(channel => channel.CategoryIds.Contains(selectedCategory))
                    .ToList();

                foreach (var channel in channelsInCategory)
                {
                    MyListBoxItems.Add(channel);
                }
            }
            isInitialized = true;
        }
        public ObservableCollection<XtreamChannel> MyListBoxItems { get; set; }
    }
    public class XtreamMyMockClass
    {
        public XtreamMyMockClass()
        {
            MyListBoxItems = new ObservableCollection<XtreamChannel>();
            MyListBoxItems.Add(new XtreamChannel()
            {
                ChannelName = "|FR| TF1 UHD",
                LogoUrl = "http://f.iptv-pure.com/tf14k.png",
                EPGData = new XtreamEPGData
                {
                    ProgramTitle = "Title 1",
                    Description = "Description 1",
                    StartTime = DateTime.Now,
                }
            });
            MyListBoxItems.Add(new XtreamChannel()
            {
                ChannelName = "|FR| CSTAR FHD",
                LogoUrl = "http://f.iptv-pure.com/cstar.png",
                EPGData = new XtreamEPGData
                {
                    ProgramTitle = "Title 2",
                    Description = "Description 2",
                    StartTime = DateTime.Now.AddHours(1),
                }
            });
        }
        public ObservableCollection<XtreamChannel> MyListBoxItems { get; set; }

    }
}
