using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using CybergrindMusicExplorer.Components;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(MusicManager))]
    public class MusicManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MusicManager), "OnEnable")]
        public static bool MusicManager_OnEnable_Prefix(MusicManager __instance)
        { 
            __instance.bossTheme = __instance.battleTheme;
            return true;
        }
        
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
        public static bool MusicManager_PlayBattleMusic_Prefix(MusicChanger __instance)
        {
            var caller = new StackTrace().GetFrame(2).GetMethod();
            return caller.DeclaringType?.DeclaringType == typeof(CalmThemeManager);
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
        public static bool MusicManager_PlayCleanMusic_Prefix(MusicChanger __instance)
        {
            var caller = new StackTrace().GetFrame(2).GetMethod();
            return caller.DeclaringType?.DeclaringType == typeof(CalmThemeManager);
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
        
        private static IEnumerable<CodeInstruction> IL(params (OpCode, object)[] instructions) =>
            instructions.Select(i => new CodeInstruction(i.Item1, i.Item2)).ToList();
    }
}