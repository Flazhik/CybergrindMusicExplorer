using System.Collections.Generic;
using Newtonsoft.Json;

namespace CybergrindMusicExplorer.Downloader.Data.Soundcloud
{
    public class SoundcloudPlaylistResponse
    {
        [JsonProperty("tracks")]
        public List<TrackEntry> Tracks;
        
        public class TrackEntry
        {
            [JsonProperty("id")]
            public string Id;            
            
            [JsonProperty("policy")]
            public string Policy;
        }
    }
}