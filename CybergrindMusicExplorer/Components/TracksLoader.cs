using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CybergrindMusicExplorer.Data;
using SubtitlesParser.Classes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static CybergrindMusicExplorer.Util.CustomTracksNamingUtil;
using File = TagLib.File;
using SongData = Playlist.SongData;

namespace CybergrindMusicExplorer.Components
{
    public class TracksLoader
    {
        private readonly Dictionary<string, SongData> customTracksCache =
            new Dictionary<string, SongData>();

        private readonly Dictionary<string, SoundtrackSong> soundtrackCache = new Dictionary<string, SoundtrackSong>();
        private readonly Dictionary<string, List<List<SubtitleItem>>> subtitlesCache =
            new Dictionary<string, List<List<SubtitleItem>>>();
        private readonly Sprite defaultIcon;

        public TracksLoader(Sprite defaultIcon)
        {
            this.defaultIcon = defaultIcon;
        }

        public IEnumerator LoadSongData(TrackReference reference, Action<SoundtrackSong> callback)
        {
            SoundtrackSong song;
            if (soundtrackCache.ContainsKey(reference.Reference))
            {
                yield return new WaitUntil(() => soundtrackCache[reference.Reference] != null);
                song = soundtrackCache[reference.Reference];
            }
            else
            {
                var handle = new AssetReferenceT<SoundtrackSong>(reference.Reference).LoadAssetAsync();
                soundtrackCache.Add(reference.Reference, null);
                yield return new WaitUntil(() => handle.IsDone);

                song = handle.Result;
                soundtrackCache[reference.Reference] = song;
                Addressables.Release(handle);
            }

            callback.Invoke(song);
        }

        public IEnumerator LoadSongData(FileInfo fileInfo, Action<SongData> callback, Action<List<List<SubtitleItem>>> subtitles)
        {
            CustomTrackMetadata metadata;
            AudioClip introClip = null, loopClip = null;

            if (customTracksCache.TryGetValue(fileInfo.FullName, out var songData))
            {
                callback.Invoke(songData);
                if (subtitlesCache.TryGetValue(fileInfo.FullName, out var subs))
                    subtitles.Invoke(subs);
                
                yield break;
            }

            if (fileInfo.Exists)
            {
                var file = File.Create(fileInfo.FullName);
                metadata = CustomTrackMetadata.From(file);
                
                subtitlesCache[fileInfo.FullName] = new List<List<SubtitleItem>> { LoadSubtitlesFor(fileInfo) };
                subtitles.Invoke(subtitlesCache[fileInfo.FullName]);

                yield return LoadSong(fileInfo, clip => loopClip = clip);
                Debug.Log($"[CybergrindMusicExplorer] Loaded custom music file={fileInfo.Name}");
            }
            else
            {
                var introClipInfo = WithPostfix(fileInfo, "intro");
                var loopClipInfo = WithPostfix(fileInfo, "loop");

                if (!introClipInfo.Exists || !loopClipInfo.Exists)
                    yield break;

                metadata = CustomTrackMetadata.From(File.Create(introClipInfo.FullName));

                var introSubs = LoadSubtitlesFor(introClipInfo);
                var loopSubs = LoadSubtitlesFor(loopClipInfo);

                subtitlesCache[introClipInfo.FullName] = new List<List<SubtitleItem>>
                {
                    introSubs, loopSubs
                };
                
                subtitles.Invoke(subtitlesCache[introClipInfo.FullName]);

                yield return LoadSong(introClipInfo, clip => introClip = clip);
                yield return LoadSong(loopClipInfo, clip => loopClip = clip);

                Debug.Log(
                    $"[CybergrindMusicExplorer] Loaded custom music file={fileInfo.Name} with intro and loop components");
            }

            try
            {
                if (loopClip == null)
                    yield break;

                var song = SongDataFromCustomAudioClip(
                    introClip,
                    loopClip,
                    metadata.Title ?? Path.GetFileNameWithoutExtension(fileInfo.Name),
                    metadata.Artist,
                    metadata.Logo
                        ? metadata.Logo
                        : defaultIcon);

                customTracksCache[fileInfo.FullName] = song;
                callback.Invoke(song);
            }
            catch (Exception)
            {
                Debug.Log(
                    $"[CybergrindMusicExplorer] Can't retrieve custom track {fileInfo.Name} metadata");
            }
        }

        private static List<SubtitleItem> LoadSubtitlesFor(FileInfo clipFile)
        {
            return SubtitlesManager.GetSubtitlesFor(clipFile);
        }

        private static SongData SongDataFromCustomAudioClip(AudioClip intro, AudioClip loop, string title,
            string artist,
            Sprite icon)
        {
            var alt = artist ?? "Custom song";
            return new SongData(
                title + $" <color=grey>{alt}</color>",
                icon,
                intro,
                new List<AudioClip> { loop }, 1);
        }

        private static IEnumerator LoadSong(FileSystemInfo fileInfo, Action<AudioClip> callback)
        {
            yield return LoadCustomSong(fileInfo.FullName, AudioTypesByExtension[fileInfo.Extension], true, callback);
        }

        public static IEnumerator LoadCustomSong(string path, AudioType audioType, bool async, Action<AudioClip> callback)
        {
            AudioClip audioClip;
            using (var request =
                   UnityWebRequestMultimedia.GetAudioClip("file:///" + UnityWebRequest.EscapeURL(path), audioType))
            {
                ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = async;
                yield return request.SendWebRequest();

                while (async ? request.downloadedBytes < 1024 : !request.isDone)
                    yield return null;

                if (request.error != null)
                {
                    Debug.LogError(request.error);
                    yield break;
                }

                try
                {
                    audioClip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                }
                catch (Exception)
                {
                    Debug.LogError($"Cannot parse AudioClip {path}");
                    yield break;
                }

                if (audioClip == null)
                {
                    Debug.LogError($"Downloaded AudioClip is null, path=[{path}].");
                    yield break;
                }

                if (request.error != null)
                {
                    Debug.LogError("Unexpected error, can't download AudioClip path=[{path}].");
                    yield break;
                }
            }

            callback(audioClip);
        }
    }
}