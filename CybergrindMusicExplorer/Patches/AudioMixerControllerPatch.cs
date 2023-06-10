using HarmonyLib;
using UnityEngine;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(AudioMixerController))]
    public class AudioMixerControllerPatch
    {
        private static CybergrindMusicExplorerManager _manager;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioMixerController), "SetMusicVolume")]
        public static bool AudioMixerController_SetMusicVolume_Prefix(float volume, AudioMixerController __instance)
        {
            if (_manager == null)
                _manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;


            var boost = _manager.allowMusicBoost ? _manager.CustomTracksBoost : 0;
            if (!__instance.forceOff)
                __instance.musicSound.SetFloat("allVolume", __instance.CalculateMusicVolume(volume) + boost);
            __instance.musicVolume = volume;

            return false;
        }
    }
}