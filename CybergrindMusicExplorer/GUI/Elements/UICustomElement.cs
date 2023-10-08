using System;

namespace CybergrindMusicExplorer.GUI.Elements
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UICustomElement : Attribute
    {
        public string GameObjectName { get; }

        public UICustomElement(string goName = "")
        {
            GameObjectName = goName;
        }
    }
}