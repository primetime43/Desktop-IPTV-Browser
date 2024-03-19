using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using X_IPTV.Navigation;
using static X_IPTV.Service.UserDataSaver;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;
using X_IPTV.Service;
using X_IPTV.Utilities;

namespace X_IPTV.Views
{
    /// <summary>
    /// Interaction logic for M3ULogin.xaml
    /// </summary>
    public partial class M3ULogin : Page
    {
        public M3ULogin()
        {
            InitializeComponent();
        }

        private void M3U_loadButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the M3UFolderPath from the configuration
            string M3UFolderPath = ConfigurationManager.GetSetting("M3UFolderPath");
            if (string.IsNullOrEmpty(M3UFolderPath))
            {
                // If M3UFolderPath is not set, fall back to a default path
                M3UFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "M3U");
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "M3U JSON Files (*.json)|*.json", // Adjusted filter to select JSON files
                InitialDirectory = M3UFolderPath // Use the M3UFolderPath from configuration
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Extract the playlist name from the selected file's name (without extension)
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                // Use the filename without extension as the playlistName parameter
                M3UData m3uData = UserDataSaver.GetM3UData(filenameWithoutExtension);

                if (m3uData != null)
                {
                    m3uURLTxtbox.Text = m3uData.PlaylistURL;
                    m3uEpgUrlTxtbox.Text = m3uData.EPGURL;
                    Xceed.Wpf.Toolkit.MessageBox.Show(filenameWithoutExtension + " loaded successfully.");
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Failed to load M3U data.");
                }
            }
        }

        private void M3U_saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(m3uURLTxtbox.Text))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("The playlist URL is empty.");
                return;
            }

            InputDialog inputDialog = new InputDialog("Save Playlist", "Please enter a name for the playlist:", "DefaultPlaylistName");
            bool? dialogResult = inputDialog.ShowDialog();

            // Check if the dialog was accepted
            if (dialogResult == true)
            {
                string playlistName = inputDialog.InputResult;

                if (!string.IsNullOrWhiteSpace(playlistName))
                {
                    M3UData m3uData = new M3UData
                    {
                        PlaylistURL = m3uURLTxtbox.Text,
                        EPGURL = m3uEpgUrlTxtbox.Text
                    };

                    SaveM3UData(m3uData, playlistName);
                    Xceed.Wpf.Toolkit.MessageBox.Show("Playlist saved successfully as " + playlistName);
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("You must enter a name for the playlist.");
                }
            }
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private async void Con_btn_Click(object sender, RoutedEventArgs e)
        {
            Instance.M3uChecked = true;

            busy_ind.IsBusy = true;
            busy_ind.BusyContent = "Attempting to connect...";
            try
            {
                busy_ind.BusyContent = "Loading playlist channel data...";
                await M3UPlaylist.RetrieveM3UPlaylistData(m3uURLTxtbox.Text, _cts.Token); // Load epg into the channels array

                //Only attempt to download the epg data if the epg url is not empty
                if (string.IsNullOrWhiteSpace(m3uEpgUrlTxtbox.Text))
                {
                    busy_ind.BusyContent = "Loading playlist epg data...";
                    Instance.allM3uEpgData = await M3UPlaylist.DownloadAndParseEPG(m3uEpgUrlTxtbox.Text, _cts.Token);
                    if (Instance.allM3uEpgData != null)
                    {
                        //await M3UPlaylist.MatchChannelsWithEPG(epgData, Instance.M3UChannels);
                        await M3UPlaylist.UpdateChannelsEpgData(Instance.M3UChannels);
                    }

                    //var epgDataForChannel = Instance.M3UEPGDataList.Where(e => e.ChannelId == "someChannelId").ToList();

                    // Update the lastEpgDataLoadTime setting with the current date and time
                    ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.Now.ToString("o"));
                }

                busy_ind.IsBusy = false;
                if (!_cts.IsCancellationRequested)
                {
                    var navigationManager = new NavigationManager(this.NavigationService);
                    navigationManager.NavigateToPage("CategoriesPage");
                }
                else
                {
                    busy_ind.IsBusy = false;
                    busy_ind.BusyContent = ""; // Clear the busy content if the connection fails
                }
            }
            catch (OperationCanceledException) { }

            _cts = new CancellationTokenSource();//reset the token
        }
    }
}
