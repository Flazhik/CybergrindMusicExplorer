using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CybergrindMusicExplorer.Downloader.Data.Youtube
{
    public class YoutubePlayerResponse
    {
        [JsonProperty("videoDetails")]
        public Details VideoDetails;        
        
        [JsonProperty("streamingData")]
        public StreamingData Streaming;
        
        public string GetTitle() => VideoDetails.Title;
        
        public string GetThumbnail() => VideoDetails?.Thumbnail?.Thumbnails?[1].URL;

        public string GetMp4AudioUrl() => Streaming.Formats
                .Where(format => format.Mime.Contains("video/mp4") && format.Url.Contains("ratebypass=yes"))
                .OrderByDescending(format => int.Parse(format.AudioSampleRate))
                .Select(format => format.Url)
                .FirstOrDefault();

        public class Details
        {
            [JsonProperty("title")]
            public string Title;
            
            [JsonProperty("thumbnail")]
            public Thumbnail Thumbnail;
        }
        
        public class StreamingData
        {
            [JsonProperty("formats")]
            public List<StreamFormat> Formats;
        }
        
        public class StreamFormat
        {
            [JsonProperty("mimeType")]
            public string Mime;
            
            [JsonProperty("url")]
            public string Url;
            
            [JsonProperty("bitrate")]
            public string Bitrate;            
            
            [JsonProperty("audioSampleRate")]
            public string AudioSampleRate;
        }

        public class Thumbnail
        {
            [JsonProperty("thumbnails")]
            public List<ThumbnailObject> Thumbnails;
        }

        public class ThumbnailObject
        {
            [JsonProperty("url")]
            public string URL;
        }
    }
}