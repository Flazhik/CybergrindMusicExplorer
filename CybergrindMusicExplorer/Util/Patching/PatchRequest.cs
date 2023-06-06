using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CybergrindMusicExplorer.Util.Patching
{
    public class PatchRequest
    {
        private const BindingFlags TargetMethodBindingAttributes = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags HarmonyPatchesAttributes = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        private readonly MethodInfo targetMethod;
        private MethodInfo prefix;
        private MethodInfo postfix;
        private Harmony harmony;

        private PatchRequest(IReflect targetType, string methodName)
        {
            targetMethod = targetType.GetMethod(methodName, TargetMethodBindingAttributes);
        }
        
        public static PatchRequest PatchMethod(Type targetType, string methodName)
        {
            return new PatchRequest(targetType, methodName);
        }
        
        public PatchRequest WithPrefix(Type patcherType, string prefixName)
        {
            prefix = patcherType.GetMethod(prefixName, HarmonyPatchesAttributes);
            return this;
        }
        
        public PatchRequest WithPostfix(Type patcherType, string postfixName)
        {
            postfix = patcherType.GetMethod(postfixName, HarmonyPatchesAttributes);
            return this;
        }
        
        public PatchRequest Using(Harmony harmonyInstance)
        {
            harmony = harmonyInstance;
            return this;
        }
        
        public void Once()
        {
            if (harmony == null)
                throw new ArgumentNullException(nameof(harmony));
            
            if (!harmony.GetPatchedMethods().Contains(targetMethod))
                harmony.Patch(
                    targetMethod,
                    prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                    postfix: postfix != null ? new HarmonyMethod(postfix) : null);
        }
    }
}