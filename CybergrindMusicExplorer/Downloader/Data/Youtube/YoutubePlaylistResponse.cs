using System;
using System.Collections.Generic;
using System.Linq;
using CybergrindMusicExplorer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CybergrindMusicExplorer.Downloader.Data.Youtube
{
    public class YoutubePlaylistResponse
    {
        [JsonProperty("contents")]
        [JsonConverter(typeof(PlaylistContentsDeserializer))]
        public List<PlaylistContentEntry> Entries;

        public class PlaylistContentEntry
        {
            [JsonProperty("playlistVideoRenderer")]
            private PlaylistVideoRenderer Renderer;

            public string GetTitle() => Renderer?.Title?.GetTitle();
            
            public string GetVideoId() => Renderer?.VideoId;

            public string GetThumbnail() =>
                Renderer?.Thumbnail?.Thumbnails?.OrderBy(t => t.Width).FirstOrDefault()?.Url;
        }
            
        public class PlaylistVideoRenderer
        {
            [JsonProperty("videoId")]
            public string VideoId;            
            
            [JsonProperty("thumbnail")]
            public Thumbnail Thumbnail;

            [JsonProperty("title")]
            public Title Title;

            public PlaylistVideoRenderer(string videoId)
            {
                VideoId = videoId;
            }
        }        
        
        public class Thumbnail
        {
            [JsonProperty("thumbnails")]
            public List<Thumbnails> Thumbnails;
        }
        
        public class Thumbnails
        {
            [JsonProperty("url")]
            public string Url;
            
            [JsonProperty("width")]
            public string Width;
            
            [JsonProperty("height")]
            public string Height;
        }
        
        public class Title
        {
            [JsonProperty("runs")]
            private List<TitleText> titles;

            public string GetTitle() => titles
                .Select(t => t.Text)
                .OrderBy(t => t.Length)
                .First();
        }

        public class TitleText
        {
            [JsonProperty("text")] public string Text;
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
                    .GetOrNull("twoColumnBrowseResultsRenderer")
                    ?.ArrayOrNull("tabs")
                    ?.First
                    ?.GetOrNull("tabRenderer")
                    ?.GetOrNull("content")
                    ?.GetOrNull("sectionListRenderer")
                    ?.ArrayOrNull("contents")
                    ?.First(c => c.GetOrNull("itemSectionRenderer") != null)
                    .GetOrNull("itemSectionRenderer")
                    ?.ArrayOrNull("contents")
                    ?.First
                    ?.GetOrNull("playlistVideoListRenderer")
                    ?.ArrayOrNull("contents")
                    ?.ToObject<List<PlaylistContentEntry>>();
            }

            public override bool CanConvert(Type objectType)
            {
                return false;
            }
        }
    }
}