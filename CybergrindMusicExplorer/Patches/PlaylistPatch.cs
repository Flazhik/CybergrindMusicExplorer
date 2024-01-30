using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(Playlist))]
    public class PlaylistPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Playlist), "Add")]
        public static bool Playlist_Add_Prefix(Playlist.SongIdentifier id, Playlist __instance)
        {
            return !(CybergrindMusicExplorerManager.Instance.PreventDuplicateTracks && __instance.ids.Contains(id));
        }        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Playlist), "get_currentPath", MethodType.Getter)]
        public static bool Playlist_get_currentPath_Prefix(ref string __result)
        {
            var str = Path.Combine(Playlist.directory.Parent.FullName, "Playlist.json");
            var currentPath = Path.Combine(Playlist.directory.FullName, $"slot{CybergrindMusicExplorerManager.Instance.SelectedPlaylistSlot.ToString()}.json");
            if (File.Exists(str) && !File.Exists(currentPath))
                File.Move(str, currentPath);
            __result = currentPath;
            return false;
        }
    }
}