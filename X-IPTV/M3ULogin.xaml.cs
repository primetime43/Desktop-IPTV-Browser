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
using static X_IPTV.UserDataSaver;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;

namespace X_IPTV
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "M3U Files (*.txt)|*.txt",
                InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "M3U")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                M3UData m3uData = GetM3UData(filenameWithoutExtension);

                if (m3uData != null)
                {
                    m3uURLTxtbox.Text = m3uData.PlaylistURL;
                    m3uEpgUrlTxtbox.Text = m3uData.EPGURL;
                    Xceed.Wpf.Toolkit.MessageBox.Show(filenameWithoutExtension + " loaded successfully.");
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

            string playlistName = Microsoft.VisualBasic.Interaction.InputBox("Please enter a name for the playlist:", "Save Playlist", "DefaultPlaylistName");

            // Check if the InputBox was cancelled by the user.
            if (playlistName == "")
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(playlistName))
            {
                M3UData m3uData = new M3UData
                {
                    PlaylistURL = m3uURLTxtbox.Text,
                    EPGURL = m3uEpgUrlTxtbox.Text
                };

                SaveM3UData(m3uData, playlistName);
                MessageBox.Show("Playlist saved successfully as " + playlistName);
            }
            else
            {
                MessageBox.Show("You must enter a name for the playlist.");
            }
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private async void Con_btn_Click(object sender, RoutedEventArgs e)
        {
            Instance.M3uChecked = true;

            if (string.IsNullOrWhiteSpace(m3uEpgUrlTxtbox.Text))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please enter the M3U playlist url.");
                return;
            }

            busy_ind.IsBusy = true;
            busy_ind.BusyContent = "Attempting to connect...";
            try
            {
                busy_ind.BusyContent = "Loading playlist channel data...";
                await M3UPlaylist.RetrieveM3UPlaylistData(m3uURLTxtbox.Text, _cts.Token); // Load epg into the channels array

                busy_ind.BusyContent = "Loading playlist epg data...";
                var epgData = await M3UPlaylist.DownloadAndParseEPG(m3uEpgUrlTxtbox.Text, _cts.Token);
                if (epgData != null)
                {
                    await M3UPlaylist.MatchChannelsWithEPG(epgData, Instance.M3UChannels); //needs fixed
                }

                //var epgDataForChannel = Instance.M3UEPGDataList.Where(e => e.ChannelId == "someChannelId").ToList();

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
