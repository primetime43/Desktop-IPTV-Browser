using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using X_IPTV.Utilities;
using static X_IPTV.Service.GitHubReleaseChecker;

namespace X_IPTV.Views
{
    public partial class SettingsPage : Page
    {
        public bool IsVlcDefault { get; set; }
        public bool IsGenericPlayerDefault { get; set; }
        public SettingsPage()
        {
            InitializeComponent();
            LoadConfigPaths();
            InitializeAutoUpdate();
            LoadLastEPGUpdateTime();
            LoadDefaultPlayerSetting();
        }

        private void InitializeAutoUpdate()
        {
            bool autoUpdateEnabled = bool.TryParse(ConfigurationManager.GetSetting("autoUpdateEnabled"), out bool isEnabled) && isEnabled;
            autoUpdateCheckBox.IsChecked = autoUpdateEnabled;
            UpdateAutoUpdateIndicator(autoUpdateEnabled);

            int updateIntervalHours = int.TryParse(ConfigurationManager.GetSetting("epgUpdateIntervalHours"), out int interval) ? interval : 6;
            epgUpdateIntervalSlider.Value = updateIntervalHours;
        }

        private void LoadDefaultPlayerSetting()
        {
            string defaultPlayer = ConfigurationManager.GetSetting("defaultPlayer");
            IsVlcDefault = defaultPlayer == "vlc";
            IsGenericPlayerDefault = defaultPlayer == "generic";
            DataContext = this;
        }

        private void SaveDefaultPlayer_Btn_Click(object sender, RoutedEventArgs e)
        {
            string selectedPlayer = IsVlcDefault ? "vlc" : "generic";
            ConfigurationManager.UpdateSetting("defaultPlayer", selectedPlayer);
            Xceed.Wpf.Toolkit.MessageBox.Show($"Default player set to: {selectedPlayer}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateAutoUpdateIndicator(bool enabled)
        {
            autoUpdateIndicator.Text = enabled ? "Auto-Update: On" : "Auto-Update: Off";
            autoUpdateIndicator.Foreground = new SolidColorBrush(enabled ? Colors.LimeGreen : Colors.Red);
        }

        private void LoadLastEPGUpdateTime()
        {
            string lastEpgUpdateTimeIso = ConfigurationManager.GetSetting("lastEpgDataLoadTime");
            if (DateTimeOffset.TryParse(lastEpgUpdateTimeIso, out var lastEpgUpdateTime))
            {
                lastEPGUpdateLbl.Text = "Last EPG Update: " + lastEpgUpdateTime.ToLocalTime().ToString("MMMM dd, yyyy, h:mm tt");
            }
            else
            {
                lastEPGUpdateLbl.Text = "Last EPG Update: Not Available";
            }
        }

        private void AutoUpdateCheckBox_Checked(object sender, RoutedEventArgs e) => SetAutoUpdate(true);
        private void AutoUpdateCheckBox_Unchecked(object sender, RoutedEventArgs e) => SetAutoUpdate(false);

        private void SetAutoUpdate(bool enabled)
        {
            ConfigurationManager.UpdateSetting("autoUpdateEnabled", enabled.ToString());
            UpdateAutoUpdateIndicator(enabled);
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
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(
                        $"Current version: {programVersion}\nNew release available: {release.name} ({release.tag_name})\nDo you want to download it?",
                        "New Release", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo { FileName = ReleaseChecker.releaseURL, UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show($"An error occurred: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("You are using the latest version.");
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No new releases available.");
            }
        }

        private void checkForUpdate_Btn_Click(object sender, RoutedEventArgs e) => checkForUpdate();

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
                    UpdateLastEPGUpdateTime();
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Failed to update EPG data. Please check your connection and try again.");
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"EPG Not Updated! Error: {ex.Message}");
            }
        }

        private void UpdateLastEPGUpdateTime()
        {
            var localTime = DateTimeOffset.UtcNow.ToLocalTime();
            string formattedTime = localTime.ToString("MMMM dd, yyyy, h:mm tt");
            lastEPGUpdateLbl.Text = "Last EPG Update: " + formattedTime;
            // Update the lastEpgDataLoadTime setting with the local machine's current date and time in ISO 8601 format
            ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.Now.ToString("o"));
        }

        private void LoadConfigPaths()
        {
            CheckForPlayerPath();
            CheckForVLCPath();
            setUsersFolderPath_Input.Text = ConfigurationManager.GetSetting("usersFolderPath") ?? "Users folder path not set.";
            setEpgDataFolderPath_Input.Text = ConfigurationManager.GetSetting("epgDataFolderPath") ?? "EPG Data folder path not set.";
            setM3UPlaylistsPath_Input.Text = ConfigurationManager.GetSetting("M3UFolderPath") ?? "M3U Playlists folder path not set.";
        }

        private void CheckForPlayerPath()
        {
            var playerPath = ConfigurationManager.GetSetting("genericPlayerPath") ?? ConfigurationManager.GetPlayerPath("genericPlayerPath");
            playerLocation_Input.Text = string.IsNullOrEmpty(playerPath) ? "Player not found" : playerPath;
        }

        private void CheckForVLCPath()
        {
            var vlcPath = ConfigurationManager.GetSetting("vlcLocationPath") ?? ConfigurationManager.GetPlayerPath("vlcLocationPath");
            vlcLocation_Input.Text = string.IsNullOrEmpty(vlcPath) ? "VLC not found" : vlcPath;
        }

        private void EpgUpdateIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int intervalHours = (int)e.NewValue;
            ConfigurationManager.UpdateSetting("epgUpdateIntervalHours", intervalHours.ToString());
            Debug.WriteLine($"EPG update interval set to {intervalHours} hours");
        }

        private void SetPathFromDialog(TextBox targetTextBox, string settingKey, Func<(string FullPath, bool Selected)> dialogFunc, string successMessage)
        {
            var selection = dialogFunc();
            if (selection.Selected)
            {
                ConfigurationManager.UpdateSetting(settingKey, selection.FullPath);
                targetTextBox.Text = selection.FullPath;
                Xceed.Wpf.Toolkit.MessageBox.Show(successMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No path was selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetFolderPath(string settingKey, TextBox targetTextBox, string successMessage)
        {
            var folderSelection = DialogHelpers.FolderDialogSelectFolder();
            var folderPath = folderSelection.FullPath;

            if (!string.IsNullOrEmpty(folderPath))
            {
                ConfigurationManager.UpdateSetting(settingKey, folderPath);
                targetTextBox.Text = folderPath; // Update the TextBox directly
                Xceed.Wpf.Toolkit.MessageBox.Show(successMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please select a valid folder path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handlers for each button using the helper method

        private void setVLCpath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetPathFromDialog(vlcLocation_Input, "vlcLocationPath", () => DialogHelpers.FileDialogSelectFile("Executable Files (*.exe)|*.exe"), "VLC path saved successfully.");

        private void setPlayerPath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetPathFromDialog(playerLocation_Input, "genericPlayerPath", () => DialogHelpers.FileDialogSelectFile("Executable Files (*.exe)|*.exe"), "Player path saved successfully.");

        private void setUsersFolderPath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetFolderPath("usersFolderPath", setUsersFolderPath_Input, "Users folder path saved successfully.");

        private void setEpgDataFolderPath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetFolderPath("epgDataFolderPath", setEpgDataFolderPath_Input, "EPG Data folder path saved successfully.");

        private void setM3UPlaylistsPath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetFolderPath("M3UFolderPath", setM3UPlaylistsPath_Input, "M3U Playlists path saved successfully.");
    }
}
