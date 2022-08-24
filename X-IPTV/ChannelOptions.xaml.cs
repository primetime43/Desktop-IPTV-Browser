using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for ChannelOptions.xaml
    /// </summary>
    public partial class ChannelOptions : Window
    {
        public ChannelEntry tempCE = new ChannelEntry();
        public ChannelOptions()
        {
            InitializeComponent();
        }

        private void openVLCbtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe", Instance.playlistDataMap[tempCE.stream_id.ToString()].stream_url);

            string streamURL = Instance.playlistDataMap[tempCE.stream_id.ToString()].stream_url;

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
            processTemp.Start();

            //delete
            MessageBox.Show("Opening " + Instance.playlistDataMap[tempCE.stream_id.ToString()].tvg_name);
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void displaySelectedChannelData(Dictionary<string, PlaylistData> playlistDataMap, ChannelEntry entry)
        {
            this.Title = entry.name;
            this.Icon = new BitmapImage(new Uri(entry.stream_icon));
            tempCE = entry;
            richTextBox.Document.Blocks.Clear();

            streamURLtxtBox.Text = Instance.playlistDataMap[entry.stream_id.ToString()].stream_url;

            foreach(PropertyInfo ce in typeof(ChannelEntry).GetProperties())
            {
                richTextBox.AppendText(ce.Name + ": " + ce.GetValue(entry) + "\r");
            }

            //richTextBox.AppendText(Instance.playlistDataMap[entry.stream_id.ToString()].stream_url);

        }
    }
}
