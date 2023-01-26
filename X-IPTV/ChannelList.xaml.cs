using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            channelOp.displaySelectedChannelData(entry);
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
    }

    public class ChannelModel
    {
        public ChannelModel()
        {
            //load all channels
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
            foreach (ChannelGroups entry in Instance.ChannelGroupsArray)
            {
                if (Instance.selectedCategory == (entry.category_name + " - " + entry.category_id))
                {
                    string selectedCategoryID = entry.category_id.ToString();
                    //load epg data for select category here

                    List<ChannelEntry> channels = Instance.categoryToChannelMap[selectedCategoryID];
                    foreach (ChannelEntry channel in channels)
                    {
                        int streamId = channel.stream_id;
                        Debug.WriteLine(channel.name + " " + streamId);
                        // Do something with the streamId value
                        //with each get the epg for each channel (can eventually do this with category too instead of getting all live channels.player_api.php?username=X&password=X&action=get_live_streams&category_id=X (This will get All LIVE Streams in the selected category ONLY)
                        //await REST_Ops.GetEPGDataForIndividualChannel(channel.stream_id.ToString());
                        testLoadData(channel.stream_id.ToString());
                    }
                    Debug.WriteLine("");
                    //REST_Ops.GetEPGDataForIndividualChannel(selectedCategoryID);
                    //add each value from the categoryToChannelMap to the MyListBoxItems array
                    try
                    {
                        foreach (ChannelEntry channelEntry in Instance.categoryToChannelMap[selectedCategoryID])
                        {
                            MyListBoxItems.Add(channelEntry);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        MessageBox.Show("No channels found for this category.");
                    }
                }
            }
        }

        private async Task testLoadData(string streamID)
        {
            await REST_Ops.GetEPGDataForIndividualChannel(streamID);
        }

        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }
    }

    public class MyMockClass
    {
        public MyMockClass()
        {
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| TF1 UHD", stream_icon = "http://f.iptv-pure.com/tf14k.png", title = "Title Test", start_timestamp = "Hello World" });
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| CSTAR FHD", stream_icon = "http://f.iptv-pure.com/cstar.png", title = "Title Test 2", start_timestamp = "Hello World 2" });
        }
        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }
    }
}
