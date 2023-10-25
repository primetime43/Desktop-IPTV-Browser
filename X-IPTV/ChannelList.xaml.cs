using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for ChannelList.xaml
    /// </summary>
    public partial class ChannelList : Window
    {
        public ChannelList()
        {
            InitializeComponent();

            ChannelModel model = new ChannelModel();
            model.Initialize();

            //the model is the array that holds all of the ChannelEntry Objects set in the ChannelModel class
            ChannelLst.DataContext = model;
        }

        private void ChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChannelOptions channelOp = new ChannelOptions();

;
            if (e.AddedItems.Count > 1) 
                return;

            ChannelEntry entry = e.AddedItems[0] as ChannelEntry;

            ChannelStreamData pdTest = e.AddedItems as ChannelStreamData;


            //Console.WriteLine(Instance.playlistDataMap[entry.stream_id.ToString()].stream_url);

            //fix
            if(channelOp.displaySelectedChannelData(entry))
                channelOp.Show();
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

    public class ChannelModel
    {
        private bool isInitialized = false;

        public ChannelModel()
        {
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
        }

        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

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

            isInitialized = true; // Mark as initialized
        }

        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }
    }

    public class MyMockClass
    {
        public MyMockClass()
        {
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| TF1 UHD", stream_icon = "http://f.iptv-pure.com/tf14k.png", title = "Title", start_end_timestamp = "Start Time" });
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| CSTAR FHD", stream_icon = "http://f.iptv-pure.com/cstar.png", title = "Title 2", start_end_timestamp = "Start Time 2" });
        }
        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }
    }
}
