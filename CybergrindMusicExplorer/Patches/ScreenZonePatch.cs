using CybergrindMusicExplorer.Components;
using HarmonyLib;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(ScreenZone))]
    public class ScreenZonePatch
    {
        private static ScreenZoneHelper helper;
        private static ScreenZone cgTerminal;
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScreenZone), "Update")]
        public static bool ScreenZone_Update_Prefix(bool ___inZone, ScreenZone __instance)
        {
            if (cgTerminal == null)
            {
                if (__instance.name.Equals("CyberGrindSettings"))
                    cgTerminal = __instance;
                else
                    return true;
            }

            if (cgTerminal != __instance)
                return true;

            if (helper == null)
            {
                if (!__instance.TryGetComponent<ScreenZoneHelper>(out var h))
                    return true;
                helper = h;
            }

            helper.inZone = ___inZone;
            return true;
        }
    }
}