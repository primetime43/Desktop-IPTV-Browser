using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using X_IPTV.Utilities;
using static X_IPTV.Service.GitHubReleaseChecker;

namespace X_IPTV.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadConfigPaths();
        }

        private async void checkForUpdate()
        {
            var release = await ReleaseChecker.CheckForNewRelease("primetime43", "Xtream-Browser");
            string programVersion = Instance.programVersion;

            if (release != null)
            {
                int latestReleaseInt = ReleaseChecker.convertVersionToInt(release.tag_name);
                int localProgramVersionInt = ReleaseChecker.convertVersionToInt(programVersion);

                if (latestReleaseInt > localProgramVersionInt)
                {
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Current version: " + programVersion + "\nNew release available: " + release.name + " (" + release.tag_name + ")\nDo you want to download it?", "New Release", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = ReleaseChecker.releaseURL,
                                UseShellExecute = true
                            };

                            Process.Start(startInfo);
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
                else if(latestReleaseInt == localProgramVersionInt)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("You are using the latest version.");
                }
                else
                    Debug.WriteLine("Release null, skipping check.");
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No new releases available.");
            }
        }

        private void LoadConfigPaths()
        {
            CheckForVLCPath(); // Existing functionality to load VLC path

            // Load Users Folder Path
            var usersFolderPath = ConfigurationManager.GetSetting("usersFolderPath");
            setUsersFolderPath_Input.Text = !string.IsNullOrEmpty(usersFolderPath) ? usersFolderPath : "Users folder path not set.";

            // Load EPG Data Folder Path
            var epgDataFolderPath = ConfigurationManager.GetSetting("epgDataFolderPath");
            setEpgDataFolderPath_Input.Text = !string.IsNullOrEmpty(epgDataFolderPath) ? epgDataFolderPath : "EPG Data folder path not set.";

            // Load M3U Playlists Folder Path
            var m3uPlaylistsFolderPath = ConfigurationManager.GetSetting("M3UFolderPath");
            setM3UPlaylistsPath_Input.Text = !string.IsNullOrEmpty(m3uPlaylistsFolderPath) ? m3uPlaylistsFolderPath : "M3U Playlists folder path not set.";
        }

        private void CheckForVLCPath()
        {
            // Try to get the VLC path directly from the configuration first
            var vlcPath = ConfigurationManager.GetSetting("vlcLocationPath");

            // If the path doesn't exist or is empty, then use GetVLCPath to find it
            if (string.IsNullOrEmpty(vlcPath))
            {
                vlcPath = ConfigurationManager.GetVLCPath();
            }

            // After attempting to retrieve or find the VLC path, check if we have a valid path
            if (!string.IsNullOrEmpty(vlcPath))
            {
                vlcLocation_Input.Text = vlcPath;
            }
            else
            {
                // Handle the case where VLC is not found
                vlcLocation_Input.Text = "VLC not found";
            }
        }

        //only auto update the epg on 15 min increments if the window has been open that long
        //(actually this needs fixed because if no windows aren't open longer than 15 min increments, it won't
        //ever auto update; unless manually update is clicked)
        //Add an check eventually that checks the last time the epg data was updated and update it
        private void AutoUpdateEPGData() //not sure what to do with this yet
        {
            DateTime now = DateTime.Now;
            string lastUpdateString = ConfigurationManager.GetSetting("lastEpgDataLoadTime");
            DateTime lastEpgDataUpdateTime;

            // Attempt to parse the last EPG data update time from settings
            if (!DateTime.TryParse(lastUpdateString, out lastEpgDataUpdateTime))
            {
                Debug.WriteLine("Could not parse the last EPG data load time, setting to now.");
                lastEpgDataUpdateTime = DateTime.MinValue; // Or set to a default value that will trigger an update
            }

            // Check if it's been at least 15 minutes since the last update or other conditions for updating
            if ((now - lastEpgDataUpdateTime).TotalMinutes >= 15 || Instance.ShouldUpdateOnInterval(now))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        bool updatePerformed = false;
                        if (Instance.XtreamCodesChecked)
                        {
                            await XtreamCodes.UpdateChannelsEpgData(Instance.XtreamChannels);
                            Debug.WriteLine("EPG updated for XtreamCodes...");//eventually have a log file and write to that (maybe also show a log window on settings page)
                            updatePerformed = true;
                        }
                        else if (Instance.M3uChecked)
                        {
                            // Assume similar update method exists for M3U
                            await M3UPlaylist.UpdateChannelsEpgData(Instance.M3UChannels);
                            Debug.WriteLine("EPG updated for M3U...");
                            updatePerformed = true;
                        }

                        if (updatePerformed)
                        {
                            // Update the lastEpgDataLoadTime in settings to now
                            ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.Now.ToString("o"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show($"Error updating EPG: {ex.Message}");
                        });
                    }
                });
            }
            else
            {
                Debug.WriteLine("EPG not updated due to time constraints...");
            }
        }

        private void checkForUpdate_Btn_Click(object sender, RoutedEventArgs e)
        {
            checkForUpdate();
        }

        private async void updateEpgBtn_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            try
            {
                if (Instance.XtreamCodesChecked)
                    success = await XtreamCodes.UpdateChannelsEpgData(Instance.XtreamChannels);
                else if (Instance.M3uChecked)
                    success = await M3UPlaylist.UpdateChannelsEpgData(Instance.M3UChannels);

                if (success)
                    Xceed.Wpf.Toolkit.MessageBox.Show("EPG Updated!");
                else
                    Xceed.Wpf.Toolkit.MessageBox.Show("Failed to update EPG data. Please check your connection and try again.");
            }
            catch (Exception ex) // Maybe log to file or output log on settings page eventually
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"EPG Not Updated! Error: {ex.Message}");
            }
        }

        private void setVLCpath_Btn_Click(object sender, RoutedEventArgs e)
        {
            var fileSelection = DialogHelpers.FileDialogSelectFile("Executable Files (*.exe)|*.exe");

            if (fileSelection.FileSelected)
            {
                var vlcPath = fileSelection.FileFullPath;

                // Save the path to the configuration
                ConfigurationManager.UpdateSetting("vlcLocationPath", vlcPath);

                // Show a message box to inform the user that the path has been saved
                Xceed.Wpf.Toolkit.MessageBox.Show("VLC path saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the displayed VLC path in case it's displayed elsewhere or for confirmation
                CheckForVLCPath();
            }
            else
            {
                // Inform the user that no VLC path was selected
                Xceed.Wpf.Toolkit.MessageBox.Show("No VLC path was selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void setUsersFolderPath_Btn_Click(object sender, RoutedEventArgs e)
        {
            var folderSelection = DialogHelpers.FolderDialogSelectFolder();
            var usersFolderPath = folderSelection.FullPath;

            // Check if the path is neither null nor empty
            if (!string.IsNullOrEmpty(usersFolderPath))
            {
                ConfigurationManager.UpdateSetting("usersFolderPath", usersFolderPath);
                Xceed.Wpf.Toolkit.MessageBox.Show("Users folder path saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please select a valid Users folder path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void setEpgDataFolderPath_Btn_Click(object sender, RoutedEventArgs e)
        {
            var folderSelection = DialogHelpers.FolderDialogSelectFolder();
            var epgDataFolderPath = folderSelection.FullPath;

            if (!string.IsNullOrEmpty(epgDataFolderPath))
            {
                ConfigurationManager.UpdateSetting("epgDataFolderPath", epgDataFolderPath);
                Xceed.Wpf.Toolkit.MessageBox.Show("EPG Data folder path saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please enter a valid EPG Data folder path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void setM3UPlaylistsPath_Btn_Click(object sender, RoutedEventArgs e)
        {
            var folderSelection = DialogHelpers.FolderDialogSelectFolder();
            var m3uDataFolderPath = folderSelection.FullPath;

            if (!string.IsNullOrEmpty(m3uDataFolderPath))
            {
                ConfigurationManager.UpdateSetting("M3UFolderPath", m3uDataFolderPath);
                Xceed.Wpf.Toolkit.MessageBox.Show("M3U Playlists folder path saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please enter a valid M3U Playlists folder path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
