using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CybergrindMusicExplorer.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TrackReference : IEquatable<TrackReference>
    {
        [JsonProperty("type")] [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public readonly SoundtrackType Type;

        [JsonProperty("reference")] public readonly string Reference;

        [JsonConstructor]
        public TrackReference(SoundtrackType type, string reference)
        {
            Type = type;
            Reference = reference;
        }

        public bool Equals(TrackReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && Reference == other.Reference;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TrackReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Type * 397) ^ (Reference != null ? Reference.GetHashCode() : 0);
            }
        }

        private sealed class TypeReferenceEqualityComparer : IEqualityComparer<TrackReference>
        {
            public bool Equals(TrackReference x, TrackReference y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Type.Equals(y.Type) && x.Reference == y.Reference;
            }

            public int GetHashCode(TrackReference obj)
            {
                unchecked
                {
                    return ((int)obj.Type * 397) ^ (obj.Reference != null ? obj.Reference.GetHashCode() : 0);
                }
            }
        }

        public static IEqualityComparer<TrackReference> TypeReferenceComparer { get; } = new TypeReferenceEqualityComparer();
    }
}