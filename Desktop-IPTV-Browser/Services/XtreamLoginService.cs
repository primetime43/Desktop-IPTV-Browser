using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Desktop_IPTV_Browser.Models;
using System.Globalization;
using System.Diagnostics;
using System.Xml.Linq;
using System.Configuration;

namespace Desktop_IPTV_Browser.Services
{
    public class XtreamLoginService
    {
        private readonly HttpClient _httpClient;
        private readonly string _saveDir;
        private readonly string _epgSaveDir;

        private string _server;
        private string _username;
        private string _password;
        private string _port;
        private bool _useHttps;

        public XtreamLoginService()
        {
            _httpClient = new HttpClient();
            _saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XtreamUsers");
            _epgSaveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EPGData");

            if (!Directory.Exists(_epgSaveDir))
                Directory.CreateDirectory(_epgSaveDir);
        }

        /// <summary>
        /// Validates login credentials by sending a request to the Xtream server.
        /// </summary>
        public async Task<bool> CheckLoginConnection(string server, string username, string password, string port, bool useHttps, CancellationToken cancellationToken)
        {
            try
            {
                string protocol = useHttps ? "https" : "http";
                string url = $"{protocol}://{server}:{port}/player_api.php?username={username}&password={password}";

                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
                    JObject json = JObject.Parse(responseData);

                    if (json["user_info"]?["auth"]?.ToString() == "1")
                    {
                        _server = server;
                        _username = username;
                        _password = password;
                        _port = port;
                        _useHttps = useHttps;
                        return true; // Login successful
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking login connection: {ex.Message}");
            }

            return false; // Login failed
        }

        /// <summary>
        /// Saves user login details to a file in the XtreamUsers directory.
        /// </summary>
        public void SaveUserData(string username, string password, string server, string port, bool useHttps)
        {
            if (!Directory.Exists(_saveDir))
                Directory.CreateDirectory(_saveDir);

            string filePath = Path.Combine(_saveDir, $"{username}.json");
            var userData = new
            {
                Username = username,
                Password = password,
                Server = server,
                Port = port,
                UseHttps = useHttps
            };

            File.WriteAllText(filePath, JObject.FromObject(userData).ToString());
        }

        /// <summary>
        /// Loads saved user login details from a file.
        /// </summary>
        public JObject LoadUserData(string username)
        {
            string filePath = Path.Combine(_saveDir, $"{username}.json");
            if (File.Exists(filePath))
            {
                return JObject.Parse(File.ReadAllText(filePath));
            }

            return null;
        }

        /// <summary>
        /// Retrieves a list of saved logins by reading files from the XtreamUsers directory.
        /// </summary>
        public IEnumerable<string> GetSavedLogins()
        {
            if (!Directory.Exists(_saveDir))
                Directory.CreateDirectory(_saveDir);

            var files = Directory.GetFiles(_saveDir, "*.json");
            foreach (var file in files)
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        /// <summary>
        /// Retrieves Xtream playlist data by sending a "get_live_streams" request to the Xtream server.
        /// </summary>
        public async Task<List<XtreamChannel>> RetrieveXtreamPlaylistData(CancellationToken cancellationToken)
        {
            try
            {
                string responseContent = await SendXtreamRequestAsync("get_live_streams", cancellationToken);

                // Deserialize the response into a list of XtreamChannel
                var channels = JsonConvert.DeserializeObject<List<XtreamChannel>>(responseContent);

                return channels ?? new List<XtreamChannel>();
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving playlist data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Downloads EPG data as XML and saves it to the EPGData directory.
        /// </summary>
        public async Task<string> DownloadEPGData(CancellationToken cancellationToken)
        {
            try
            {
                string protocol = _useHttps ? "https" : "http";
                string epgUrl = $"{protocol}://{_server}:{_port}/xmltv.php?username={_username}&password={_password}";

                var response = await _httpClient.GetAsync(epgUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                string epgData = await response.Content.ReadAsStringAsync(cancellationToken);

                // Generate a unique filename using timestamp
                string fileName = $"{_username}_EPG_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml";
                string filePath = Path.Combine(_epgSaveDir, fileName);

                // Save the EPG data to a file
                await File.WriteAllTextAsync(filePath, epgData, cancellationToken);

                Console.WriteLine($"EPG data saved to {filePath}");
                return filePath; // Return the path of the saved file
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading EPG data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Links downloaded EPG data to a list of Xtream channels by matching Channel IDs.
        /// </summary>
        /// <param name="channels">The list of channels to link EPG data to.</param>
        /// <param name="epgFilePath">The file path of the EPG data XML file.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of updated channels with linked EPG data.</returns>
        public async Task<List<XtreamChannel>> LinkChannelsToEPGData(List<XtreamChannel> channels, string epgFilePath, CancellationToken cancellationToken)
        {
            if (channels == null || !channels.Any())
                throw new ArgumentException("Channels list is empty or null.");

            if (string.IsNullOrWhiteSpace(epgFilePath) || !File.Exists(epgFilePath))
                throw new FileNotFoundException("EPG data file not found.", epgFilePath);

            try
            {
                // Read the EPG data from the file
                string epgData = await File.ReadAllTextAsync(epgFilePath, cancellationToken);
                GlobalData.XtreamEPGData = epgData; // Store globally if needed

                // Parse the EPG XML document
                var xdoc = XDocument.Parse(epgData);

                // Extract EPG channels and programmes
                var epgChannels = xdoc.Descendants("channel").Select(c => new
                {
                    Id = (string)c.Attribute("id"),
                    DisplayName = (string)c.Element("display-name"),
                    IconSrc = (string)c.Element("icon")?.Attribute("src")
                }).Where(c => !string.IsNullOrEmpty(c.Id)).ToList();

                var programmesByChannel = xdoc.Descendants("programme")
                    .GroupBy(p => (string)p.Attribute("channel"))
                    .Where(g => !string.IsNullOrEmpty(g.Key)) // Filter out invalid channel groups
                    .ToDictionary(g => g.Key, g => g.ToList());

                DateTime now = DateTime.Now;

                // Prepare to store EPG data
                var epgDataList = new List<IEPGData>();

                // Process each channel
                foreach (var channel in channels)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrEmpty(channel.ChannelId))
                        continue;

                    // Match EPG channel and update metadata
                    var matchedChannel = epgChannels.FirstOrDefault(c => c.Id == channel.ChannelId);
                    if (matchedChannel != null)
                    {
                        channel.ChannelName = matchedChannel.DisplayName ?? channel.ChannelName;
                        channel.LogoUrl = matchedChannel.IconSrc ?? channel.LogoUrl;
                    }

                    // Match current programme
                    if (programmesByChannel.TryGetValue(channel.ChannelId, out var programmes))
                    {
                        var currentProgram = programmes
                            .Select(p => new XtreamEPGData
                            {
                                ChannelId = channel.ChannelId,
                                ProgramTitle = (string)p.Element("title") ?? "No Title",
                                StartTime = DateTime.ParseExact((string)p.Attribute("start"), "yyyyMMddHHmmss zzzz", CultureInfo.InvariantCulture),
                                EndTime = DateTime.ParseExact((string)p.Attribute("stop"), "yyyyMMddHHmmss zzzz", CultureInfo.InvariantCulture),
                                Description = (string)p.Element("desc")
                            })
                            .FirstOrDefault(epg => epg.StartTime <= now && epg.EndTime > now);

                        if (currentProgram != null)
                        {
                            channel.EPGData = currentProgram;
                            epgDataList.Add(currentProgram);
                        }
                    }
                }

                GlobalData.XtreamEPGDataList = epgDataList;

                return channels;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error linking channels to EPG data: {ex.Message}");
                throw new Exception("An error occurred while linking channels to EPG data.", ex);
            }
        }

        /// <summary>
        /// Sends an Xtream API request with a specific action and returns the response as a string.
        /// </summary>
        private async Task<string> SendXtreamRequestAsync(string action, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_port))
                {
                    throw new InvalidOperationException("Login credentials are not set. Please log in first.");
                }

                string protocol = _useHttps ? "https" : "http";
                string url = $"{protocol}://{_server}:{_port}/player_api.php?username={_username}&password={_password}&action={action}";

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode(); // Throws if status code is not successful

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Xtream request for action '{action}': {ex.Message}");
                throw;
            }
        }
    }
}
