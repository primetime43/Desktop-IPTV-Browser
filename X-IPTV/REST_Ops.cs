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
using System.ServiceModel.Channels;
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
    public class XtreamCodes
    {
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private static string _user;
        private static string _pass;
        private static string _server;
        private static string _port;
        private static bool _useHttps;
        public XtreamCodes() { }

        public interface IChannelXtream
        {
            string DisplayName { get; }
            string IconUrl { get; }
            string Title { get; }
            string Description { get; }
        }

        public class XtreamCategory
        {
            [JsonProperty("category_id")]
            public string CategoryId { get; set; }

            [JsonProperty("category_name")]
            public string CategoryName { get; set; }

            [JsonProperty("parent_id")]
            public int ParentId { get; set; }

            public string CategoryNameId => $"{CategoryName} - {CategoryId}";
        }


        public class XtreamChannel : IChannelXtream
        {
            [JsonProperty("num")]
            public string ChannelNumber { get; set; }

            [JsonProperty("epg_channel_id")]
            public string ChannelId { get; set; }

            [JsonProperty("name")]
            public string ChannelName { get; set; }

            [JsonProperty("stream_icon")]
            public string LogoUrl { get; set; }

            [JsonProperty("stream_id")]
            public int StreamId { get; set; }

            [JsonProperty("category_id")]
            public string CategoryId { get; set; } // Single category ID

            [JsonProperty("category_ids")]
            public List<int> CategoryIds { get; set; } // List of category IDs (some channels belong to more than one category)

            [JsonProperty("stream_type")]
            public string StreamType { get; set; }

            public string CategoryName { get; set; }

            public string StreamUrl
            {
                get
                {
                    return $"{(_useHttps ? "https" : "http")}://{_server}:{_port}/{_user}/{_pass}/{StreamId}.ts";
                }
            }

            public XtreamEPGData EPGData { get; set; }

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

        public class XtreamEPGData
        {
            public string ChannelId { get; set; }
            public string ProgramTitle { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Description { get; set; }
        }

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
                //Debug.WriteLine("Request URL: " + url);

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

        //done
        public static async Task<string> DownloadEPGAndSaveToFile(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // Format the current date and time for the filename
                string dateTimeFormat = DateTime.Now.ToString("yyyyMMddHHmmss");

                // Combine the user name and the date time to create a unique file name
                string fileName = $"Xtream-{_user}_{dateTimeFormat}.xml";

                // 1. Download the XML file
                using (var client = new HttpClient())
                {
                    string epgUrl = $"{(_useHttps ? "https" : "http")}://{_server}:{_port}/xmltv.php?username={_user}&password={_pass}";

                    var response = await client.GetAsync(epgUrl, token);
                    response.EnsureSuccessStatusCode();

                    string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EPGData"); // Folder where you want to save the file
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath); // Create the directory if it does not exist

                    var savedXMLPath = Path.Combine(directoryPath, fileName);

                    await using (var fs = new FileStream(savedXMLPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs, token);
                    }

                    // 3. Parse the XML file
                    var epgData = await File.ReadAllTextAsync(savedXMLPath, token);

                    return epgData;
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error: " + ex.Message);
                return null;
            }
        }

        //done, rewritten
        public static async Task RetrieveXtreamPlaylistData(BusyIndicator busy_ind, CancellationToken token)//maybe pass in the action as a string and use this for all action calls
        {
            //action=get_live_streams  use this to get all the channels and their data
            //action=get_live_categories    use this to get the categories (already is used, should be fine)
            //action=get_simple_data_table&stream_id=X  use this to get the channel data for a specific channel (EPG)

            try
            {
                token.ThrowIfCancellationRequested();

                string url = $"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}&action=get_live_streams";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url, token);
                    response.EnsureSuccessStatusCode(); // Throw if not a success code.

                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response into an array of XtreamChannel objects.
                    var channels = JsonConvert.DeserializeObject<XtreamChannel[]>(responseContent);

                    Instance.XtreamChannels.AddRange(channels);
                }
            }
            catch (HttpRequestException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("HTTP Error: " + ex.Message);
            }
            catch (OperationCanceledException)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Login Canceled.");
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        //done, rewritten
        public static async Task RetrieveCategories(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                string url = $"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}&action=get_live_categories";

                HttpResponseMessage response = await _httpClient.GetAsync(url, token);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.

                string responseFromServer = await response.Content.ReadAsStringAsync();

                var categories = JsonConvert.DeserializeObject<XtreamCategory[]>(responseFromServer);

                foreach (var category in categories)
                {
                    Instance.XtreamCategoryList.Add(category);
                }
            }
            catch (HttpRequestException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("HTTP Error: " + ex.Message);
            }
            catch (OperationCanceledException)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Operation canceled.");
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        //matches the channels with the epg data (manual updating epg data)
        public static async Task<bool> UpdateChannelsEpgData(List<XtreamChannel> channels)
        {
            if(Instance.XtreamEPGDataList.Count > 0)//clear it out so if the epg is being updated
                Instance.XtreamEPGDataList.Clear();

            try
            {
                var xdoc = XDocument.Parse(Instance.allXtreamEpgData);
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

                var epgDataList = new List<XtreamEPGData>(); // Initialize the EPG data list

                foreach (var channel in channels)
                {
                    if (string.IsNullOrEmpty(channel.ChannelId))
                    {
                        continue;
                    }

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
                            .Select(p => new XtreamEPGData
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
                // Does not store any past or future epg data
                Instance.XtreamEPGDataList = epgDataList;

                // Update the lastEpgDataLoadTime setting with the current date and time
                ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.Now.ToString("o"));

                return true;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error matching channels: " + ex.Message);
                return false;
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
            return client;
        }
    }

    public class M3UPlaylist
    {
        public M3UPlaylist() { }

        public interface IChannelM3U
        {
            string DisplayName { get; }
            string IconUrl { get; }
            string Title { get; }
            string Description { get; }
        }

        public class M3UChannel : IChannelM3U
        {
            public string ChannelNumber { get; set; }
            public string ChannelId { get; set; }
            public string ChannelName { get; set; }
            public string LogoUrl { get; set; }
            public string CategoryName { get; set; }
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

        public class M3UCategory
        {
            [JsonProperty("group-title")]
            public string CategoryName { get; set; }
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
                        channel.CategoryName = value;
                        // Only add if it doesn't already exist
                        if (!Instance.M3UCategoryList.Any(c => c.CategoryName == value))
                        {
                            Instance.M3UCategoryList.Add(new M3UCategory { CategoryName = value });
                        }
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

        public static async Task<bool> UpdateChannelsEpgData(List<M3UChannel> channels)
        {
            if (Instance.M3UEPGDataList.Count > 0)//clear it out so if the epg is being updated
                Instance.M3UEPGDataList.Clear();

            try
            {
                var xdoc = XDocument.Parse(Instance.allM3uEpgData);
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
                // Does not store any past or future epg data
                Instance.M3UEPGDataList = epgDataList;

                // Update the lastEpgDataLoadTime setting with the current date and time
                ConfigurationManager.UpdateSetting("lastEpgDataLoadTime", DateTime.Now.ToString("o"));

                return true;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error matching channels: " + ex.Message);
                return false;
            }
        }

        // temp testing. Might not need
        /*public static async Task MatchChannelsWithEPG(string epgData, List<M3UChannel> channels)
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
                // Does not store any past or future epg data
                Instance.M3UEPGDataList = epgDataList;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error matching channels: " + ex.Message);
            }
        }*/

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
