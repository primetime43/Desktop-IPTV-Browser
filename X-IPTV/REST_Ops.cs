using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

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

        public static async Task RetrieveChannelData(string user, string pass, string server, string port)//maybe pass in the action as a string and use this for all action calls
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
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = await reader.ReadToEndAsync();
            // Display the content.
            Debug.WriteLine(responseFromServer);

            //Channels are loaded here
            ChannelEntry[] channelInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelEntry[]>(responseFromServer);

            Instance.ChannelsArray = channelInfo;

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            await LoadPlaylistData(user, pass, server, port);//hide this call from where RetrieveChannelData is called
        }

        public static async Task RetrieveCategories(string user, string pass, string server, string port)
        {

            // Create a request for the URL.
            WebRequest request;
            if ((bool)ul.protocolCheckBox.IsChecked)//use the https protocol
                request = WebRequest.Create($"https://{server}:{port}/player_api.php?username={user}&password={pass}&action=get_live_categories");
            else//use the http protocol
                request = WebRequest.Create($"http://{server}:{port}/player_api.php?username={user}&password={pass}&action=get_live_categories");
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
            Debug.WriteLine(responseFromServer);

            ChannelGroups[] info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelGroups[]>(responseFromServer);

            Instance.ChannelGroupsArray = info;

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        //seems the get.php api call is the only one that includes the stream url in the response
        public static async Task LoadPlaylistData(string user, string pass, string server, string port)//maybe remove this one
        {
            //This method uses parsing instead of json Deserializing because this response doesn't return in json format/json formattable

            //playlistDataMap is a dictionary containing the xui_id as the key and value being the PlaylistData object
            Instance.playlistDataMap = new Dictionary<string, PlaylistData>(); 

            //*** Maybe change this from httpclient to webrequest eventually ***

            //retrieve channel data from client
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

            Task<string> stringTask = null;
            if ((bool)ul.protocolCheckBox.IsChecked)//use the https protocol
                stringTask = _client.GetStringAsync($"https://{server}:{port}/get.php?username={user}&password={pass}");
            else//use the http protocol
                stringTask = _client.GetStringAsync($"http://{server}:{port}/get.php?username={user}&password={pass}");

            //var stringTask = _client.GetStringAsync($"https://{server}:{port}/get.php?username={user}&password={pass}");

            var serverResponse = await stringTask;

            //parse the m3u playlist and split into an array. playlist array contains the unparsed data
            string[] playlist = serverResponse.Split(new string[] { "#EXTINF:" }, StringSplitOptions.None);

            PlaylistData[] info = new PlaylistData[Instance.ChannelsArray.Length];//creates the info array for X number of channels
            int index = -1;
            foreach (var channel in playlist)
            {
                if (index > -1)
                {
                    List<string> wordArray = new List<string>();
                    wordArray.Clear();
                    //dont need [0],[11],[12],[13],[14]. Need [1] - [10]
                    wordArray = channel.Split('"').Select((element, index) => index % 2 == 0 ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { element }).SelectMany(element => element).ToList();

                    //only need the stream_url, but might as well get the other data while here
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
                }
                index++;
            }
            _client.Dispose();
            //Debug.WriteLine(info);
        }

        //This is a a lot of data, so probably make the load for epg data optional. Going to need to convert from xml to json
        public static async Task LoadEPGData(string user, string pass, string server, string port)
        {

            //This method uses parsing instead of json Deserializing because this response doesn't return in json format/json formattable

            //*** Doesn't seem to want to work with Webrequest, so have to use HttpClient ***

            //retrieve channel data from client
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

            Task<string> stringTask = null;
            if ((bool)ul.protocolCheckBox.IsChecked)//use the https protocol
                stringTask = _client.GetStringAsync($"https://{server}:{port}/xmltv.php?username={user}&password={pass}");
            else//use the http protocol
                stringTask = _client.GetStringAsync($"http://{server}:{port}/xmltv.php?username={user}&password={pass}");

            var serverResponse = await stringTask;

            //after the server responds with the xml data and converts the xml data to json
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(serverResponse);
            string xmlToJsonResult = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);

            //parsing the json data to JObject
            JObject jsonEPG = JObject.Parse(xmlToJsonResult);

            List<JToken> tempTest = jsonEPG["tv"]["channel"].Children().ToList();

            Debug.WriteLine(tempTest);

            Channel[] info = new Channel[tempTest.Count];//creates the info array for X number of channels
            int index = -1;

            //Use PlaylistData tvg_id and Channel id as unique id
            foreach (var channel in tempTest)
            {
                if (index > -1)
                {
                    string channelID = (string)tempTest[index]["@id"];
                    string displayName = (string)tempTest[index]["display-name"];
                    info[index] = new Channel
                    {
                        id = channelID,
                        display_name = displayName
                    };
                    //Use PlaylistData tvg_id and Channel id as unique id. May be able to use name from ChannelEntry to Channel display-name to make it simpler
                    //Need to take the 
                    string currentChannelDisplayName = info[index].display_name;
                    for(int i = 0; i < Instance.ChannelsArray.Length; i++)
                    {
                        if(Instance.ChannelsArray[i].name = currentChannelDisplayName)
                        {
                            //set title to Programme title
                            //set desc to Programme desc
                        }
                    }
                    //Adds the channel data obj to the dict with channel id as the key
                    Instance.playlistDataMap.Add(currentChannelId, info[index]);
                    Instance.ChannelsArray[0].tv
                    Debug.WriteLine(info);
                }
                index++;
            }

            Debug.WriteLine(info);

            _client.Dispose();

            //Channels are loaded here
            //ChannelEntry[] channelInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelEntry[]>(responseFromServer);

            //Instance.ChannelsArray = channelInfo;
        }
    }
}
