using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
using SubtitlesParser.Classes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static CybergrindMusicExplorer.Util.CustomTracksNamingUtil;
using File = TagLib.File;

namespace CybergrindMusicExplorer.Components
{
    public class TracksLoader: MonoSingleton<TracksLoader>
    {
        public static Sprite defaultIcon;
        private static readonly (string, string) IntoTheFire = ("9969a3a31df68144ea5ddf0a74488bf6", "28cc2290236220e46b5e2db27a72bfe7");
        private static readonly Dictionary<string, string> CleanThemesReferences = new Dictionary<string, string>
        {
            { "87d5f276f19589c46ad75c77af3e4acb", "94bd8d8f481f4514f98bfe3e7d4edbaf" },
            { "70b9c403651642f4e98002b35da8d9bf", "a39d3132dbc9c394c84675ea2f3a0805" },
            { "169f155ef48f76e45b57126ac148190a", "15d4aa87c4b281f41888a91bf6f4c616" },
            { "e75ffc7694f458949b17614a452cd7b7", "c652d3728bd50ac4ba996aadd3aff770" },
            { "0b1d5d2e38ac93d44b365aabdb38d771", "5cf41d4b37d331843ad11db5356c7f14" },
            { "4f586090f448818488cb5282dd579c2b", "8655b118944451f4e9c0d3729c19fb8d" },
            { "1ef3a2f13eaf8d04c93b99a7146df861", "4a5337b97d5bf9a4d93024b0c814fa98" },
            { "08ae612805fd6ee4a9c77d4bfe0dead5", "9c7c25b44ab9d3c4c866e004d7d7fc98" },
            { "b05b04675e0f48e4fb81b35db71458c8", "53220dd485db9ae40a8eba4ee9639bf5" },
            { "a12f24a6abd8bf34da95e359b7419a88", "980b7cd80a699734f94df836e6b82810" },
            { "f020ee9fede39a54b805bb4264be824e", "b7bf7af0ebc7fde44889fb4d15f07994" },
            { "1b448252defb67341889f019a2d7d1a6", "bebc457f0755504468f6c6a1b338dacf" },
            { "1b5ee7ae7309522489b5d72a65d8dabb", "a832611e4b58d124ab17f996548ef889" },
            { "7b5abb0398b6ab24388e486ef8d0c917", "72c085b36bba1e64db1f89ed5382ccb4" },
            { "529bfdda1fa77064b89af39108fba2ad", "3aa6eaa2c75c61e40aec5526464133ab" },
            { "7e42e41d5c5ceda44a95405158329d6e", "7dc93ba8b2421ba41a17a46a9a5d4704" },
            { "6da404a7f7065194794357f35f6db2dd", "10214ceea6a6fde42bf44692a0c650c2" },
            { "528eb7499c8f36746aa951bb8990b769", "64f9c5192b8abf64c9739cfd1d1ab0e7" },
            { "5ee7f58d818624640ad4843106506a19", "7da78bb4aae3cf8459eeb26e6ea58b6c" },
        };
        
        private readonly Dictionary<string, CustomSongData> customTracksCache =
            new Dictionary<string, CustomSongData>();

        private readonly Dictionary<Playlist.SongIdentifier, CustomSongData> soundtrackCache = new Dictionary<Playlist.SongIdentifier, CustomSongData>();
        private readonly Dictionary<string, List<List<SubtitleItem>>> subtitlesCache =
            new Dictionary<string, List<List<SubtitleItem>>>();

        public void SetDefaultIcon(Sprite icon)
        {
            defaultIcon = icon;
        }

        public IEnumerator LoadSongData(Playlist.SongIdentifier reference, Action<CustomSongData> callback)
        {
            CustomSongData song;
            if (soundtrackCache.ContainsKey(reference))
            {
                yield return new WaitUntil(() => soundtrackCache[reference] != null);
                song = soundtrackCache[reference];
            }
            else
            {
                AudioClip cleanTheme = null;
                
                // A hack for OST calm themes
                if (CleanThemesReferences.TryGetValue(reference.path, out var clean))
                {
                    var cleanThemeHandle = new AssetReferenceT<AudioClip>(clean).LoadAssetAsync();
                    yield return new WaitUntil(() => cleanThemeHandle.IsDone);
                    cleanTheme = cleanThemeHandle.Result;
                    Addressables.Release(cleanThemeHandle);
                }

                // "Into the fire" asset is not an AudioClip
                if (reference.path.Equals(IntoTheFire.Item1))
                {
                    var cleanThemeHandle = new AssetReferenceT<GameObject>(IntoTheFire.Item2).LoadAssetAsync();
                    yield return new WaitUntil(() => cleanThemeHandle.IsDone);
                    cleanTheme = cleanThemeHandle.Result.transform.Find("MusicManager/CleanTheme")
                        .GetComponent<AudioSource>().clip;
                    Addressables.Release(cleanThemeHandle);
                }

                var battleThemeHandle = new AssetReferenceT<SoundtrackSong>(reference.path).LoadAssetAsync();
                soundtrackCache.Add(reference, null);
                yield return new WaitUntil(() => battleThemeHandle.IsDone);
                
                song = CustomSongData.FromSoundtrack(
                    battleThemeHandle.Result,
                    cleanTheme,
                    null);
                soundtrackCache[reference] = song;

                Addressables.Release(battleThemeHandle);
            }

            callback.Invoke(song);
        }

