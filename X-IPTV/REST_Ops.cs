using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace X_IPTV
{
    public class REST_Ops
    {
        private static readonly HttpClient _client = new HttpClient();
        private static UserLogin ul = new UserLogin();
        //use get_live_categories for categories
        public static async Task LoginConnect(string user, string pass, string server, string port)
        {
            // Create a request for the URL. 		
            WebRequest request;
            if ((bool)ul.protocolCheckBox.IsChecked)//use the https protocol
                request = WebRequest.Create($"https://{server}:{port}/player_api.php?username={user}&password={pass}");
            else//use the http protocol
                request = WebRequest.Create($"http://{server}:{port}/player_api.php?username={user}&password={pass}");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            // Display the status.
            Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = await reader.ReadToEndAsync();
            // Display the content.
            Debug.WriteLine(responseFromServer);

            PlayerInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInfo>(responseFromServer);

            Instance.PlayerInfo = info;

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        public static async Task RetrieveChannels(string user, string pass, string server, string port)//maybe pass in the action as a string and use this for all action calls
        {
            // Create a request for the URL. 	
            WebRequest request;
            if ((bool)ul.protocolCheckBox.IsChecked)//use the https protocol
                request = WebRequest.Create($"https://{server}:{port}/player_api.php?username={user}&password={pass}&action=get_live_streams");
            else//use the http protocol
                request = WebRequest.Create($"http://{server}:{port}/player_api.php?username={user}&password={pass}&action=get_live_streams");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            // Display the status.
            Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = await reader.ReadToEndAsync();
            // Display the content.
            Debug.WriteLine(responseFromServer);

            //Channels are loaded here
            ChannelEntry[] info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelEntry[]>(responseFromServer);

            Instance.ChannelsArray = info;

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
        //need to review these two. They seem to get the same data, but in a different format. get_live_streams seems to be better than get.php
        public static async Task LoadPlaylistData(string user, string pass, string server, string port)//maybe remove this one
        {
            Instance.playlistDataMap = new Dictionary<string, PlaylistData>(); //playlistDataMap is a dictionary containing the xui_id as the key and value being the PlaylistData object
            //Outer dict key is playlist categories, key is another dictionary containing the channel data
            //Innder dict key is each channel id,value is the Playlist data array for each channel
            Instance.categories = new Dictionary<string, Dictionary<string, PlaylistData>>();

            //Outer dict key is playlist categories, key is another dictionary containing the channel data
            //Innder dict key is each channel id,value is the Playlist data array for each channel
            //Dictionary<string, Dictionary<string, PlaylistData>> categories = new Dictionary<string, Dictionary<string, PlaylistData>>();
            //retrieve playlist data from client
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

            var stringTask = _client.GetStringAsync($"https://{server}:{port}/get.php?username={user}&password={pass}");

            var msg = await stringTask;
            //Console.Write(msg);

            //parse the m3u playlist and split into an array
            string[] playlist = msg.Split(new string[] { "#EXTINF:" }, StringSplitOptions.None);

            PlaylistData[] info = new PlaylistData[Instance.ChannelsArray.Length];//creates the info array for X number of channels
            int index = -1;
            foreach (var channel in playlist)
            {
                //Console.WriteLine($"#EXTINF:{channel}");
                if (index > -1)
                {
                    List<string> wordArray = new List<string>();
                    wordArray.Clear();
                    //dont need [0],[11],[12],[13],[14]. Need [1] - [10]
                    wordArray = channel.Split('"').Select((element, index) => index % 2 == 0 ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { element }).SelectMany(element => element).ToList();


                    info[index] = new PlaylistData
                    {
                        xui_id = wordArray[2],
                        tvg_id = wordArray[4],
                        tvg_name = wordArray[6],
                        tvg_logo = wordArray[8],
                        group_title = wordArray[10],
                        stream_url = channel.Substring(channel.LastIndexOf("https"))
                    };

                    string currentChannelId = info[index].xui_id;
                    //Adds the channel data obj to the dict with channel id as the key
                    Instance.playlistDataMap.Add(currentChannelId, info[index]);

                    bool keyExists = Instance.categories.ContainsKey(info[index].group_title);
                    if (keyExists)
                    {
                        //string, Dictionary<string, PlaylistData
                        //Adds the Playlist data obj to under the category key using channel id for the inner dict
                        Instance.categories[info[index].group_title].TryAdd(currentChannelId, Instance.playlistDataMap[currentChannelId]);
                    }
                    else
                    {
                        //Adds the category name as the key and create a new dictionary as the key
                        Instance.categories.TryAdd(info[index].group_title, new Dictionary<string, PlaylistData>());
                        Instance.categories[info[index].group_title].TryAdd(currentChannelId, Instance.playlistDataMap[currentChannelId]);
                    }
                }
                index++;
            }
        }

        public static async Task RetrieveCategories(string user, string pass, string server, string port)
        {
            
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create($"https://{server}:{port}/player_api.php?username={user}&password={pass}&action=get_live_categories");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            // Display the status.
            Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = await reader.ReadToEndAsync();
            // Display the content.
            Debug.WriteLine(responseFromServer);

            ChannelGroups[] info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelGroups[]>(responseFromServer);

            Instance.ChannelGroupsArray = info;

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
    }
}
