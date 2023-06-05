using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CybergrindMusicExplorer.Util
{
    public class CustomTrackUtil
    {

        public static IEnumerator LoadCustomSong(string path, AudioType audioType, Action<AudioClip> callback)
        {
            AudioClip audioClip = null;
            using (var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path), audioType))
            {
                yield return request.SendWebRequest();
                
                if (request.error != null)
                {
                    Debug.LogError(request.error);
                    yield break;
                }

                try
                {
                    audioClip = DownloadHandlerAudioClip.GetContent(request);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot parse audioclip {path}");
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

        // Custom songs don't really have any intro and only consist of one clip...
        // ...duh
        public static Playlist.SongData SongDataFromCustomAudioClip(AudioClip audioClip, string title, string artist,
            Sprite icon)
        {
            var alt = artist ?? "Custom song";
            return new Playlist.SongData(title + $" <color=grey>{alt}</color>", icon, null,
                new List<AudioClip> { audioClip }, 1);
        }
    }
}