        public IEnumerator LoadSongData(FileInfo fileInfo, Action<CustomSongData> callback, Action<List<List<SubtitleItem>>> subtitles)
        {
            CustomTrackMetadata metadata;
            AudioClip introClip = null, loopClip = null, calmLoopClip = null, calmIntroClip = null;

            if (customTracksCache.TryGetValue(fileInfo.FullName, out var songData))
            {
                callback.Invoke(songData);
                if (subtitlesCache.TryGetValue(fileInfo.FullName, out var subs))
                    subtitles.Invoke(subs);
                
                yield break;
            }

            if (fileInfo.Exists)
            {
                try
                {
                    var file = File.Create(fileInfo.FullName);
                    metadata = CustomTrackMetadata.From(file);
                }
                catch (Exception)
                {
                    metadata = CustomTrackMetadata.EmptyMetadata();
                }

                subtitlesCache[fileInfo.FullName] = new List<List<SubtitleItem>> { LoadSubtitlesFor(fileInfo) };
                subtitles.Invoke(subtitlesCache[fileInfo.FullName]);

                yield return LoadSong(fileInfo, clip => loopClip = clip);
                Debug.Log($"Loaded custom music file={fileInfo.Name}");
            }
            else
            {
                var introClipInfo = WithPostfix(fileInfo, "intro");
                var loopClipInfo = WithPostfix(fileInfo, "loop");

                if (!introClipInfo.Exists || !loopClipInfo.Exists)
                    yield break;

                try
                {
                    metadata = CustomTrackMetadata.From(File.Create(introClipInfo.FullName));
                } catch (Exception)
                {
                    metadata = CustomTrackMetadata.EmptyMetadata();
                }

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
                    $"Loaded custom music file={fileInfo.Name} with intro and loop components");
            }
            
            var calmLoopClipInfo = WithPostfix(fileInfo, "calmloop");
            var calmClipInfo = WithPostfix(fileInfo, "calm");
            
            var firstCalmClip = new List<FileInfo> { calmLoopClipInfo, calmClipInfo }.FirstOrDefault(file => file.Exists);
            
            if (firstCalmClip != default)
                yield return LoadSong(firstCalmClip, clip => calmLoopClip = clip);
            
            var calmIntroClipInfo = WithPostfix(fileInfo, "calmintro");
            if (calmIntroClipInfo.Exists)
                yield return LoadSong(calmIntroClipInfo, clip => calmIntroClip = clip);
            
            try
            {
                if (loopClip == null)
                    yield break;

                var song = CustomSongData.FromAudioClip(
                    introClip,
                    loopClip,
                    calmLoopClip,
                    calmIntroClip,
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
                    $"Can't retrieve custom track {fileInfo.Name} metadata");
            }
        }

        private static List<SubtitleItem> LoadSubtitlesFor(FileInfo clipFile)
        {
            return SubtitlesManager.GetSubtitlesFor(clipFile);
        }

        private static IEnumerator LoadSong(FileSystemInfo fileInfo, Action<AudioClip> callback)
        {
            yield return LoadCustomSong(fileInfo.FullName, AudioTypesByExtension[fileInfo.Extension.ToLower()], true, callback);
        }

        public static IEnumerator LoadCustomSong(string path, AudioType audioType, bool async, Action<AudioClip> callback)
        {
            using var request =
                UnityWebRequestMultimedia.GetAudioClip("file:///" + UnityWebRequest.EscapeURL(path), audioType);
            ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = async;
            request.SendWebRequest();
                
            while (!request.isDone || request.error != null)
                yield return null;
                
            if (request.error != null)
            {
                Debug.LogError(request.error);
                yield break;
            }

            AudioClip audioClip;
            try
            {
                audioClip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                    
                if (audioClip.channels != 0)
                    callback(audioClip);
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
                Debug.LogError("Unexpected error, can't download AudioClip path=[{path}].");
            
        }
    }
}