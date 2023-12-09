using Newtonsoft.Json;

namespace CybergrindMusicExplorer.Downloader.Data.Soundcloud
{
    public class SoundtrackDownloadUrlResponse
    {
        [JsonProperty("url")] public string Url;
    }
}