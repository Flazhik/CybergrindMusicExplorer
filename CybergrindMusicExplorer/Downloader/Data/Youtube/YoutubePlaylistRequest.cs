using Newtonsoft.Json;

namespace CybergrindMusicExplorer.Downloader.Data.Youtube
{
    public class YoutubePlaylistRequest
    {
        private YoutubePlaylistRequest()
        {
        }
        
        [JsonProperty("browseId")]
        public string PlaylistId;        
        
        [JsonProperty("context")]
        public YoutubePlayerContext Context = new YoutubePlayerContext();

        public class YoutubePlayerContext
        {
            [JsonProperty("client")]
            public YoutubePlayerClient Client = new YoutubePlayerClient();
        }
        
        public class YoutubePlayerClient
        {
            [JsonProperty("clientName")]
            private string clientName = "WEB";
                
            [JsonProperty("clientVersion")]
            private string clientVersion = "2.20210401.08.00";

            [JsonProperty("utcOffsetMinutes")]
            private int utcOffsetMinutes = 0;
        }

        public static YoutubePlaylistRequest ForPlaylistId(string playlistId)
        {
            return new YoutubePlaylistRequest
            {
                PlaylistId = $"VL{playlistId}"
            };
        }
    }
}