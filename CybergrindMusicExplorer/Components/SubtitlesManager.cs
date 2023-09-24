using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SubtitlesParser.Classes;
using SubtitlesParser.Classes.Parsers;
using UnityEngine;

using static CybergrindMusicExplorer.Util.CustomTracksNamingUtil;

namespace CybergrindMusicExplorer.Components
{
    public static class SubtitlesManager
    {
        private static readonly Dictionary<string, ISubtitlesParser> Parsers = new Dictionary<string, ISubtitlesParser>
        {
            { "srt", new SrtParser() },
            { "vtt", new VttParser() }
        };
        
        public static List<SubtitleItem> GetSubtitlesFor(FileInfo track)
        {
            if (!Parsers.Any(p => FileWithAnotherExtension(track, p.Key).Exists))
                return new List<SubtitleItem>();
            
            Debug.Log($"[CybergrindMusicExplorer] Found subtitles for {track.Name}");

            var parser = Parsers.First(p => FileWithAnotherExtension(track, p.Key).Exists);
            using var fileStream = File.OpenRead(FileWithAnotherExtension(track, parser.Key).FullName);
            
            return parser.Value.ParseStream(fileStream, Encoding.Default);
        }
    }
}