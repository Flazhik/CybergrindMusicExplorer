using System;
using Newtonsoft.Json;

namespace CybergrindMusicExplorer.Data
{
    public class GitHubLatest
    {
        private static readonly Version OldVersion = Version.Parse("1.0.0");
        
        [JsonConverter(typeof(VersionDeserializer))]
        [JsonProperty("tag_name")]
        public Version Version;

        private class VersionDeserializer : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType != JsonToken.String)
                    return OldVersion;

                return Version.TryParse(reader.Value.ToString().Substring(1), out var version)
                    ? version
                    : OldVersion;
            }

            public override bool CanConvert(Type objectType)
            {
                return false;
            }
        }
    }
}