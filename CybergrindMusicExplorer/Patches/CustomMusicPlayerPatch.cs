using CybergrindMusicExplorer.Components;
using HarmonyLib;
using UnityEngine;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(CustomMusicPlayer))]
    public class CustomMusicPlayerPatch : MonoBehaviour
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomMusicPlayer), "OnEnable")]
        public static bool CustomMusicPlayer_OnEnable_Prefix(CustomMusicPlayer __instance)
        {
            __instance.transform.gameObject.AddComponent<EnhancedMusicPlayer>();
            Destroy(__instance);
            return false;
        }
    }
}