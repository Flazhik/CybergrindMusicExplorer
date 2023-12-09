using System;

namespace CybergrindMusicExplorer.GUI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CustomComponent : Attribute
    {
        public Type ComponentType { get; }

        public CustomComponent(Type type)
        {
            ComponentType = type;
        }
    }
}