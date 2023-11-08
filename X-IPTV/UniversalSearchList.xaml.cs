using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static X_IPTV.XtreamCodes;
using static X_IPTV.M3UPlaylist;
using System.Collections;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UniversalSearchList : Window
    {
        private UniversalSearchModel model;
        public UniversalSearchList()
        {
            InitializeComponent();
            model = new UniversalSearchModel();
            model.Initialize(); // populate the allChannels list
        }

        private void USearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (USearchTextBox != null)
            {
                var filterText = USearchTextBox.Text.ToLower();

                IList currentList = Instance.XtreamCodesChecked
                    ? (IList)model.MyXtreamListBoxItems
                    : (IList)model.MyM3uListBoxItems;

                currentList.Clear();
                var searchResult = Instance.XtreamCodesChecked
                    ? model.SearchXtreamChannels(filterText).Cast<object>()
                    : model.SearchM3uChannels(filterText).Cast<object>();

                foreach (var item in searchResult)
                {
                    currentList.Add(item);
                }

                // The UI will now update to show only filtered items
                USearchChannelLst.ItemsSource = currentList;
            }
        }

        private void USearchChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            else if(e.AddedItems.Count > 0 && Instance.M3uChecked)
            {
                M3UChannel m3uChannel = e.AddedItems[0] as M3UChannel;

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
    }

    public class UniversalSearchModel
    {
        private bool isInitialized = false;
        private List<XtreamChannel> allXtreamChannels;
        private List<M3UChannel> allM3uChannels;

        public ObservableCollection<XtreamChannel> MyXtreamListBoxItems { get; set; }
        public ObservableCollection<M3UChannel> MyM3uListBoxItems { get; set; }

        public UniversalSearchModel()
        {
            MyXtreamListBoxItems = new ObservableCollection<XtreamChannel>();
            MyM3uListBoxItems = new ObservableCollection<M3UChannel>();
            allXtreamChannels = new List<XtreamChannel>();
            allM3uChannels = new List<M3UChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.XtreamCodesChecked)
            {
                allXtreamChannels.AddRange(Instance.XtreamChannels); // Load all channels, but do not add to MyListBoxItems
            }
            else if(Instance.M3uChecked)
            {
                allM3uChannels.AddRange(Instance.M3UChannels); // Load all channels, but do not add to MyListBoxItems
            }
            isInitialized = true;
        }

        public IEnumerable<XtreamChannel> SearchXtreamChannels(string filterText)
        {
            // Return an empty list if allXtreamChannels is null
            if (allXtreamChannels == null)
            {
                return Enumerable.Empty<XtreamChannel>();
            }

            // Use StringComparison.OrdinalIgnoreCase for a case-insensitive comparison that is also faster than converting strings to lower case.
            var comparisonType = StringComparison.OrdinalIgnoreCase;

            return allXtreamChannels
                .Where(channel => channel != null && // Check that the channel is not null
                    (channel.ChannelName != null && channel.ChannelName.IndexOf(filterText, comparisonType) >= 0) ||
                    (channel.EPGData != null && channel.EPGData.Description != null && channel.EPGData.Description.IndexOf(filterText, comparisonType) >= 0));
        }

        public IEnumerable<M3UChannel> SearchM3uChannels(string filterText)
        {
            // Return an empty list if allM3uChannels is null
            if (allM3uChannels == null)
            {
                return Enumerable.Empty<M3UChannel>();
            }

            // Use StringComparison.OrdinalIgnoreCase for a case-insensitive comparison that is also faster than converting strings to lower case.
            var comparisonType = StringComparison.OrdinalIgnoreCase;

            return allM3uChannels
                .Where(channel => channel != null && // Check that the channel is not null
                    (channel.ChannelName != null && channel.ChannelName.IndexOf(filterText, comparisonType) >= 0) ||
                    (channel.EPGData != null && channel.EPGData.Description != null && channel.EPGData.Description.IndexOf(filterText, comparisonType) >= 0));
        }
    }
    public class UniversalSearchMyMockClass
    {
        public UniversalSearchMyMockClass()
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
