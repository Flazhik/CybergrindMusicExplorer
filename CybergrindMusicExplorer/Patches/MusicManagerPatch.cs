using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CybergrindMusicExplorer.Components;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(MusicManager))]
    public class MusicManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MusicManager), "StartMusic")]
        [SuppressMessage("ReSharper", "MustUseReturnValue")]
        public static void MusicManager_StartMusic_Postfix()
        {
            CalmThemeManager.Instance.SlowUpdate();
        }
        
        /*
         * Only allow to change the theme for CalmThemeManager
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MusicManager), "PlayBattleMusic")]
        public static bool MusicManager_PlayBattleMusic_Prefix(MusicManager __instance)
        {
            var caller = new StackTrace().GetFrame(2).GetMethod();
            var callerIsValid = caller.DeclaringType?.DeclaringType == typeof(CalmThemeManager);

            if (!callerIsValid)
                return false;
            
            if (!__instance.dontMatch && __instance.targetTheme != __instance.battleTheme)
                __instance.battleTheme.time = __instance.cleanTheme.time;

            __instance.targetTheme = __instance.bossTheme;
            return false;
        }
        
        /*
         * By default, MusicManager equalizes the playback time between the themes
         * This won't do the trick for the external tracks, we have to use
         * AudioClip#timeSamples instead
         */
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MusicManager), "PlayBattleMusic")]
        public static IEnumerable<CodeInstruction> MusicManager_PlayBattleMusic_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!EqualizeTimeCall(codeInstructions[i]))
                    continue;

                EqualizeTimeSamplesInstead(codeInstructions, i);
                break;
            }

            return codeInstructions;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MusicManager), "PlayCleanMusic")]
        public static bool MusicManager_PlayCleanMusic_Prefix(MusicManager __instance)
        {
            var caller = new StackTrace().GetFrame(2).GetMethod();
            if (caller.DeclaringType?.DeclaringType != typeof(CalmThemeManager))
                return false;
            
            if (!__instance.dontMatch && __instance.targetTheme != __instance.cleanTheme)
                __instance.cleanTheme.time = __instance.bossTheme.time;
            if (__instance.battleTheme.volume == (double) __instance.volume)
                __instance.cleanTheme.time = __instance.bossTheme.time;

            var themesAreIdentical = __instance.cleanTheme.clip == __instance.battleTheme.clip;
            var playingClean = __instance.targetTheme == __instance.cleanTheme;

            __instance.targetTheme = themesAreIdentical switch
            {
                true when playingClean => __instance.bossTheme,
                false when !playingClean => __instance.cleanTheme,
                _ => __instance.targetTheme
            };
            return false;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MusicManager), "PlayCleanMusic")]
        public static IEnumerable<CodeInstruction> MusicManager_PlayCleanMusic_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!EqualizeTimeCall(codeInstructions[i]))
                    continue;

                EqualizeTimeSamplesInstead(codeInstructions, i);
                break;
            }

            return codeInstructions;
        }

        private static void EqualizeTimeSamplesInstead(List<CodeInstruction> codeInstructions, int replaceAt)
        {
            codeInstructions.RemoveRange(replaceAt, 2);
            codeInstructions.InsertRange(replaceAt, IL(
                (Callvirt, Method(typeof(AudioSource), "get_timeSamples")),
                (Callvirt, Method(typeof(AudioSource), "set_timeSamples", new[] { typeof(int) }))));
        }
        
        private static bool EqualizeTimeCall(CodeInstruction instruction)
        {
            return instruction.opcode == Callvirt
                   && instruction.OperandIs(Method(typeof(AudioSource), "get_time"));
        }
    }
}