using HarmonyLib;
using static CybergrindMusicExplorer.Util.Patching.PatchRequest;

namespace CybergrindMusicExplorer.Patches
{
    public static class Patches
    {
        private static readonly Harmony Harmony = new Harmony("Flazhik.ULTRAKILL.CybergrindMusicExplorer");

        public static void PatchOnStartOptionsMenu()
        {
            PatchMethod(typeof(OptionsMenuToManager), "Start")
                .WithPrefix(typeof(OptionsMenuToManagerPatch), "OptionsMenuToManager_Start_Prefix")
                .Using(Harmony)
                .Once();
        }
                
        public static void PatchOnEnableOptionsMenu()
        {
            PatchMethod(typeof(OptionsMenuToManager), "OpenOptions")
                .WithPrefix(typeof(OptionsMenuToManagerPatch), "OptionsMenuToManager_OnEnable_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchAudioMixer()
        {
            PatchMethod(typeof(AudioMixerController), "SetMusicVolume")
                .WithPrefix(typeof(AudioMixerControllerPatch), "AudioMixerController_SetMusicVolume_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchMusicPlayer()
        {
            PatchMethod(typeof(CustomMusicPlayer), "OnEnable")
                .WithPrefix(typeof(CustomMusicPlayerPatch), "CustomMusicPlayer_OnEnable_Prefix")
                .Using(Harmony)
                .Once();
        }
    }
}