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
                var filteredItems = this.model.SearchChannels(filterText);

                // Clear the existing items and add the filtered items
                model.MyListBoxItems.Clear();
                foreach (var item in filteredItems)
                {
                    model.MyListBoxItems.Add(item);
                }

                USearchChannelLst.ItemsSource = model.MyListBoxItems; // The UI will now update to show only filtered items
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

        //might be able to remove this
        private void Window_Closed(object sender, EventArgs e)
        {
            CategoryNav categoryNavWindow = new CategoryNav();
            categoryNavWindow.Show();
        }
    }

    public class UniversalSearchModel
    {
        private bool isInitialized = false;
        private List<XtreamChannel> allChannels;

        public ObservableCollection<XtreamChannel> MyListBoxItems { get; set; }

        public UniversalSearchModel()
        {
            MyListBoxItems = new ObservableCollection<XtreamChannel>();
            allChannels = new List<XtreamChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.XtreamCodesChecked)
            {
                allChannels.AddRange(Instance.XtreamChannels); // Load all channels, but do not add to MyListBoxItems
            }
            /*else if(Instance.M3uChecked)
            {
                allChannels.AddRange(Instance.M3UChannels); // Load all channels, but do not add to MyListBoxItems
            }*/
            isInitialized = true;
        }

        public IEnumerable<XtreamChannel> SearchChannels(string filterText)
        {
            if (allChannels == null)
            {
                return Enumerable.Empty<XtreamChannel>(); // Return an empty list if allChannels is null
            }

            return allChannels
                .Where(channel => (channel.ChannelName?.ToLower().Contains(filterText) ?? false) ||
                                  (channel.EPGData?.Description.ToLower().Contains(filterText) ?? false));
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
