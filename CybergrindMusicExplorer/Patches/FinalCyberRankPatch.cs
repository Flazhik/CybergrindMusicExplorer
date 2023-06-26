using CybergrindMusicExplorer.Components;
using HarmonyLib;
using UnityEngine;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(FinalCyberRank))]
    public class FinalCyberRankPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FinalCyberRank), "GameOver")]
        public static bool FinalCyberRank_GameOver_Prefix()
        {
            var player = (EnhancedMusicPlayer)Object.FindObjectOfType(typeof(EnhancedMusicPlayer));
            if (player != null)
                player.StopPlaylist();

            return true;
        }
    }
}