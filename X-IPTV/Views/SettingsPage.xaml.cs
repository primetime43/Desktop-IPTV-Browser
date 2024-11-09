using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

            // Temporarily unsubscribe from the Checked and Unchecked events
            autoUpdateCheckBox.Checked -= AutoUpdateCheckBox_Checked;
            autoUpdateCheckBox.Unchecked -= AutoUpdateCheckBox_Unchecked;

            // Retrieve the auto-update setting from the configuration
            bool autoUpdateEnabled = bool.TryParse(ConfigurationManager.GetSetting("autoUpdateEnabled"), out bool isEnabled) && isEnabled;

            // Set the checkbox state based on the setting
            autoUpdateCheckBox.IsChecked = autoUpdateEnabled;

            // Update the indicator label based on the setting
            autoUpdateIndicator.Content = autoUpdateEnabled ? "Auto-Update: On" : "Auto-Update: Off";
            autoUpdateIndicator.Foreground = autoUpdateEnabled ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.Red);

            // Reattach the Checked and Unchecked event handlers after initialization
            autoUpdateCheckBox.Checked += AutoUpdateCheckBox_Checked;
            autoUpdateCheckBox.Unchecked += AutoUpdateCheckBox_Unchecked;

            // Load EPG Update Interval setting
            int updateIntervalHours = int.TryParse(ConfigurationManager.GetSetting("epgUpdateIntervalHours"), out int interval) ? interval : 6;
            epgUpdateIntervalSlider.Value = updateIntervalHours;

            // Retrieve the last EPG update time string from the configuration
            string lastEpgUpdateTimeIso = ConfigurationManager.GetSetting("lastEpgDataLoadTime");

            if (!string.IsNullOrEmpty(lastEpgUpdateTimeIso))
            {
                // Parse the ISO 8601 date-time string into a DateTimeOffset object
                DateTimeOffset lastEpgUpdateTime;
                if (DateTimeOffset.TryParse(lastEpgUpdateTimeIso, out lastEpgUpdateTime))
                {
                    // Convert UTC time to local time
                    var localTime = lastEpgUpdateTime.ToLocalTime();

                    // Format the DateTimeOffset to a more readable string
                    // Example: "March 19, 2024, 9:58 PM"
                    string formattedTime = localTime.ToString("MMMM dd, yyyy, h:mm tt");

                    lastEPGUpdateLbl.Content = "Last EPG Update: " + formattedTime;
                }
                else
                {
                    // Handle parsing error or set to a default value
                    lastEPGUpdateLbl.Content = "Time unavailable";
                }
            }
            else
            {
                lastEPGUpdateLbl.Content = "Not available";
            }
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

        // Auto Update based on the slider
        private void AutoUpdateEPGData()
        {
            DateTime now = DateTime.Now;
            string lastUpdateString = ConfigurationManager.GetSetting("lastEpgDataLoadTime");
            DateTime lastEpgDataUpdateTime;
            int updateIntervalHours = int.TryParse(ConfigurationManager.GetSetting("epgUpdateIntervalHours"), out int interval) ? interval : 6;

            // Attempt to parse the last EPG data update time from settings
            if (!DateTime.TryParse(lastUpdateString, out lastEpgDataUpdateTime))
            {
                Debug.WriteLine("Could not parse the last EPG data load time, setting to now.");
                lastEpgDataUpdateTime = DateTime.MinValue;
            }

            // Check if the time since the last update exceeds the configured interval
            if ((now - lastEpgDataUpdateTime).TotalHours >= updateIntervalHours)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        bool updatePerformed = false;
                        if (Instance.XtreamCodesChecked)
                        {
                            await XtreamCodes.UpdateChannelsEpgData(Instance.XtreamChannels);
                            Debug.WriteLine("EPG updated for XtreamCodes...");
                            updatePerformed = true;
                        }
                        else if (Instance.M3uChecked)
                        {
                            await M3UPlaylist.UpdateChannelsEpgData(Instance.M3UChannels);
                            Debug.WriteLine("EPG updated for M3U...");
                            updatePerformed = true;
                        }

                        if (updatePerformed)
                        {
                            ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.UtcNow.ToString("o"));
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

        private void AutoUpdateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Update the configuration setting to enable auto-update
            ConfigurationManager.UpdateSetting("autoUpdateEnabled", "true");

            // Check if autoUpdateIndicator is not null
            if (autoUpdateIndicator != null)
            {
                // Update the indicator label to show that auto-update is on
                autoUpdateIndicator.Content = "Auto-Update: On";
                autoUpdateIndicator.Foreground = new SolidColorBrush(Colors.LimeGreen);
                Debug.WriteLine("autoUpdateIndicator is on.");
            }
            else
            {
                // Handle the case where autoUpdateIndicator is null
                // For example, log an error or show a message to the user
                Debug.WriteLine("autoUpdateIndicator is null.");
            }
        }

        private void AutoUpdateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Update the configuration setting to disable auto-update
            ConfigurationManager.UpdateSetting("autoUpdateEnabled", "false");

            // Check if autoUpdateIndicator is not null
            if (autoUpdateIndicator != null)
            {
                // Update the indicator label to show that auto-update is off
                autoUpdateIndicator.Content = "Auto-Update: Off";
                autoUpdateIndicator.Foreground = new SolidColorBrush(Colors.Red);
                Debug.WriteLine("autoUpdateIndicator is off.");
            }
            else
            {
                // Handle the case where autoUpdateIndicator is null
                // For example, log an error or show a message to the user
                Debug.WriteLine("autoUpdateIndicator is null.");
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
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("EPG Updated!");

                    // Update the last EPG update time label
                    var localTime = DateTimeOffset.UtcNow.ToLocalTime();
                    string formattedTime = localTime.ToString("MMMM dd, yyyy, h:mm tt");
                    lastEPGUpdateLbl.Content = "Last EPG Update: " + formattedTime;

                    // Update the lastEpgDataLoadTime setting
                    ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.UtcNow.ToString("o"));
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Failed to update EPG data. Please check your connection and try again.");
                }
            }
            catch (Exception ex) // Maybe log to file or output log on settings page eventually
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"EPG Not Updated! Error: {ex.Message}");
            }
        }

        private void EpgUpdateIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int intervalHours = (int)e.NewValue;
            ConfigurationManager.UpdateSetting("epgUpdateIntervalHours", intervalHours.ToString());
            Debug.WriteLine($"EPG update interval set to {intervalHours} hours");
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
