using System;

namespace CybergrindMusicExplorer.GUI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UIElement : Attribute
    {
        public string Path { get; }

        public UIElement(string path = "")
        {
            Path = path;
        }
    }
}