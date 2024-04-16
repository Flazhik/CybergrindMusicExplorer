using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CybergrindMusicExplorer.Components
{
    public class SpecialEnemies
    {
        private const string SpawnableObjectDatabaseGuid = "c2192e0756ac3e44ab8e5475d0aac73e";
        
        public static readonly Dictionary<EnemyType, string> SpecialEnemiesNames = new Dictionary<EnemyType, string>
        {
            { EnemyType.Sisyphus, "SISYPHEAN INSURRECTIONIST" },
            { EnemyType.Mindflayer, "MINDFLAYER" },
            { EnemyType.Ferryman, "FERRYMAN" },
            { EnemyType.HideousMass, "HIDEOUS MASS" },
            { EnemyType.Swordsmachine, "SWORDSMACHINE" },
            { EnemyType.Gutterman, "GUTTERMAN" },
            { EnemyType.Guttertank, "GUTTERTANK" },
            { EnemyType.Mannequin, "MANNEQUIN" },
            { EnemyType.Cerberus, "CERBERUS" },
            { EnemyType.Drone, "DRONE" },
            { EnemyType.MaliciousFace, "MALICIOUS FACE" },
            { EnemyType.Streetcleaner, "STREETCLEANER" },
            { EnemyType.Virtue, "VIRTUE" },
            { EnemyType.Stalker, "STALKER" },
            { EnemyType.Stray, "STRAY" },
            { EnemyType.Schism, "SCHISM" },
            { EnemyType.Soldier, "SOLDIER" },
            { EnemyType.Turret, "TURRET" },
            { EnemyType.Filth, "FILTH" },
            { EnemyType.Idol, "IDOL" }
        };

        public static Dictionary<EnemyType, Sprite> LoadEnemiesSprites()
        {
            var handle = new AssetReferenceT<SpawnableObjectsDatabase>(SpawnableObjectDatabaseGuid).LoadAssetAsync();
            var database = handle.WaitForCompletion();
            Addressables.Release(handle);

            return database.enemies
                .ToList()
                .Where(e => e.enemyType != EnemyType.Centaur)
                .Select(enemy => new { enemy.enemyType, enemy.gridIcon })
                .ToDictionary(enemy => enemy.enemyType, enemy => enemy.gridIcon);
        }
    }
}