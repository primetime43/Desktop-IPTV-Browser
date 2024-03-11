using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http.Json;

namespace X_IPTV.Service
{
    class GitHubReleaseChecker
    {
        public class Release
        {
            public string tag_name { get; set; }
            public string name { get; set; }
            public DateTime published_at { get; set; }
        }

        public class ReleaseChecker
        {
            private static readonly HttpClient client = new HttpClient();
            private const string apiUrl = "https://api.github.com/repos/{0}/{1}/releases";
            public static string releaseURL = "https://github.com/{0}/{1}/releases";

            public static async Task<Release> CheckForNewRelease(string owner, string repo)
            {
                var url = string.Format(apiUrl, owner, repo);
                releaseURL = string.Format(releaseURL, owner, repo);

                if (!client.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "request");
                }

                try
                {
                    var releases = await client.GetFromJsonAsync<List<Release>>(url);
                    return releases.FirstOrDefault(); // Returns the first or default if no releases found
                }
                catch (HttpRequestException ex)
                {
                    // If it's a rate limit error or other 4xx/5xx errors, handle it here
                    if (ex.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show($"Error checking for new release: {ex.Message}\nPlease try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show($"Error checking for new release: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            public static int convertVersionToInt(string version)
            {
                string[] parts = version.Split('.');
                int major = int.Parse(parts[0].TrimStart('v'));
                int minor = int.Parse(parts[1]);
                int patch = int.Parse(parts[2]);
                int versionInt = major * 10000 + minor * 100 + patch;
                return versionInt;
            }
        }
    }
}