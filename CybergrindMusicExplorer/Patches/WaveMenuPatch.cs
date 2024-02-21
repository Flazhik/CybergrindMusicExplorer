using CybergrindMusicExplorer.Components;
using HarmonyLib;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(WaveMenu))]
    public class WaveMenuPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WaveMenu), "Start")]
        public static void WaveMenu_Start_Postfix(WaveMenu __instance)
        {
            
            __instance.gameObject.AddComponent<ScreenZoneHelper>();
        }
    }
}