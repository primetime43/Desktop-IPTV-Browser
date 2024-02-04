using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Controls;
using static X_IPTV.M3UPlaylist;
using static X_IPTV.XtreamCodes;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for ChannelOptions.xaml
    /// </summary>
    public partial class ChannelOptions : Window
    {
        public object tempChannel;
        public ChannelOptions(object selectedChannel)
        {
            InitializeComponent();
            tempChannel = selectedChannel;
            DisplaySelectedChannelData();
        }

        private void openVLCbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string vlcLocatedPath = "";
                string vlcX64path = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
                string vlcX86path = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";

                if (File.Exists(vlcX86path))
                {
                    vlcLocatedPath = vlcX86path;
                }
                else if (File.Exists(vlcX64path))
                {
                    vlcLocatedPath = vlcX64path;
                }
                else
                {
                    OpenFileDialog openFileDialog1 = new OpenFileDialog
                    {
                        InitialDirectory = "c:\\",
                        Filter = "VLC Executable File (*.exe)|*.exe",
                        RestoreDirectory = true
                    };

                    bool? result = openFileDialog1.ShowDialog();

                    if (result == true)
                    {
                        vlcLocatedPath = openFileDialog1.FileName;
                    }
                }

                if (!string.IsNullOrEmpty(vlcLocatedPath))
                {
                    string streamURL = "";

                    if (tempChannel is XtreamChannel xtreamChannel)
                    {
                        streamURL = xtreamChannel.StreamUrl;
                    }
                    else if (tempChannel is M3UChannel m3uChannel)
                    {
                        streamURL = m3uChannel.StreamUrl;
                    }

                    if (!string.IsNullOrEmpty(streamURL))
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/C \"{vlcLocatedPath}\" {streamURL}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        Process.Start(startInfo);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("Stream URL not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("VLC path not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Failed to open VLC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool DisplaySelectedChannelData()
        {
            try
            {
                // Clear any existing content
                richTextBox.Document.Blocks.Clear();
                streamURLtxtBox.Text = string.Empty;

                // Check if the passed object is a ChannelEntry
                if (this.tempChannel is XtreamChannel xtreamChannel)
                {
                    // Set properties specific to ChannelEntry
                    //DisplayChannelEntryDetails(entry); //delete?
                    DisplayXtreamChannelDetails(xtreamChannel);
                }
                // Check if the passed object is a M3UChannel
                else if (this.tempChannel is M3UChannel m3uChannel)
                {
                    // Set properties specific to M3UChannel
                    DisplayM3UChannelDetails(m3uChannel);
                }
                else
                {
                    throw new ArgumentException("Invalid channel type", nameof(this.tempChannel));
                }
                return true;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("An error occurred: " + ex.Message);
                return false;
            }
        }

        private void DisplayM3UChannelDetails(M3UChannel m3uChannel)
        {
            this.Title = m3uChannel.ChannelName;
            //this.Icon = new BitmapImage(new Uri(m3uChannel.LogoUrl));

            streamURLtxtBox.Text = m3uChannel.StreamUrl ?? "URL not available";

            // Assuming you want to display the EPG data in the RichTextBox
            if (m3uChannel.EPGData != null)
            {
                richTextBox.AppendText("Title: " + m3uChannel.EPGData.ProgramTitle + "\r");
                richTextBox.AppendText("Description: " + m3uChannel.EPGData.Description + "\r");
                richTextBox.AppendText("Start Time: " + m3uChannel.EPGData.StartTime.ToString() + "\r");
                richTextBox.AppendText("End Time: " + m3uChannel.EPGData.EndTime.ToString() + "\r");
            }
        }

        private void DisplayXtreamChannelDetails(XtreamChannel xtreamChannel)
        {
            this.Title = xtreamChannel.ChannelName;
            //this.Icon = new BitmapImage(new Uri(xtreamChannel.LogoUrl));

            streamURLtxtBox.Text = xtreamChannel.StreamUrl ?? "URL not available";

            // Assuming you want to display the EPG data in the RichTextBox
            if (xtreamChannel.EPGData != null)
            {
                richTextBox.AppendText("Title: " + xtreamChannel.EPGData.ProgramTitle + "\r");
                richTextBox.AppendText("Description: " + xtreamChannel.EPGData.Description + "\r");
                richTextBox.AppendText("Start Time: " + xtreamChannel.EPGData.StartTime.ToString() + "\r");
                richTextBox.AppendText("End Time: " + xtreamChannel.EPGData.EndTime.ToString() + "\r");
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }

        public static string convertUnixToRealTIme(int unixTime)
        {
            //var realTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local).AddSeconds(unixTime).ToLocalTime();

            DateTime realTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime);
            realTime = TimeZoneInfo.ConvertTimeFromUtc(realTime, TimeZoneInfo.Local);

            return realTime.ToString("h:mm tt MM-dd-yyyy");
        }
    }
}
