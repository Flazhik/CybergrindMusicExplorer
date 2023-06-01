using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CybergrindMusicExplorer.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TrackReference
    {
        [JsonProperty("type")] [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public SoundtrackType Type;

        [JsonProperty("reference")] public string Reference;

        [JsonConstructor]
        public TrackReference(SoundtrackType type, string reference)
        {
            Type = type;
            Reference = reference;
        }
    }
}