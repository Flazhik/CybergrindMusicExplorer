using Newtonsoft.Json;

namespace CybergrindMusicExplorer.Downloader.Data.Youtube
{
    public class YoutubePlayerRequest
    {
        private YoutubePlayerRequest()
        {
        }
        
        [JsonProperty("videoId")]
        public string VideoId;        
        
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
            private string clientName = "ANDROID_TESTSUITE";
                
            [JsonProperty("clientVersion")]
            private string clientVersion = "1.9";
            
            [JsonProperty("androidSdkVersion")]
            private int androidSdkVersion = 30;
            
            [JsonProperty("utcOffsetMinutes")]
            private int utcOffsetMinutes = 0;
        }

        public static YoutubePlayerRequest ForVideoId(string videoId)
        {
            return new YoutubePlayerRequest
            {
                VideoId = videoId
            };
        }
    }
}