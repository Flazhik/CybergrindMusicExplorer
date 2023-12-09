using System;
using System.Collections.Generic;
using System.Linq;
using CybergrindMusicExplorer.Util;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CybergrindMusicExplorer.Downloader.Data.Soundcloud
{
    public class SoundcloudTrackResponse
    {
        [JsonProperty("artwork_url")]
        public string ArtworkUrl;        
        
        [JsonProperty("permalink_url")]
        public string Permalink;
        
        [JsonProperty("title")]
        public string Title;
        
        [JsonProperty("publisher_metadata")] [CanBeNull]
        public PublisherMetadata Publisher;        
        
        [JsonProperty("media")]
        [JsonConverter(typeof(PlaylistContentsDeserializer))]
        private List<TranscodingsEntry> transcodings;

        public string GetDownloadUrl() => transcodings.FirstOrDefault(t =>
            t.Format.MimeType.Equals("audio/mpeg") && t.Format.Protocol.Equals("progressive"))?.Url;

        public class PublisherMetadata
        {
            [JsonProperty("artist")]
            public string Artist;
        }

        public class TranscodingsEntry
        {
            [JsonProperty("url")]
            public string Url;            
            
            [JsonProperty("format")]
            public TranscodingsFormat Format;
        }
        
        public class TranscodingsFormat
        {
            [JsonProperty("protocol")]
            public string Protocol;
            
            [JsonProperty("mime_type")]
            public string MimeType;
        }
            
        private class PlaylistContentsDeserializer : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return string.Empty;

                return JObject.Load(reader)
                    .ArrayOrNull("transcodings")
                    ?.ToObject<List<TranscodingsEntry>>();
            }

            public override bool CanConvert(Type objectType)
            {
                return false;
            }
        }
    }
}