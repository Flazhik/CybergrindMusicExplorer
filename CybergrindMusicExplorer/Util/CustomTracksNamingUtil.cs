using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static System.IO.Path;

namespace CybergrindMusicExplorer.Util
{
    public static class CustomTracksNamingUtil
    {
        public static readonly Dictionary<string, AudioType> AudioTypesByExtension = new Dictionary<string, AudioType>
        {
            { ".mp3", AudioType.MPEG },
            { ".wav", AudioType.WAV },
            { ".ogg", AudioType.OGGVORBIS }
        };
        
        private static readonly List<string> IntroLoopPostfixes = new List<string>
        {
            "intro",
            "loop"
        };

        private static readonly List<string> SpecialPostfixes = new List<string>
        {
            "intro",
            "loop",
            "calm",
            "calmloop",
            "calmintro"
        };
        
        public static FileInfo FileWithAnotherExtension(FileInfo path, string extension)
        {
            return new FileInfo(
                $"{GetDirectoryName(path.FullName)}\\{GetFileNameWithoutExtension(path.FullName)}.{extension}");
        }

        public static FileInfo WithPostfix(FileInfo path, string postfix)
        {
            return new FileInfo(
                $"{GetDirectoryName(path.FullName)}\\{GetFileNameWithoutExtension(path.FullName)}_{postfix}{path.Extension}");
        }

        public static FileInfo WithoutPostfix(FileInfo path)
        {
            return !HasSpecialPostfix(path)
                ? path
                : new FileInfo(Regex.Replace(path.FullName, "_[a-zA-Z]+\\.[a-zA-Z0-9]+$", path.Extension));
        }

        public static bool HasSpecialPostfix(FileInfo path) =>
            SpecialPostfixes.Any(postfix => HasSpecialPostfix(path, postfix));
        
        
        public static bool IntroOrLoopPart(FileInfo path) =>
            IntroLoopPostfixes.Any(postfix => HasSpecialPostfix(path, postfix));
        
        
        public static bool HasIntroAndLoop(IGrouping<string, FileInfo> tracks) =>
            IntroLoopPostfixes.All(postfix => tracks.Any(track => HasSpecialPostfix(track, postfix)));
        

        public static bool HasSpecialPostfix(FileInfo path, string postfix) 
            => GetFileNameWithoutExtension(path.Name).EndsWith($"_{postfix}");
    }
}