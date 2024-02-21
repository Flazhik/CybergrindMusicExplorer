using System.Collections.Generic;
using System.IO;
using System.Text;
using CybergrindMusicExplorer.Components;
using CybergrindMusicExplorer.Data;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using static System.IO.Path;
using static CybergrindMusicExplorer.Util.MetadataUtils;
using static CybergrindMusicExplorer.Util.CustomTracksNamingUtil;
using File = TagLib.File;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(CustomMusicPlaylistEditor))]
    public class CustomMusicPlaylistEditorPatch
    {
        private static readonly Dictionary<string, Playlist.SongMetadata> MetadataCache = new Dictionary<string, Playlist.SongMetadata>();
        private static GameObject clearButton;
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomMusicPlaylistEditor), "GetSongMetadataFromFilepath")]
        public static bool CustomMusicPlaylistEditor_GetSongMetadataFromFilepath_Prefix(
            Playlist.SongIdentifier id,
            Sprite ___defaultIcon,
            ref Playlist.SongMetadata __result)
        {
            if (MetadataCache.TryGetValue(id.path, out var cachedMetadata))
            {
                __result = cachedMetadata;
                return false;
            }

            var fileInfo = new FileInfo(id.path);
            
            // If the file file doesn't exist it could indicate that the track is segmented
            // We'll attempt to retrieve metadata from an intro part in this case
            if (!fileInfo.Exists)
                fileInfo = WithPostfix(fileInfo, "intro");

            if (!fileInfo.Exists)
            {
                __result = null;
                return false;
            }
            var tags = File.Create(fileInfo.FullName).Tag;
            var coverSprite = GetAlbumCoverSprite(tags) ?? ___defaultIcon;

            var title = new StringBuilder(tags.Title ?? GetFileNameWithoutExtension(WithoutPostfix(fileInfo).FullName));
            if (tags.FirstPerformer != null)
            {
                title.Append(" <color=#7f7f7f>");
                title.Append(tags.FirstPerformer);
                title.Append("</color>");
            }
            
            __result = new Playlist.SongMetadata(title.ToString(), coverSprite);
            MetadataCache.Add(id.path, __result);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomMusicPlaylistEditor), "LoadPlaylist")]
        public static bool CustomMusicPlaylistEditor_LoadPlaylist_Prefix(CustomMusicPlaylistEditor __instance)
        {
            Playlist loadedPlaylist;

            using (var streamReader = new StreamReader(System.IO.File.Open(Playlist.currentPath, FileMode.OpenOrCreate)))
                loadedPlaylist = JsonConvert.DeserializeObject<Playlist>(streamReader.ReadToEnd());

            if (loadedPlaylist == null)
                return true;
            
            var newPlaylist = new Playlist
            {
                selected = loadedPlaylist.selected,
                loopMode = loadedPlaylist.loopMode,
                shuffled = loadedPlaylist.shuffled
            };

            foreach (var id in loadedPlaylist.ids)
                if (id.type == Playlist.SongIdentifier.IdentifierType.Addressable || new FileInfo(id.path).Exists)
                    newPlaylist.Add(id);

            __instance.playlist = newPlaylist;
            __instance.SavePlaylist();
            return true;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomMusicPlaylistEditor), "Select")]
        public static void CustomMusicPlaylistEditor_Select_Postfix(
            int newIndex,
            bool rebuild,
            CustomMusicPlaylistEditor __instance)
        {
            if (!CybergrindMusicExplorerManager.Instance.EnableTracksPreview || !rebuild)
                return;
            
            var shopping = FistControl.Instance.shopping;
            
            if (!shopping)
                return; 

            var tracksLoader = TracksLoader.Instance;
            var newId = __instance.playlist.ids[newIndex];

            tracksLoader.StartCoroutine(newId.type == Playlist.SongIdentifier.IdentifierType.Addressable
                ? tracksLoader.LoadSongData(newId, LoadedPreviewCallback)
                : tracksLoader.LoadSongData(new FileInfo(newId.path), LoadedPreviewCallback, s => { }));
            
            void LoadedPreviewCallback(CustomSongData selectedSong)
            {
                Object.FindObjectOfType<ScreenZoneHelper>().PlayPreview(selectedSong.clips[0]);
            }
        }
    }
}