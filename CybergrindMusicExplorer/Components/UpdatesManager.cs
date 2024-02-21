using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CybergrindMusicExplorer.Data;
using static System.TimeSpan;
using static Newtonsoft.Json.JsonConvert;

namespace CybergrindMusicExplorer.Components
{
    public static class UpdatesManager
    {
        private const string GitHubUrl =
            "https://api.github.com/repos/flazhik/cybergrindmusicexplorer/releases/latest";

        private static readonly Version CurrentVersion = Version.Parse(PluginInfo.VERSION);
        private static readonly HttpClient Client = new HttpClient();
        
        public static Version NewestVersion;
        
        public static async Task GetNewVersion()
        {
            try
            {
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Client.DefaultRequestHeaders.UserAgent.TryParseAdd("CybergrindMusicExplorer/1.6.2");
                Client.Timeout = FromSeconds(5);

                var raw = await Client.GetStringAsync(GitHubUrl);
                var latest = DeserializeObject<GitHubLatest>(raw);
                NewestVersion = latest.Version;
            }
            catch (Exception)
            {
                NewestVersion = CurrentVersion;
            }
        }
    }
}