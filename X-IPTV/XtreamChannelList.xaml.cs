using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static X_IPTV.M3UPlaylist;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for XtreamChannelList.xaml
    /// </summary>
    public partial class XtreamChannelList : Window
    {
        public XtreamChannelList()
        {
            InitializeComponent();

            XtreamChannelModel model = new XtreamChannelModel();
            model.Initialize();

            if (Instance.XstreamCodesChecked)
            {
                XtreamChannelLst.DataContext = model;
            }
        }

        private void XtreamChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Create an instance of ChannelOptions.
            ChannelOptions channelOp = new ChannelOptions();

            // Check if there is at least one item selected and the XtreamCodes checkbox is checked.
            if (e.AddedItems.Count > 0 && Instance.XstreamCodesChecked)
            {
                // Attempt to cast the selected item to a ChannelEntry.
                ChannelEntry entry = e.AddedItems[0] as ChannelEntry;

                // If the cast is successful (i.e., the selected item is indeed a ChannelEntry).
                if (entry != null)
                {
                    // Set the tempChannel property of the ChannelOptions instance.
                    channelOp.tempChannel = entry;

                    // Now display the selected channel data in the ChannelOptions window.
                    if (channelOp.DisplaySelectedChannelData(entry))
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
            ChannelNav channelNavWindow = new ChannelNav();
            channelNavWindow.Show();
        }
    }

    public class XtreamChannelModel
    {
        private bool isInitialized = false;

        public XtreamChannelModel()
        {
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
            M3UListBoxItems = new ObservableCollection<M3UChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.XstreamCodesChecked)
            {
                // Existing logic to load XstreamCodes channels
                foreach (ChannelGroups entry in Instance.ChannelGroupsArray)
                {
                    if (Instance.selectedCategory == entry.category_name)
                    {
                        string selectedCategoryID = entry.category_id.ToString();
                        try
                        {
                            foreach (ChannelEntry channelEntry in Instance.categoryToChannelMap[selectedCategoryID])
                            {
                                MyListBoxItems.Add(channelEntry);
                            }
                        }
                        catch (Exception e)
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show("No channels found for this category.");
                        }
                    }
                }
            }

            isInitialized = true;
        }

        public ObservableCollection<M3UChannel> M3UListBoxItems { get; set; }
        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }
    }
    public class XtreamMyMockClass
    {
        public XtreamMyMockClass()
        {
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| TF1 UHD", stream_icon = "http://f.iptv-pure.com/tf14k.png", title = "Title", start_end_timestamp = "Start Time" });
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| CSTAR FHD", stream_icon = "http://f.iptv-pure.com/cstar.png", title = "Title 2", start_end_timestamp = "Start Time 2" });
        }
        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }

    }
}
