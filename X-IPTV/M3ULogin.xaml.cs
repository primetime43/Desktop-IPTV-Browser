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
    }
}
