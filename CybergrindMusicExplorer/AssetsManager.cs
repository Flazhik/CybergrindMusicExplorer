using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CybergrindMusicExplorer
{
    public class AssetsManager : MonoSingleton<AssetsManager>
    {
        private static Dictionary<string, Object> _prefabs = new Dictionary<string, Object>();
        private AssetBundle bundle;

        public void LoadAssets()
        {
            bundle = AssetBundle.LoadFromMemory(Resources.CybergrindMusicExplorer);
        }

        public void RegisterPrefabs()
        {
            foreach (var assetName in bundle.AllAssetNames())
            {
                Debug.LogWarning(assetName);
                _prefabs.Add(assetName, bundle.LoadAsset<Object>(assetName));
            }
        }

        public Object GetAsset(string assetName)
        {
            return _prefabs[assetName];
        }
    }
}