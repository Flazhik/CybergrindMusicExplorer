using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(MusicChanger))]
    public class MusicChangerPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MusicChanger), "Change")]
        public static IEnumerable<CodeInstruction> MusicChanger_Change_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!DestroyInstance(codeInstructions[i]))
                    continue;

                codeInstructions.RemoveRange(i - 1, 2);
                break;
            }

            return codeInstructions;
        }
        
        private static bool DestroyInstance(CodeInstruction instruction) =>
            instruction.opcode == Call && instruction.OperandIs(
                Method(typeof(Object), "Destroy", new[] { typeof(Object) }));
    }
}