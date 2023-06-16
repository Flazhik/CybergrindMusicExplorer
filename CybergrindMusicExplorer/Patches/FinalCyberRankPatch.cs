using CybergrindMusicExplorer.Components;
using HarmonyLib;
using UnityEngine;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(FinalCyberRank))]
    public class FinalCyberRankPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MusicManager), "GameOver")]
        public static bool MusicManager_GameOver_Prefix(MusicManager __instance)
        {
            var player = (EnhancedMusicPlayer)Object.FindObjectOfType(typeof(EnhancedMusicPlayer));
            if (player != null)
                player.StopPlaylist();

            return true;
        }
    }
}