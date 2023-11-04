using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;
using static X_IPTV.M3UPlaylist;

namespace X_IPTV
{
    /*
     * player_api.php?action=get_live_streams
     * 
     * player_api.php?action=get_live_categories
     * 
     * xmltv.php xml data channel ids, display name, icon src. Has the desc for channels
     * 
     * get.php
     */
    public class XstreamCodes
    {
        private static HttpClient _client = new HttpClient();

        private static string _user;
        private static string _pass;
        private static string _server;
        private static string _port;
        private static bool _useHttps;
        //use get_live_categories for categories

        //not needed other than to get basic account info
        //keep for misc reasons and base url
        public static async Task<bool> CheckLoginConnection(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                _user = Instance.currentUser.username;
                _pass = Instance.currentUser.password;
                _server = Instance.currentUser.server;
                _port = Instance.currentUser.port;
                _useHttps = Instance.currentUser.useHttps;

                string url = $"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}";
                Debug.WriteLine("Request URL: " + url);

                // Create a request for the URL.
                WebRequest request = WebRequest.Create($"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}");

                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;

                // Get the response.
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = await reader.ReadToEndAsync();
                // Display the content.
                Debug.WriteLine("Response from server: " + responseFromServer);

                PlayerInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInfo>(responseFromServer);

                Instance.PlayerInfo = info;

                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();

                return true; // Connection was successful
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    using (Stream errorResponse = response.GetResponseStream())
                    {
                        if (errorResponse != null)
                        {
                            StreamReader errorReader = new StreamReader(errorResponse);
                            string errorResponseText = await errorReader.ReadToEndAsync();
                            // Display the content without HTML tags.
                            string textOnly = System.Text.RegularExpressions.Regex.Replace(errorResponseText, "<.*?>", "");
                            Xceed.Wpf.Toolkit.MessageBox.Show("Response from server: " + textOnly);
                        }
                    }
                }

