using Newtonsoft.Json.Linq;

namespace CybergrindMusicExplorer.Util
{
    public static class JsonUtils
    {
        public static JToken GetOrNull(this JToken obj, string key)
        {
            if (obj[key] == default || obj[key].Type == JTokenType.Null || obj[key].Type == JTokenType.Undefined)
                return null;
            

            return obj[key];
        }
        
        public static JArray ArrayOrNull(this JToken obj, string key)
        {
            if (obj[key] == default || obj[key].Type != JTokenType.Array)
                return null;

            return (JArray)obj[key];
        }
    }
}