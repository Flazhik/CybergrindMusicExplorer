using HarmonyLib;

namespace CybergrindMusicExplorer.Patches
{
    /**
     * MusicManager REALLY doesn't like streamed AudioClips
     */
    [HarmonyPatch(typeof(MusicManager))]
    public class MusicManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MusicManager), "OnEnable")]
        public static bool MusicManager_OnEnable_Prefix(MusicManager __instance)
        {
            if (SceneHelper.CurrentScene != "Endless")
                return true;

            __instance.bossTheme = __instance.battleTheme;
            __instance.cleanTheme = __instance.battleTheme;

            return true;
        }
    }
}