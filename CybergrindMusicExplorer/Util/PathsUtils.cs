using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CybergrindMusicExplorer.Util
{
    public static class PathsUtils
    {
        public static readonly string UltrakillPath =
            Directory.GetParent(Application.dataPath)?.FullName ?? FallbackUltrakillPath;
        private const string FallbackUltrakillPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ULTRAKILL";

        public static readonly string CgmePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string CgmeJsonPath = Path.Combine(UltrakillPath, "Preferences", "CgmePreferences.json");
        public static readonly string CustomSongsPath = Path.Combine(UltrakillPath, "CyberGrind", "Music");
        
        public static readonly DirectoryInfo SpecialEffectsDirectory = new DirectoryInfo(Path.Combine(CustomSongsPath, "CGME"));
        public static readonly DirectoryInfo TemporaryFilesDirectory = new DirectoryInfo(Path.Combine(CustomSongsPath, "CGME", "Temp"));
        
        public static string CoerceValidFileName(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = $@"[{invalidChars}]+";

            var reservedWords = new []
            {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");

            return reservedWords
                .Select(reservedWord => $"^{reservedWord}\\.")
                .Aggregate(sanitisedNamePart, (current, reservedWordPattern) =>
                    Regex.Replace(current, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase));
        }
        
        public static string RelativePathToDirectory(IDirectoryTree<FileInfo> directory)
        {
            var result = string.Empty;
            var currentDir = directory;
            while (currentDir.parent != null)
            {
                result = currentDir.name + "\\" + result;
                currentDir = currentDir.parent;
            }

            return result;
        }
    }
}