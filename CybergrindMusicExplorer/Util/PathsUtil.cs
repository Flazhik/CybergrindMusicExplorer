using System.IO;
using UnityEngine;

namespace CybergrindMusicExplorer.Util
{
    public static class PathsUtil
    {
        private static readonly string UltrakillPath =
            Directory.GetParent(Application.dataPath)?.FullName ?? FallbackUltrakillPath;
        private const string FallbackUltrakillPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ULTRAKILL";
        
        public static readonly string PlaylistJsonPath = Path.Combine(UltrakillPath, "Preferences", "EnhancedPlaylist.json");
        public static readonly string CustomSongsPath = Path.Combine(UltrakillPath, "CyberGrind", "Music");
        
        public static readonly DirectoryInfo SpecialEffectsPath = new DirectoryInfo(Path.Combine(CustomSongsPath, "CGME"));
    }
}