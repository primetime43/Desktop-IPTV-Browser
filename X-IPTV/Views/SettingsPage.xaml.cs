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
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadConfigPaths();
            InitializeAutoUpdate();
            LoadLastEPGUpdateTime();
        }

        private void InitializeAutoUpdate()
        {
            bool autoUpdateEnabled = bool.TryParse(ConfigurationManager.GetSetting("autoUpdateEnabled"), out bool isEnabled) && isEnabled;
            autoUpdateCheckBox.IsChecked = autoUpdateEnabled;
            UpdateAutoUpdateIndicator(autoUpdateEnabled);

            int updateIntervalHours = int.TryParse(ConfigurationManager.GetSetting("epgUpdateIntervalHours"), out int interval) ? interval : 6;
            epgUpdateIntervalSlider.Value = updateIntervalHours;
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
                    MessageBoxResult result = MessageBox.Show(
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
                            MessageBox.Show($"An error occurred: {ex.Message}");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You are using the latest version.");
                }
            }
            else
            {
                MessageBox.Show("No new releases available.");
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
                    MessageBox.Show("EPG Updated!");
                    UpdateLastEPGUpdateTime();
                }
                else
                {
                    MessageBox.Show("Failed to update EPG data. Please check your connection and try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"EPG Not Updated! Error: {ex.Message}");
            }
        }

        private void UpdateLastEPGUpdateTime()
        {
            var localTime = DateTimeOffset.UtcNow.ToLocalTime();
            string formattedTime = localTime.ToString("MMMM dd, yyyy, h:mm tt");
            lastEPGUpdateLbl.Text = "Last EPG Update: " + formattedTime;
            ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.UtcNow.ToString("o"));
        }

        private void LoadConfigPaths()
        {
            CheckForVLCPath();
            setUsersFolderPath_Input.Text = ConfigurationManager.GetSetting("usersFolderPath") ?? "Users folder path not set.";
            setEpgDataFolderPath_Input.Text = ConfigurationManager.GetSetting("epgDataFolderPath") ?? "EPG Data folder path not set.";
            setM3UPlaylistsPath_Input.Text = ConfigurationManager.GetSetting("M3UFolderPath") ?? "M3U Playlists folder path not set.";
        }

        private void CheckForVLCPath()
        {
            var vlcPath = ConfigurationManager.GetSetting("vlcLocationPath") ?? ConfigurationManager.GetVLCPath();
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
                MessageBox.Show(successMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No path was selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void setVLCpath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetPathFromDialog(vlcLocation_Input, "vlcLocationPath", () => DialogHelpers.FileDialogSelectFile("Executable Files (*.exe)|*.exe"), "VLC path saved successfully.");

        private void SetFolderPath(string settingKey, string successMessage)
        {
            var folderSelection = DialogHelpers.FolderDialogSelectFolder();
            var folderPath = folderSelection.FullPath;

            if (!string.IsNullOrEmpty(folderPath))
            {
                ConfigurationManager.UpdateSetting(settingKey, folderPath);
                Xceed.Wpf.Toolkit.MessageBox.Show(successMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please select a valid folder path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handlers for each button using the helper method

        private void setUsersFolderPath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetFolderPath("usersFolderPath", "Users folder path saved successfully.");

        private void setEpgDataFolderPath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetFolderPath("epgDataFolderPath", "EPG Data folder path saved successfully.");

        private void setM3UPlaylistsPath_Btn_Click(object sender, RoutedEventArgs e) =>
            SetFolderPath("M3UFolderPath", "M3U Playlists folder path saved successfully.");
    }
}
