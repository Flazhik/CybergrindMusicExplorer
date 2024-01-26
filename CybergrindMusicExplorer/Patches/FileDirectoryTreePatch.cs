using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using static CybergrindMusicExplorer.Util.PathsUtils;
using static CybergrindMusicExplorer.Util.CustomTracksNamingUtil;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(FileDirectoryTree))]
    public class FileDirectoryTreePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FileDirectoryTree), "GetFilesRecursive")]
        public static bool FileDirectoryTree_GetFilesRecursive_Prefix(ref IEnumerable<FileInfo> __result, FileDirectoryTree __instance)
        {
            if (!__instance.realDirectory.FullName.StartsWith(CustomSongsPath))
                return true;
            
            __result = __instance.children
                .Where(child => !IsSpecialEffectsFolder(child))
                .SelectMany(child => child.GetFilesRecursive())
                .Concat(__instance.files);
            return false;
        }        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FileDirectoryTree), "Refresh")]
        public static bool FileDirectoryTree_Refresh_Prefix(FileDirectoryTree __instance)
        {
            if (!__instance.realDirectory.FullName.StartsWith(CustomSongsPath))
                return true;
            
            __instance.realDirectory.Create();

            var tracks = __instance.realDirectory.GetFiles();
            var segmentedTracks = tracks
                .Where(file => AudioTypesByExtension.Keys.Contains(file.Extension.ToLower()))
                .Where(IntroOrLoopPart)
                .GroupBy(file => WithoutPostfix(file).FullName)
                .Where(HasIntroAndLoop)
                .ToList();

            var regularTracks = tracks
                .Where(file => AudioTypesByExtension.Keys.Contains(file.Extension.ToLower()))
                .Where(track => !HasSpecialPostfix(track, "calmintro") 
                                && !HasSpecialPostfix(track, "calmloop")
                                && !HasSpecialPostfix(track, "calm"))
                .Select(track => track.FullName)
                .Where(track => !segmentedTracks
                    .SelectMany(t => t)
                    .Select(t => t.FullName)
                    .Contains(track))
                .ToList();

            var files = segmentedTracks
                .Select(group => group.Key)
                .Concat(regularTracks)
                .Select(track => new FileInfo(Path.Combine(CustomSongsPath, track.Substring(CustomSongsPath.Length + 1))))
                .ToArray();

            SetPrivate(__instance, "name", __instance.realDirectory.Name);
            SetPrivate(__instance, "children", __instance.realDirectory
                .GetDirectories()
                .Where(dir => dir.FullName != SpecialEffectsDirectory.FullName)
                .Select(dir => new FileDirectoryTree(dir, __instance)));
            SetPrivate(__instance, "files", files);
            return false;
        }

        private static bool IsSpecialEffectsFolder(IDirectoryTree<FileInfo> dir) =>
            dir.name == "CGME" && dir.parent?.parent == null;
        
        private static void SetPrivate(IDirectoryTree instance, string paramName, object value) =>
            typeof(FileDirectoryTree).GetProperty(paramName)!.SetValue(instance, value, null);
    }
}