                return false; // Connection was not successful
            }
            catch (OperationCanceledException)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Login Canceled.");
                return false;
            }
        }

        // Define a static HttpClient for the application's lifetime
        public static readonly HttpClient httpClient = new HttpClient();

        //testing for rewrite
        public static async Task GetEPGDataForIndividualChannel(ChannelEntry channel, CancellationToken token)
        {
            try
            {
                // Periodically check if cancellation is requested
                token.ThrowIfCancellationRequested();

                // Create a request for the URL.
                string requestUri = $"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}&action=get_simple_data_table&stream_id={channel.stream_id}";
                Debug.WriteLine("Request URL: " + requestUri);

                // Send the request and get the response
                HttpResponseMessage response = await httpClient.GetAsync(requestUri, token);
                response.EnsureSuccessStatusCode();

                // Deserialize the JSON content directly from the stream
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    Channel24hrEPG channel24hrEpgData = await System.Text.Json.JsonSerializer.DeserializeAsync<Channel24hrEPG>(stream, cancellationToken: token);

                    // Check if there are no EPG listings
                    if (channel24hrEpgData.epg_listings.Count == 0)
                    {
                        channel.title = "No information";
                        channel.desc = "No information";
                        return;
                    }

                    // Find the now playing entry
                    var nowPlaying = channel24hrEpgData.epg_listings.FirstOrDefault(x => x.now_playing == 1);
                    if (nowPlaying != null)
                    {
                        // Decode title and description from base64
                        channel.title = DecodeFrom64(nowPlaying.title);
                        channel.desc = DecodeFrom64(nowPlaying.description);

                        // Convert timestamps to DateTime objects
                        DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(nowPlaying.start_timestamp)).DateTime;
                        DateTime stopTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(nowPlaying.stop_timestamp)).DateTime;

                        // Convert UTC to local time
                        startTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, TimeZoneInfo.Local);
                        stopTime = TimeZoneInfo.ConvertTimeFromUtc(stopTime, TimeZoneInfo.Local);

                        // Format the timestamps
                        channel.start_timestamp = startTime.ToString("h:mm tt MM-dd-yyyy");
                        channel.stop_timestamp = stopTime.ToString("h:mm tt MM-dd-yyyy");
                        channel.start_end_timestamp = startTime.ToString("h:mm tt") + " - " + stopTime.ToString("h:mm tt");
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    using (Stream errorResponse = response.GetResponseStream())
                    {
                        if (errorResponse != null)
                        {
                            StreamReader errorReader = new StreamReader(errorResponse);
                            string errorResponseText = await errorReader.ReadToEndAsync();
                            // Display the content without HTML tags.
                            string textOnly = Regex.Replace(errorResponseText, "<.*?>", "");
                            Xceed.Wpf.Toolkit.MessageBox.Show("Response from server: " + textOnly);
                        }
                    }
                }
            }
            catch (OperationCanceledException) {}
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error: " + ex.Message);
            }
        }


        //Retrieves each individual channel data
        //keep pass action as parameter
        public static async Task RetrieveChannelData(BusyIndicator busy_ind, CancellationToken token)//maybe pass in the action as a string and use this for all action calls
        {
            //action=get_live_streams  use this to get all the channels and their data
            //action=get_live_categories    use this to get the categories (already is used, should be fine)
            //action=get_simple_data_table&stream_id=X  use this to get the channel data for a specific channel (EPG)

            try
            {
                token.ThrowIfCancellationRequested();

                // Create a request for the URL. 	
                WebRequest request = WebRequest.Create($"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}&action=get_live_streams");
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = await reader.ReadToEndAsync();
                // Display the content.
                //Debug.WriteLine(responseFromServer);

                //Channels are loaded here
                ChannelEntry[] channelInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelEntry[]>(responseFromServer);

                int counter = 0;
                int total = channelInfo.Length;
                foreach (ChannelEntry channel in channelInfo)
                {
                    busy_ind.BusyContent = $"Processing channel data... ({counter + 1}/{total})";
                    if (Instance.categoryToChannelMap.ContainsKey(channel.category_id))
                        Instance.categoryToChannelMap[channel.category_id].Add(channel);
                    else
                        Instance.categoryToChannelMap.Add(channel.category_id, new List<ChannelEntry>() { channel });
                    counter++;
                }

                Instance.ChannelsArray = channelInfo;

                //busy_ind.IsBusy = false;

                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();

                await LoadPlaylistDataAsync();//hide this call from where RetrieveChannelData is called
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    using (Stream errorResponse = response.GetResponseStream())
                    {
                        if (errorResponse != null)
                        {
                            StreamReader errorReader = new StreamReader(errorResponse);
                            string errorResponseText = await errorReader.ReadToEndAsync();
                            // Display the content without HTML tags.
                            string textOnly = Regex.Replace(errorResponseText, "<.*?>", "");
                            Xceed.Wpf.Toolkit.MessageBox.Show("Response from server: " + textOnly);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Login Canceled.");
            }
        }

        //keep but join in other functions, pass the action as a parameter
        public static async Task RetrieveCategories(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // Create a request for the URL.
                WebRequest request = WebRequest.Create($"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}&action=get_live_categories");
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = await reader.ReadToEndAsync();
                // Display the content.
                //Debug.WriteLine(responseFromServer);

                ChannelGroups[] info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelGroups[]>(responseFromServer);

                //add each category_id to the categoryToChannelMap as a key and the values null
                foreach (ChannelGroups entry in info)
                {
                    if (!Instance.categoryToChannelMap.ContainsKey(entry.category_id))
                        Instance.categoryToChannelMap.Add(entry.category_id, new List<ChannelEntry>());
                    else
                        Debug.WriteLine("Key already exists in categoryToChannelMap dictionary");
                }


                Instance.ChannelGroupsArray = info;

                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    using (Stream errorResponse = response.GetResponseStream())
                    {
                        if (errorResponse != null)
                        {
                            StreamReader errorReader = new StreamReader(errorResponse);
                            string errorResponseText = await errorReader.ReadToEndAsync();
                            // Display the content without HTML tags.
                            string textOnly = Regex.Replace(errorResponseText, "<.*?>", "");
                            Xceed.Wpf.Toolkit.MessageBox.Show("Response from server: " + textOnly);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Login Canceled.");
            }
        }

        private static readonly HttpClient _clientTest = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
            return client;
        }

        //Gets all channels basic info data (not epg data)
        public static async Task LoadPlaylistDataAsync()
        {
            Instance.playlistDataMap = new Dictionary<string, ChannelStreamData>();
            try
            {
                var stringTask = _clientTest.GetStringAsync($"{(_useHttps ? "https" : "http")}://{_server}:{_port}/get.php?username={_user}&password={_pass}");
                var serverResponse = await stringTask;

                var playlist = serverResponse.Split("#EXTINF:");
                var info = new ChannelStreamData[playlist.Length];

                for (int index = 1; index < playlist.Length; index++) // Start index at 1 to skip index 0
                {
                    var playlistItem = playlist[index];
                    //Debug.WriteLine("Playlist Item: " + playlistItem);

                    var attributeValuePairs = playlistItem.Split(' ')
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Select(item => item.Split('='))
                    .Where(parts => parts.Length > 1) // Exclude items without both attribute and value
                    .Select(parts => new { Attribute = parts[0], Value = parts[1].Trim('"') })
                    .ToDictionary(pair => pair.Attribute, pair => pair.Value);

                    // Extract the last HTTPS or HTTP URL from the playlist item
                    MatchCollection urls;
                    if (_useHttps)
                        urls = Regex.Matches(playlistItem, @"https://[^""]+");
                    else
                        urls = Regex.Matches(playlistItem, @"http://[^""]+");

                    string lastHttpsUrl = urls.Cast<Match>().LastOrDefault()?.Value;

                    if (!string.IsNullOrEmpty(lastHttpsUrl))
                    {
                        string trimmedLastHttpsUrl = lastHttpsUrl.Trim();
                        var idMatch = Regex.Match(trimmedLastHttpsUrl, @"/(?<id>\d+)(?:\.ts)?$");
                        string channelId = idMatch.Groups["id"].Value;

                        var channelData = new ChannelStreamData
                        {
                            tvg_id = attributeValuePairs.GetValueOrDefault("tvg-id"),
                            tvg_name = attributeValuePairs.GetValueOrDefault("tvg-name"),
                            tvg_logo = attributeValuePairs.GetValueOrDefault("tvg-logo"),
                            group_title = attributeValuePairs.GetValueOrDefault("group-title"),
                            stream_url = lastHttpsUrl,
                            channel_id = channelId
                        };

                        AddToPlaylistDataMap(channelData, channelId);
                    }
                    else
                    {
                        Debug.WriteLine("Errored on Playlist Item: " + playlistItem);
                        throw new Exception("Failed to extract stream URL from playlist.");
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    using (Stream errorResponse = response.GetResponseStream())
                    {
                        if (errorResponse != null)
                        {
                            StreamReader errorReader = new StreamReader(errorResponse);
                            string errorResponseText = await errorReader.ReadToEndAsync();
                            // Display the content without HTML tags.
                            string textOnly = Regex.Replace(errorResponseText, "<.*?>", "");
                            Xceed.Wpf.Toolkit.MessageBox.Show("Response from server: " + textOnly);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error: " + ex.Message);
            }
        }

        private static void AddToPlaylistDataMap(ChannelStreamData data, string channelId)
        {
            if (!Instance.playlistDataMap.ContainsKey(channelId))
            {
                Instance.playlistDataMap.Add(channelId, data);
            }
            else
            {
                //Debug.WriteLine("Duplicate key: " + channelId);
            }
        }

        public static string DecodeFrom64(string encodedData)
        {
            try
            {
                byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
                string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);
                return returnValue;
            }
            catch (FormatException e)
            {
                Console.WriteLine("An error occurred while decoding the base64 encoded string: " + e.Message);
                return null;
            }
        }
    }

    public class M3UPlaylist
    {
        public M3UPlaylist() { }

        public interface IChannel
        {
            string DisplayName { get; }
            string IconUrl { get; }
            string Title { get; }
            string Description { get; }
        }

        public class M3UChannel : IChannel
        {
            public string ChannelNumber { get; set; }
            public string ChannelId { get; set; }
            public string ChannelName { get; set; }
            public string LogoUrl { get; set; }
            public string GroupTitle { get; set; }
            public string StreamUrl { get; set; }

            public M3UEPGData EPGData { get; set; }

            public string DisplayName => this.ChannelName;
            public string IconUrl => this.LogoUrl;
            public string Title => this.EPGData?.ProgramTitle;
            public string Description => this.EPGData?.Description;

            public string FormattedTimeRange
            {
                get
                {
                    if (EPGData != null)
                    {
                        return $"{EPGData.StartTime:hh:mm tt} - {EPGData.EndTime:hh:mm tt}";
                    }
                    return string.Empty;
                }
            }
        }

        public class M3UEPGData
        {
            public string ChannelId { get; set; }
            public string ProgramTitle { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Description { get; set; }
        }

        public static async Task RetrieveM3UPlaylistData(string m3uPlaylistUrl, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // Create a request for the URL.
                WebRequest request = WebRequest.Create(m3uPlaylistUrl);
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = await reader.ReadToEndAsync();

                // Now you have the M3U playlist data in responseFromServer, and you can parse it.
                var channels = ParseM3UPlaylist(responseFromServer);
                Instance.M3UChannels = channels;

                // Link M3U channels to EPG data
                LinkChannelsToEPG(Instance.M3UChannels, Instance.M3UEPGDataList);

                // Here you can do something with the parsed channels
                // For example, you could add them to a list in your application.

                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    using (Stream errorResponse = response.GetResponseStream())
                    {
                        if (errorResponse != null)
                        {
                            StreamReader errorReader = new StreamReader(errorResponse);
                            string errorResponseText = await errorReader.ReadToEndAsync();
                            // Display the content without HTML tags.
                            string textOnly = Regex.Replace(errorResponseText, "<.*?>", "");
                            Xceed.Wpf.Toolkit.MessageBox.Show("Response from server: " + textOnly);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Operation Canceled.");
            }
        }

        private static void LinkChannelsToEPG(List<M3UChannel> channels, List<M3UEPGData> epgDataList)
        {
            foreach (var channel in channels)
            {
                // Find the EPG data for the current channel
                var epgData = epgDataList.FirstOrDefault(e => e.ChannelId == channel.ChannelId);
                if (epgData != null)
                {
                    channel.EPGData = epgData;
                }
            }
        }

        private static List<M3UChannel> ParseM3UPlaylist(string playlistData)
        {
            var channels = new List<M3UChannel>();
            var lines = playlistData.Split('\n');
            M3UChannel currentChannel = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("#EXTINF:"))
                {
                    currentChannel = ParseChannelInfo(line);
                }
                else if (line.StartsWith("http") && currentChannel != null)
                {
                    currentChannel.StreamUrl = line.Trim();
                    channels.Add(currentChannel);
                    currentChannel = null;
                }
            }

            return channels;
        }

        private static M3UChannel ParseChannelInfo(string infoLine)
        {
            var channel = new M3UChannel();
            var attributesPart = infoLine.Substring(infoLine.IndexOf(':') + 1, infoLine.IndexOf(',') - infoLine.IndexOf(':') - 1);
            var channelNamePart = infoLine.Substring(infoLine.IndexOf(',') + 1).Trim();

            // Regular expression to match key-value pairs in the attributes part of the EXTINF line
            var regex = new Regex(@"([\w-]+)=(""([^""]*)""|([^\s,]+))");
            var matches = regex.Matches(attributesPart);

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[3].Success ? match.Groups[3].Value : match.Groups[4].Value;

                switch (key)
                {
                    case "tvg-chno":
                        channel.ChannelNumber = value;
                        break;
                    case "tvg-id":
                        channel.ChannelId = value;
                        break;
                    case "tvg-name":
                        channel.ChannelName = value;
                        break;
                    case "tvg-logo":
                        channel.LogoUrl = value;
                        break;
                    case "group-title":
                        channel.GroupTitle = value;
                        break;
                }
            }

            // Set the channel name
            channel.ChannelName = channelNamePart;

            return channel;
        }


        public static async Task<string> DownloadAndParseEPG(string epgUrl, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // 1. Download the .gz file
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(epgUrl, token);
                    response.EnsureSuccessStatusCode();

                    // 2. Save the .gz file to a temporary location
                    var tempFilePath = Path.GetTempFileName();
                    using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await response.Content.CopyToAsync(fs, token);
                    }

                    // 3. Extract the contents of the .gz file
                    var extractedFilePath = Path.ChangeExtension(tempFilePath, ".xml");
                    using (var originalFileStream = new FileStream(tempFilePath, FileMode.Open))
                    using (var decompressedFileStream = File.Create(extractedFilePath))
                    using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        await decompressionStream.CopyToAsync(decompressedFileStream, 4096, token);
                    }

                    // 4. Parse the extracted XML file
                    var epgData = await File.ReadAllTextAsync(extractedFilePath, token);

                    // 5. Clean up temporary files
                    File.Delete(tempFilePath);
                    File.Delete(extractedFilePath);

                    return epgData;
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error: " + ex.Message);
                return null;
            }
        }

        public static async Task MatchChannelsWithEPG(string epgData, List<M3UChannel> channels)
        {
            try
            {
                var xdoc = XDocument.Parse(epgData);
                var epgChannels = xdoc.Descendants("channel").Select(c => new
                {
                    Id = (string)c.Attribute("id"),
                    DisplayName = (string)c.Element("display-name"),
                    IconSrc = (string)c.Element("icon")?.Attribute("src")
                }).ToList();

                var programmesByChannel = xdoc.Descendants("programme")
                    .GroupBy(p => (string)p.Attribute("channel"))
                    .ToDictionary(g => g.Key, g => g.ToList());

                DateTime now = DateTime.Now; // Get the current local time

                var epgDataList = new List<M3UEPGData>(); // Initialize the EPG data list

                foreach (var channel in channels)
                {
                    var matchedChannel = epgChannels.FirstOrDefault(c => c.Id == channel.ChannelId);
                    if (matchedChannel != null)
                    {
                        // Match found, update channel information
                        channel.ChannelName = matchedChannel.DisplayName;
                        channel.LogoUrl = matchedChannel.IconSrc;
                    }

                    if (programmesByChannel.TryGetValue(channel.ChannelId, out var programmes))
                    {
                        // Find the current program based on the start and end times
                        var currentProgram = programmes
                            .Select(p => new M3UEPGData
                            {
                                ChannelId = channel.ChannelId,
                                ProgramTitle = (string)p.Element("title"),
                                StartTime = DateTime.ParseExact((string)p.Attribute("start"), "yyyyMMddHHmmss zzzz", CultureInfo.InvariantCulture),
                                EndTime = DateTime.ParseExact((string)p.Attribute("stop"), "yyyyMMddHHmmss zzzz", CultureInfo.InvariantCulture),
                                Description = (string)p.Element("desc")
                            })
                            .FirstOrDefault(epgData2 => epgData2.StartTime <= now && epgData2.EndTime > now);

                        if (currentProgram != null)
                        {
                            // If there is a current program, add it to the EPG data list
                            epgDataList.Add(currentProgram);
                            // Also set it to the channel's EPGData
                            channel.EPGData = currentProgram;
                        }
                    }
                }

                // Store the list of current EPG data in the Instance class
                Instance.M3UEPGDataList = epgDataList;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error matching channels: " + ex.Message);
            }
        }

        public static async Task PairEPGTOChannelM3U(List<M3UChannel> channels)
        {
            try
            {
                // Clear the existing map before starting the pairing
                Instance.M3UChannelToEPGMap.Clear();

                // Loop through all channels and pair with EPG data
                foreach (var channel in channels)
                {
                    // Find the EPG data that matches the current channel's ID
                    var epgData = Instance.M3UEPGDataList.FirstOrDefault(e => e.ChannelId == channel.ChannelId);

                    // If matching EPG data is found, associate it with the channel
                    if (epgData != null)
                    {
                        channel.EPGData = epgData;
                    }

                    // Add the channel with its EPG data to the map
                    Instance.M3UChannelToEPGMap.Add(channel);
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error in PairEPGTOChannelM3U: {ex.Message}");
            }
        }
    }
}
