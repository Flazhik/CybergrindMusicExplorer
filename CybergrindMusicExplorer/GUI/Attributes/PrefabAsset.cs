using System;

namespace CybergrindMusicExplorer.GUI
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PrefabAsset : Attribute
    {
        public string Path { get; }

        public PrefabAsset(string path = "")
        {
            Path = path;
        }
    }
}