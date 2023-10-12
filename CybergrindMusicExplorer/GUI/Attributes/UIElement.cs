using System;

namespace CybergrindMusicExplorer.GUI
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