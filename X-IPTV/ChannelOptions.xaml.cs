using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

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
            string vlcLocatedPath = "";
            string vlcX64path = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            string vlcX86path = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";

            if (File.Exists(vlcX86path))
                vlcLocatedPath = vlcX86path;
            else if (File.Exists(vlcX64path))
                vlcLocatedPath = vlcX64path;
            else
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "VLC Executable File (*.exe)|*.exe";
                openFileDialog1.RestoreDirectory = true;

                bool? result = openFileDialog1.ShowDialog();

                if (result == true)
                    vlcLocatedPath = openFileDialog1.FileName;
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(vlcLocatedPath, Instance.playlistDataMap[tempCE.stream_id.ToString()].stream_url);

            string streamURL = Instance.playlistDataMap[tempCE.stream_id.ToString()].stream_url;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C \"{vlcLocatedPath}\" {streamURL}";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process processTemp = new Process();
            processTemp.StartInfo = startInfo;
            processTemp.EnableRaisingEvents = true;
            processTemp.Start();
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

            foreach (PropertyInfo ce in typeof(ChannelEntry).GetProperties())
            {
                richTextBox.AppendText(ce.Name + ": " + ce.GetValue(entry) + "\r");
            }

            /*foreach (PropertyInfo pd in typeof(PlaylistData).GetProperties())
            {
                richTextBox.AppendText(pd.Name + ": " + pd.GetValue(pdTest) + "\r");
            }*/

            //richTextBox.AppendText(Instance.playlistDataMap[entry.stream_id.ToString()].stream_url);

        }
    }
}
