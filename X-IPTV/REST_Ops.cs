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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using System.Xml;
using Xceed.Wpf.Toolkit;

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
    public class REST_Ops
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
        public static async Task<bool> CheckLoginConnection()
        {
            try
            {
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
                //Debug.WriteLine(responseFromServer);

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
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Handle the 404 Not Found response
                        System.Windows.MessageBox.Show("404 Not Found");
                    }
                }
                return false; // Connection was not successful
            }
        }

        //testing for rewrite
        public static async Task GetEPGDataForIndividualChannel(ChannelEntry channel)
        {
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create($"{(_useHttps ? "https" : "http")}://{_server}:{_port}/player_api.php?username={_user}&password={_pass}&action=get_simple_data_table&stream_id={channel.stream_id}");
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

            Channel24hrEPG channel24hrEpgData = JsonConvert.DeserializeObject<Channel24hrEPG>(responseFromServer);

            if (channel24hrEpgData.epg_listings.Count == 0)
            {
                //Debug.WriteLine("epg_listings is an empty list");
                channel.title = "No information";
                channel.desc = "No information";
                return;
            }

            var nowPlaying = channel24hrEpgData.epg_listings.Where(x => x.now_playing == 1).FirstOrDefault();
            if (nowPlaying != null)
            {
                //System.Windows.MessageBox.Show("Now playing: " + DecodeFrom64(nowPlaying.title) + "\nDescription: " + DecodeFrom64(nowPlaying.description));
                channel.title = DecodeFrom64(nowPlaying.title);
                channel.desc = DecodeFrom64(nowPlaying.description);

                DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(double.Parse(nowPlaying.start_timestamp));
                DateTime stopTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(double.Parse(nowPlaying.stop_timestamp));

                startTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, TimeZoneInfo.Local);
                stopTime = TimeZoneInfo.ConvertTimeFromUtc(stopTime, TimeZoneInfo.Local);

                channel.start_timestamp = startTime.ToString("h:mm tt MM-dd-yyyy");
                channel.stop_timestamp = stopTime.ToString("h:mm tt MM-dd-yyyy");

                channel.start_end_timestamp = startTime.ToString("h:mm tt") + " - " + stopTime.ToString("h:mm tt");


                /*channel.start_timestamp = nowPlaying.start;
                channel.stop_timestamp = nowPlaying.end;*/
                //Debug.WriteLine("Added epg for " + channel.title + " - " + nowPlaying.channel_id);
            }

            //Instance.allChannelEPG_24HRS_Dict.Add(stream_id, myDeserializedClass);

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        //Retrieves each individual channel data
        //keep pass action as parameter
        public static async Task RetrieveChannelData(BusyIndicator busy_ind)//maybe pass in the action as a string and use this for all action calls
        {
            //action=get_live_streams  use this to get all the channels and their data
            //action=get_live_categories    use this to get the categories (already is used, should be fine)
            //action=get_simple_data_table&stream_id=X  use this to get the channel data for a specific channel (EPG)


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

            //add each ChannelEntry to categoryToChannelMap based on the category_id
            /*foreach (ChannelEntry channel in channelInfo)
            {
                if (Instance.categoryToChannelMap.ContainsKey(channel.category_id))
                    Instance.categoryToChannelMap[channel.category_id].Add(channel);
                else
                    Instance.categoryToChannelMap.Add(channel.category_id, new List<ChannelEntry>() { channel });
            }*/

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

            //await LoadPlaylistChannelData(user, pass, server, port);//hide this call from where RetrieveChannelData is called

            await LoadPlaylistDataAsync();//hide this call from where RetrieveChannelData is called
        }

        //keep but join in other functions, pass the action as a parameter
        public static async Task RetrieveCategories()
        {

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
                    var wordArray = playlist[index].Split('"').Select((element, i) => i % 2 == 0 ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { element }).SelectMany(element => element).ToList();

                    var startIndex = playlist[index].LastIndexOf("https");
                    if (startIndex >= 0)
                    {
                        info[index] = new ChannelStreamData
                        {
                            xui_id = wordArray[2],
                            tvg_id = wordArray[4],
                            tvg_name = wordArray[6],
                            tvg_logo = wordArray[8],
                            group_title = wordArray[10],
                            stream_url = playlist[index].Substring(startIndex)
                        };

                        AddToPlaylistDataMap(info[index]);
                    }
                    else
                    {
                        throw new Exception("Failed to extract stream URL from playlist."); // Throw a custom exception with an error message
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void AddToPlaylistDataMap(ChannelStreamData data)
        {
            if (!Instance.playlistDataMap.ContainsKey(data.xui_id))
            {
                Instance.playlistDataMap.Add(data.xui_id, data);
            }
            else
            {
                System.Windows.MessageBox.Show("Duplicate key: " + data.xui_id + "\rClosing application");
                Application.Current.Shutdown();
            }
        }

        public static string DecodeFrom64(string encodedData)
        {
            try
            {
                byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
                string returnValue = System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
                return returnValue;
            }
            catch (FormatException e)
            {
                Console.WriteLine("An error occurred while decoding the base64 encoded string: " + e.Message);
                return null;
            }
        }
    }
}
