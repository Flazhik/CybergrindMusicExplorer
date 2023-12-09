using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
using Newtonsoft.Json;
using UnityEngine;
using static CybergrindMusicExplorer.Util.PathsUtils;

namespace CybergrindMusicExplorer.Components
{
    public class CalmThemeManager : MonoSingleton<CalmThemeManager>
    {
        public CgmePreferences Preferences = new CgmePreferences();
        public bool loaded;
        
        private CybergrindMusicExplorerManager manager;
        private EnemyTracker enemyTracker;
        private MusicManager musicManager;

        private int calmThemeThreshold;

        public void Init()
        {
            manager = CybergrindMusicExplorerManager.Instance;
            musicManager = MusicManager.Instance;
            calmThemeThreshold = manager.CalmThemeEnemiesThreshold;

            LoadCgmePreferences();
            loaded = true;
            
            StartCoroutine(SlowUpdate());
        }

        public void ToggleSpecialEnemy(EnemyType type, bool enable, int threshold)
        {
            var configPresent = Preferences.CalmTheme.SpecialEnemies.ContainsKey(type);

            switch (configPresent)
            {
                case true when !enable:
                    Preferences.CalmTheme.SpecialEnemies.Remove(type);
                    break;
                case false when enable:
                    Preferences.CalmTheme.SpecialEnemies.Add(type, threshold);
                    break;
            }

            SaveCgmePreferences();
        }

        public void ChangeEnemyThreshold(EnemyType type, int threshold)
        {
            var configPresent = Preferences.CalmTheme.SpecialEnemies.ContainsKey(type);

            if (configPresent)
                Preferences.CalmTheme.SpecialEnemies[type] = threshold;
            else
                Preferences.CalmTheme.SpecialEnemies.Add(type, threshold);

            SaveCgmePreferences();
        }

        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        public IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return CheckPlayback();
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        private IEnumerator CheckPlayback()
        {
            if (!manager.PlayCalmTheme)
            {
                ResetRequestedThemes();
                musicManager.PlayBattleMusic();
                yield break;
            }
                
            if (enemyTracker == null)
                enemyTracker = EnemyTracker.Instance;
            
            if (enemyTracker == null)
                yield break;
            
            calmThemeThreshold = manager.CalmThemeEnemiesThreshold;
            
            if (AliveEnemies() <= calmThemeThreshold
                && SpecialEnemiesThresholdReached()
                && !musicManager.targetTheme != musicManager.cleanTheme)
            {
                ResetRequestedThemes();
                musicManager.PlayCleanMusic();
            }
            else if (musicManager.targetTheme != musicManager.battleTheme)
            {
                ResetRequestedThemes();
                musicManager.PlayBattleMusic();
            }

            int AliveEnemies() => enemyTracker.enemies.Count(enemy => !enemy.dead);

            void ResetRequestedThemes() => musicManager.requestedThemes = 0;
        }
        
        private void LoadCgmePreferences()
        {
            CgmePreferences loaderPreferences;
            using (var streamReader = new StreamReader(File.Open(CgmeJsonPath, FileMode.OpenOrCreate)))
                loaderPreferences = JsonConvert.DeserializeObject<CgmePreferences>(streamReader.ReadToEnd());

            if (loaderPreferences?.CalmTheme == null)
                SaveCgmePreferences();
            else
                Preferences = loaderPreferences;
        }
        
        public void SaveCgmePreferences() => File.WriteAllText(CgmeJsonPath, JsonConvert.SerializeObject(Preferences));

        private bool SpecialEnemiesThresholdReached() =>
            enemyTracker.enemies
                .Where(enemy => !enemy.dead)
                .Select(enemy => enemy.enemyType)
                .GroupBy(type => type)
                .All(group => !Preferences.CalmTheme.SpecialEnemies.ContainsKey(group.Key) 
                              || Preferences.CalmTheme.SpecialEnemies[group.Key] >= group.Count());
    }
}