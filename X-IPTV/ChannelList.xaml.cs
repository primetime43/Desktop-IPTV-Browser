using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            //MessageBox.Show("Hello, world!", "My App");

            //open a window with a textbox to copy the url and see info etc.
            //Then have a button to say do you want to open in vlc, yes or no

            ChannelOptions channelOp = new ChannelOptions();

;
            if (e.AddedItems.Count > 1) 
                return;

            ChannelEntry entry = e.AddedItems[0] as ChannelEntry;


            Console.WriteLine(Instance.playlistDataMap[entry.stream_id.ToString()].stream_url);

            channelOp.displaySelectedChannelData(Instance.playlistDataMap, entry);
            channelOp.Show();


            /*ProcessStartInfo processStartInfo = new ProcessStartInfo(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe", Instance.playlistDataMap[entry.stream_id.ToString()].stream_url);

            string streamURL = Instance.playlistDataMap[entry.stream_id.ToString()].stream_url;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C \"C:/Program Files (x86)/VideoLAN/VLC/vlc.exe\" {streamURL}";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process processTemp = new Process();
            processTemp.StartInfo = startInfo;
            processTemp.EnableRaisingEvents = true;
            processTemp.Start();*/
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
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
            for (int i = 0; i < Instance.ChannelsArray.Length; i++)
            {
                //Loads each ChannelEntry Object into the list box
                MyListBoxItems.Add(Instance.ChannelsArray[i]);
            }
        }

        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }
    }

    public class MyMockClass
    {
        public MyMockClass()
        {
            MyListBoxItems = new ObservableCollection<ChannelEntry>();
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| TF1 UHD", stream_icon = "http://f.iptv-pure.com/tf14k.png" });
            MyListBoxItems.Add(new ChannelEntry() { name = "|FR| CSTAR FHD", stream_icon = "http://f.iptv-pure.com/cstar.png" });
        }
        public ObservableCollection<ChannelEntry> MyListBoxItems { get; set; }
    }
}
