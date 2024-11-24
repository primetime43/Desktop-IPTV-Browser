using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Desktop_IPTV_Browser.Services
{
    public class M3ULoginService
    {
        private readonly string _saveDir;
        private readonly HttpClient _httpClient;

        public M3ULoginService()
        {
            // Define the directory to store M3U login data
            _saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "M3ULogins");
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Saves M3U login data to a file.
        /// </summary>
        public void SaveM3UData(string fileName, string m3uUrl, string epgUrl)
        {
            if (!Directory.Exists(_saveDir))
                Directory.CreateDirectory(_saveDir);

            string filePath = Path.Combine(_saveDir, $"{fileName}.json");
            var loginData = new
            {
                M3UUrl = m3uUrl,
                EPGUrl = epgUrl
            };

            File.WriteAllText(filePath, JObject.FromObject(loginData).ToString());
        }

        /// <summary>
        /// Loads M3U login data from a file.
        /// </summary>
        public JObject LoadM3UData(string fileName)
        {
            string filePath = Path.Combine(_saveDir, $"{fileName}.json");
            if (File.Exists(filePath))
            {
                return JObject.Parse(File.ReadAllText(filePath));
            }

            return null;
        }

        /// <summary>
        /// Retrieves a list of saved M3U logins.
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
        /// Connects to the M3U playlist URL and optionally processes the EPG URL.
        /// </summary>
        /// <param name="m3uUrl">The M3U playlist URL.</param>
        /// <param name="epgUrl">The optional EPG URL.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ConnectM3U(string m3uUrl, string epgUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(m3uUrl))
                throw new ArgumentException("M3U URL cannot be empty.");

            // Validate the M3U URL
            try
            {
                var m3uResponse = await _httpClient.GetAsync(m3uUrl, cancellationToken);
                if (!m3uResponse.IsSuccessStatusCode)
                    throw new HttpRequestException($"Failed to connect to M3U URL. Status Code: {m3uResponse.StatusCode}");

                Console.WriteLine("M3U playlist loaded successfully.");

                // Optionally process the EPG URL if provided
                if (!string.IsNullOrWhiteSpace(epgUrl))
                {
                    var epgResponse = await _httpClient.GetAsync(epgUrl, cancellationToken);
                    if (!epgResponse.IsSuccessStatusCode)
                        throw new HttpRequestException($"Failed to connect to EPG URL. Status Code: {epgResponse.StatusCode}");

                    Console.WriteLine("EPG data loaded successfully.");
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Operation canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to M3U or EPG: {ex.Message}");
                throw;
            }
        }
    }
}
