using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SubtitlesParser.Classes;
using UnityEngine;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CustomPlaylist : Playlist
    {
        public CustomPlaylist()
        {
            OnChanged += Changed;
        }

        public event Action Changed;

        [JsonProperty("_references")] public List<TrackReference> References = new List<TrackReference>();

        [JsonProperty("_selected")]
        protected int _selected
        {
            get => (int)GetPrivate(this, typeof(Playlist), "_selected");
            set => SetPrivate(this, typeof(Playlist), "_selected", value);
        }

        [JsonProperty("_shuffled")]
        private bool Shuffled
        {
            get => (bool)GetPrivate(this, typeof(Playlist), "_shuffled");
            set => SetPrivate(this, typeof(Playlist), "_shuffled", value);
        }

        [JsonProperty("_loopMode")]
        private LoopMode LoopMode
        {
            get => (LoopMode)GetPrivate(this, typeof(Playlist), "_loopMode");
            set => SetPrivate(this, typeof(Playlist), "_loopMode", value);
        }

        protected Dictionary<TrackReference, SongData> LoadedSongDict = new Dictionary<TrackReference, SongData>();
        
        protected Dictionary<TrackReference, List<List<SubtitleItem>>> SubtitlesDict = new Dictionary<TrackReference, List<List<SubtitleItem>>>();

        public new int Count => References.Count;

        public bool IsSongLoaded(TrackReference reference) => LoadedSongDict.ContainsKey(reference);

        public bool GetSongData(TrackReference reference, out SongData data) =>
            LoadedSongDict.TryGetValue(reference, out data);

        public bool GetSubtitles(TrackReference reference, out List<List<SubtitleItem>> subtitles) =>
            SubtitlesDict.TryGetValue(reference, out subtitles);
        
        public void AddTrack(TrackReference reference, SoundtrackSong song)
        {
            AddTrack(reference,
                new SongData(song.songName + " <color=grey>" + song.extraLevelBit + "</color>", song.icon,
                    song.introClip, song.clips, song.maxClipsIfNotRepeating));
        }
        
        public void AddTrack(TrackReference reference, SongData song)
        {
            if (song == null)
            {
                Debug.LogWarning("Attempted to add null song to playlist, reference '" + reference.Reference +
                                 $"', type {reference.Type}. Ignoring...");
            }
            else
            {
                References.Add(reference);
                if (!LoadedSongDict.ContainsKey(reference))
                    LoadedSongDict.Add(reference, song);

                if (Changed == null)
                    return;
                Changed();
            }
        }

        public void AddSubtitles(TrackReference reference, List<List<SubtitleItem>> subtitles)
        {
            if (!SubtitlesDict.ContainsKey(reference))
                SubtitlesDict.Add(reference, subtitles);
        }

        public new void Swap(int index1, int index2)
        {
            (References[index1], References[index2]) = (References[index2], References[index1]);
            Changed?.Invoke();
        }

        public new void Remove(int index)
        {
            if (index >= 0 && index < References.Count)
            {
                if (References.Count > 1)
                {
                    References.RemoveAt(index);
                    if (Changed == null)
                        return;
                    Changed();
                }
                else
                    Debug.LogWarning("Attempted to remove last song from playlist!");
            }
            else
                Debug.LogWarning($"Attempted to remove index '{index}' from playlist, which is out of bounds.");
        }

        public void Remove(TrackReference reference)
        {
            if (References.Count > 1)
            {
                References.Remove(reference);
                if (Changed == null)
                    return;
                Changed();
            }
            else
                Debug.LogWarning("Attempted to remove last song from playlist!");
        }
    }
}