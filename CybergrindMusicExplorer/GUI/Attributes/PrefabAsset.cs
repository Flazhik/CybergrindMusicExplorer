using System;

namespace CybergrindMusicExplorer.GUI.Attributes
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