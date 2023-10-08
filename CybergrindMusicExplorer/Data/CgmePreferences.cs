using System.Collections.Generic;
using Newtonsoft.Json;

namespace CybergrindMusicExplorer.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CgmePreferences
    {
        [JsonProperty("calmThemeConfig")] public CalmThemeConfig CalmTheme = new CalmThemeConfig();
        
        public class CalmThemeConfig
        {
            [JsonProperty("specialEnemies", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            
            public Dictionary<EnemyType, int> SpecialEnemies =
                new Dictionary<EnemyType, int>
                {
                    { EnemyType.Sisyphus, 1 },
                    { EnemyType.Mindflayer, 1 },
                    { EnemyType.Ferryman, 1 },
                    { EnemyType.HideousMass, 1 },
                    { EnemyType.Swordsmachine, 2 }
                };
        }
    }
}