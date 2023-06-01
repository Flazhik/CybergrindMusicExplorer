using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CybergrindMusicExplorer.Util
{
    public class CustomTrackUtil
    {
        public static UnityWebRequestAsyncOperation LoadCustomSong(string path)
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip(new Uri(path), AudioType.MPEG))
            {
                return uwr.SendWebRequest();
            }
        }

        public static IEnumerator LoadCustomSong(string path, Action<AudioClip> callback)
        {
            AudioClip audioClip;
            using (var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path), AudioType.MPEG))
            {
                // ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;
                yield return request.SendWebRequest();

                audioClip = DownloadHandlerAudioClip.GetContent(request);
                while (!audioClip || !audioClip.isReadyToPlay)
                    yield return new WaitForSeconds(1);

                if (audioClip == null)
                {
                    Debug.LogError($"Downloaded AudioClip is null, path=[{path}].");
                    yield break;
                }

                if (request.error != null)
                {
                    Debug.LogError("Unexpected error, can't download AudioClip path=[{path}].");
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