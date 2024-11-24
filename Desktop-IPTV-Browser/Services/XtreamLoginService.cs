using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Desktop_IPTV_Browser.Services
{
    public class XtreamLoginService
    {
        private readonly HttpClient _httpClient;
        private readonly string _saveDir;

        public XtreamLoginService()
        {
            _httpClient = new HttpClient();
            _saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XtreamUsers");
        }

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

                    if (json["user_info"]?["auth"].ToString() == "1")
                    {
                        return true; // Login successful
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error checking login connection: {ex.Message}");
            }

            return false; // Login failed
        }

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

        public JObject LoadUserData(string username)
        {
            string filePath = Path.Combine(_saveDir, $"{username}.json");
            if (File.Exists(filePath))
            {
                return JObject.Parse(File.ReadAllText(filePath));
            }
            return null;
        }
    }
}
