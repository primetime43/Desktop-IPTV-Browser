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
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using System.Xml;

namespace X_IPTV
{
    /*
     * player_api.php?action=get_live_streams
     * 
     * player_api.php?action=get_live_categories
     * 
     * player_api.php?action=get_live_categories
     * 
     * xmltv.php xml data channel ids, display name, icon src. Has the desc for channels
     * 
     * get.php
     */
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
            Instance.playlistDataMap = new Dictionary<string, PlaylistEPGData>();

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

            PlaylistEPGData[] info = new PlaylistEPGData[Instance.ChannelsArray.Length];//creates the info array for X number of channels
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
                    info[index] = new PlaylistEPGData
                    {
                        xui_id = wordArray[2],
                        tvg_id = wordArray[4],
                        tvg_name = wordArray[6],
                        tvg_logo = wordArray[8],
                        group_title = wordArray[10],
                        stream_url = channel.Substring(channel.LastIndexOf("https"))
                    };
                    string currentChannelId = info[index].xui_id;
                    string currentChannelNameId = info[index].tvg_id;
                    //Adds the channel data obj to the dict with channel id as the key
                    if (currentChannelNameId != "" && currentChannelNameId != null)
                    {
                        if (!Instance.playlistDataMap.ContainsKey(currentChannelNameId))
                        {
                            //playlistDataMap is the only one that contains the stream url
                            Instance.playlistDataMap.Add(currentChannelId, info[index]);
                            //need to use the xui_id with it to make it more unqiue since there are multiple tvg_id with the same.
                            //there a some with multiples due to hd and sd
                        }
                        else
                            MessageBox.Show("Dup key debug: " + currentChannelNameId);
                    }
                }
                index++;
            }
            //_client.Dispose();
            //Debug.WriteLine(info);
        }

        //This is a lot of data, so probably make the load for epg data optional. Going to need to convert from xml to json
        public static async Task LoadEPGDataWDesc(string user, string pass, string server, string port)
        {
            //*** Doesn't seem to want to work with Webrequest, so have to use HttpClient ***

            //retrieve channel data from client
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");


            /*Task<string> stringTask = null;
            if ((bool)ul.protocolCheckBox.IsChecked)//use the https protocol
                stringTask = _client.GetStringAsync($"https://{server}:{port}/xmltv.php?username={user}&password={pass}");
            else//use the http protocol
                stringTask = _client.GetStringAsync($"http://{server}:{port}/xmltv.php?username={user}&password={pass}");*/

            try
            {
                //Wrapped this in a try catch test
                Task<string> stringTask = null;
                if ((bool)ul.protocolCheckBox.IsChecked)//use the https protocol
                    stringTask = _client.GetStringAsync($"https://{server}:{port}/xmltv.php?username={user}&password={pass}");
                else//use the http protocol
                    stringTask = _client.GetStringAsync($"http://{server}:{port}/xmltv.php?username={user}&password={pass}");

                var serverResponse = await stringTask;

                //var serverResponse = await stringTask;
                //to here; try catch test




                //after the server responds with the xml data and converts the xml data to json
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(serverResponse);
                //JSON as string
                string xmlToJsonResult = JsonConvert.SerializeXmlNode(doc);

                //parsing further down the json tree. The JSON is wrapped inside an array.
                string channelsHeaderInfo = JObject.Parse(xmlToJsonResult)["tv"]["channel"].ToString();
                string channelsWDesc = JObject.Parse(xmlToJsonResult)["tv"]["programme"].ToString();

                //deserialize the json string into an array of the Channel obj
                Channel[] arrayOfChannelNames = JsonConvert.DeserializeObject<Channel[]>(channelsHeaderInfo);
                //deserialize the json string into an array of the Programme obj
                Programme[] arrayOfChannelEpgData = JsonConvert.DeserializeObject<Programme[]>(channelsWDesc);

                //key is the channel names: US | REELZ, value is the Channel obj
                Dictionary<string, Channel> channelsDict = arrayOfChannelNames.ToDictionary(ch => ch.display_name, ch => ch);

                //make the key be the ReelzChannel.us
                //make the value be a list of Programmes since there a multiple for different times
                Dictionary<string, List<Programme>> programmeDict = new Dictionary<string, List<Programme>>();
                for (int i = 0; i < arrayOfChannelEpgData.Length; i++)
                {
                    string currentChannelID = arrayOfChannelEpgData[i].channel;
                    if (!programmeDict.ContainsKey(currentChannelID))//doesnt have the key yet, so add it
                    {
                        programmeDict.Add(currentChannelID, new List<Programme>() { arrayOfChannelEpgData[i] });
                    }
                    else if (programmeDict.ContainsKey(currentChannelID))
                    {
                        programmeDict[currentChannelID].Add(arrayOfChannelEpgData[i]);
                    }
                    else
                        Debug.WriteLine("Error trying to add ChannelID as a key");
                }


                foreach (KeyValuePair<string, Channel> entry in channelsDict)
                {
                    string channelIDTest = channelsDict[entry.Key].id;//ReelzChannel.us
                    string channelDisplayNameTest = channelsDict[entry.Key].display_name;//<display-name>US | REELZ</display-name>

                    //Debug.WriteLine(programmeDict[channelIDTest]);

                    //bug here where some keys arent present in programmeDict.
                    if (programmeDict.ContainsKey(channelIDTest))
                    {
                        for (int iTest = 0; iTest < programmeDict[channelIDTest].Count; iTest++)
                        {
                            DateTime start_time = UnixTimeStampToDateTime(Convert.ToDouble(programmeDict[channelIDTest][iTest].start_timestamp));
                            DateTime end_time = UnixTimeStampToDateTime(Convert.ToDouble(programmeDict[channelIDTest][iTest].stop_timestamp));

                            string formatted_start_time = String.Format("{0:t}", start_time).ToString();
                            string formatted_end_time = String.Format("{0:t}", end_time).ToString();

                            if ((DateTime.Now > start_time) && (DateTime.Now < end_time))
                            {
                                for (int j = 0; j < Instance.ChannelsArray.Length; j++)
                                {
                                    if (channelDisplayNameTest == Instance.ChannelsArray[j].name)
                                    {
                                        //** IMPORTANT **\\
                                        //Here is setting the data that will be displayed on the ChannelList.xaml page and other data that uses the Instance.ChannelsArray
                                        Instance.ChannelsArray[j].title = programmeDict[channelIDTest][iTest].title;
                                        //Instance.ChannelsArray[j].start_timestamp = start_time_split[1] + " - " + end_time_split[1] + " " + end_time_split[2];
                                        Instance.ChannelsArray[j].start_timestamp = formatted_start_time + " - " + formatted_end_time;
                                        Instance.ChannelsArray[j].desc = programmeDict[channelIDTest][iTest].desc;

                                        //test
                                        Instance.ChannelsArray[j].added = UnixTimeStampToDateTime(Convert.ToDouble(Instance.ChannelsArray[j].added)).ToString();
                                    }
                                }
                            }
                        }
                    }
                    else
                        //MessageBox.Show("Key " + channelIDTest + " not in programmeDict");
                        Debug.WriteLine("Key " + channelIDTest + " not in programmeDict");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("{0} Exception caught.", e.ToString());
            }
            _client.Dispose();
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
