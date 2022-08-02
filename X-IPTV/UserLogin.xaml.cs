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

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    /// 
    public partial class UserLogin : Window
    {
        //TODO: read file when user is selected and load data.
        //Add a Load user button
        private static UserDataSaver.User _currentUser = new UserDataSaver.User();
        private static readonly HttpClient _client = new HttpClient();
        private static string assemblyFolder, saveDir, userFileFullPath;
        public UserLogin()
        {
            InitializeComponent();
            assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            saveDir = assemblyFolder + @"\Users\";
            loadUsersFromDirectory();
        }

        private async void Con_btn_Click(object sender, RoutedEventArgs e)
        {
            busy_ind.IsBusy = true;

            await Connect(usrTxt.Text, passTxt.Text, serverTxt.Text, portTxt.Text);//Connect to the server

            busy_ind.BusyContent = "Loading channels list...";

            await LoadChannels(usrTxt.Text, passTxt.Text, serverTxt.Text, portTxt.Text);//Pull the data from the server

            var channelWindow = new ChannelList();

            //load epg. Eventually make it optional
            busy_ind.BusyContent = "Loading playlist data...";

            await LoadPlaylistData(usrTxt.Text, passTxt.Text, serverTxt.Text, portTxt.Text);//Load epg it into the channels array

            channelWindow.Show();

            this.Close();
        }
        private async Task Connect(string user, string pass, string server, string port)
        {
            // Create a request for the URL. 		
            WebRequest request;
            if ((bool)protocolCheckBox.IsChecked)//use the https protocol
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
            Console.WriteLine(responseFromServer);

            PlayerInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInfo>(responseFromServer);

            Instance.PlayerInfo = info;

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
        private async Task LoadChannels(string user, string pass, string server, string port)
        {
            // Create a request for the URL. 	
            WebRequest request;
            if ((bool)protocolCheckBox.IsChecked)//use the https protocol
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
            Console.WriteLine(responseFromServer);

            ChannelEntry[] info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelEntry[]>(responseFromServer);

            Instance.ChannelsArray = info;

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
        private void loadUsersFromDirectory()
        {
            //string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //string saveDir = assemblyFolder + @"\Users";
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);
            DirectoryInfo DI = new DirectoryInfo(saveDir);
            FileInfo[] files = DI.GetFiles("*.txt");
            //Read files from dir
            foreach (var file in files)
            {
                UsercomboBox.Items.Add(file.Name.Remove(file.Name.IndexOf('.')));
            }
        }
        private void loadDataIntoTextFields()
        {
            if (_currentUser?.UserName == null || _currentUser?.Password == null || _currentUser?.Server == null || _currentUser?.Port == null)
            {
                MessageBox.Show("User data is missing, unable to load " + UsercomboBox.SelectedValue.ToString());
                return;
            }

            usrTxt.Text = _currentUser.UserName;
            passTxt.Text = _currentUser.Password;
            serverTxt.Text = _currentUser.Server;
            portTxt.Text = _currentUser.Port;
        }
        private void loadUserDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UsercomboBox.SelectedItem == null)
            {
                MessageBox.Show("You must select a user to load");
                return;
            }

            _currentUser = UserDataSaver.GetUserData(UsercomboBox.SelectedValue.ToString(), getUserFileLocalPath());
            loadDataIntoTextFields();
            UsercomboBox.SelectedItem = null;
        }
        private string getUserFileLocalPath()
        {
            string? selectedUser = UsercomboBox.SelectedValue.ToString();
            if (selectedUser != null && selectedUser.Length > 0)
            {
                return userFileFullPath = saveDir + selectedUser + ".txt";
            }
            else
            {
                MessageBox.Show("You must select a user");
                return null;
            }
        }
        private async Task LoadPlaylistData(string user, string pass, string server, string port)
        {
            //retrieve playlist data from client
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

            var stringTask = _client.GetStringAsync($"https://{server}:{port}/get.php?username={user}&password={pass}");

            var msg = await stringTask;
            //Console.Write(msg);

            //parse the m3u playlist and split into an array
            string[] playlist = msg.Split(new string[] { "#EXTINF:" }, StringSplitOptions.None);

            //needs cleaned up
            PlaylistData[] info = new PlaylistData[Instance.ChannelsArray.Length];
            int index = -1;
            Instance.playlistDataMap = new Dictionary<string, PlaylistData>();
            foreach (var channel in playlist)
            {
                //Console.WriteLine($"#EXTINF:{channel}");
                if (index > -1)
                {
                    //eventually fix this, make the split better and use all of the data in the PlaylistData class
                    string[] splitArr = channel.Split(' ');
                    string xui_id = "";
                    foreach (Match match in Regex.Matches(splitArr[1], "\"([^\"]*)\""))
                        xui_id = match.ToString().Replace("\"", "");
                    info[index] = new PlaylistData
                    {
                        xui_id = xui_id,
                        stream_url = channel.Substring(channel.LastIndexOf("https"))
                    };
                    Instance.playlistDataMap.Add(info[index].xui_id, info[index]);
                }
                index++;
            }
            Console.WriteLine("Done.");
        }

        private void saveUserDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (usrTxt.Text == null || usrTxt.Text.Length <= 0)
            {
                MessageBox.Show("Username input field is blank");
                return;
            }

            _currentUser.UserName = usrTxt.Text;
            _currentUser.Password = passTxt.Text;
            _currentUser.Server = serverTxt.Text;
            _currentUser.Port = portTxt.Text;
            UserDataSaver.SaveUserData(_currentUser);
            MessageBox.Show(_currentUser.UserName + "'s data saved");
        }

        private void usrTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServer.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }

        private void passTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServer.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }

        private void serverTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServer.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }

        private void portTxt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            textBoxServer.Text = "https://" + serverTxt.Text + ":" + portTxt.Text + "/player_api.php?username=" + usrTxt.Text + "&password=" + passTxt.Text;
        }
    }
}
