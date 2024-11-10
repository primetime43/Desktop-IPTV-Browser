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
using Microsoft.Xaml.Behaviors.Media;
using X_IPTV.Utilities;

namespace X_IPTV.Views
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
            string streamURL = GetStreamURL(); // Retrieve the stream URL from the selected channel.
            if (!string.IsNullOrEmpty(streamURL))
            {
                OpenStreamInVLC(streamURL); // Call the static method with the stream URL.
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Stream URL is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public static void OpenStreamInVLC(string streamUrl = "")
        {
            try
            {
                string vlcLocatedPath = ConfigurationManager.GetVLCPath(); // Use the dedicated method to get or find the VLC path

                if (string.IsNullOrEmpty(vlcLocatedPath) || !File.Exists(vlcLocatedPath))
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        InitialDirectory = "c:\\",
                        Filter = "VLC Executable File (*.exe)|*.exe",
                        RestoreDirectory = true
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        vlcLocatedPath = openFileDialog.FileName;
                        // Optionally, update the configuration with the newly selected path
                        ConfigurationManager.UpdateSetting("vlcLocationPath", vlcLocatedPath);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("VLC path selection was canceled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(streamUrl))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C \"{vlcLocatedPath}\" {streamUrl} --loop", // --loop here to enable looping
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
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Failed to open VLC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetStreamURL()
        {
            if (tempChannel is XtreamChannel xtreamChannel)
            {
                return xtreamChannel.StreamUrl;
            }
            else if (tempChannel is M3UChannel m3uChannel)
            {
                return m3uChannel.StreamUrl;
            }

            return null;
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
            this.Icon = m3uChannel.LogoUrl != null ? new BitmapImage(new Uri(m3uChannel.LogoUrl)) : null;
            ChannelIcon_Image.Source = this.Icon;

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
            this.Icon = xtreamChannel.LogoUrl != null ? new BitmapImage(new Uri(xtreamChannel.LogoUrl)) : null;
            ChannelIcon_Image.Source = this.Icon;

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
