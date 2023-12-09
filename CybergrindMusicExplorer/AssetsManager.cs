using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CybergrindMusicExplorer.GUI.Attributes;
using UnityEngine;

namespace CybergrindMusicExplorer
{
    public class AssetsManager : MonoSingleton<AssetsManager>
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static Dictionary<string, Object> _prefabs = new Dictionary<string, Object>();
        private AssetBundle bundle;

        public void LoadAssets()
        {
            bundle = AssetBundle.LoadFromMemory(Resources.CybergrindMusicExplorer);
        }

        public void RegisterPrefabs()
        {
            foreach (var assetName in bundle.AllAssetNames())
                _prefabs.Add(assetName, bundle.LoadAsset<Object>(assetName));

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                CheckType(type);
        }
        
        private static void CheckType(IReflect type)
        {
            type.GetFields(Flags)
                .ToList()
                .ForEach(ProcessField);
        }

        private static void ProcessField(FieldInfo field)
        {
            if (field.FieldType.IsArray
                || !field.FieldType.IsAssignableFrom(typeof(GameObject))
                || !field.IsStatic)
                return;

            var assetTag = field.GetCustomAttribute<PrefabAsset>();
            if (assetTag == null)
                return;

            field.SetValue(null, _prefabs[assetTag.Path]);
        }
        
        public static Object GetAsset(string assetName)
        {
            return _prefabs[assetName];
        }
    }
}