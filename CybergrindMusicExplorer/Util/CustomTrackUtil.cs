using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace CybergrindMusicExplorer.Util
{
    public static class CustomTrackUtil
    {
        public static readonly Dictionary<string, AudioType> AudioTypesByExtension = new Dictionary<string, AudioType>
        {
            { ".mp3", AudioType.MPEG },
            { ".wav", AudioType.WAV },
            { ".ogg", AudioType.OGGVORBIS }
        };

        // Alas, normalization has to go for now
        public static void Normalize(AudioClip audioClip)
        {
            /*float[] samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            var max = samples.Select(Math.Abs).Prepend(float.MinValue).Max();
            for (var i = 0; i < samples.Length; i++) samples[i] /= max;

            audioClip.SetData(samples, 0);*/
        }

        public static IEnumerator LoadCustomSong(string path, AudioType audioType, Action<AudioClip> callback)
        {
            AudioClip audioClip = null;
            using (var request = UnityWebRequestMultimedia.GetAudioClip("file:///"  + UnityWebRequest.EscapeURL(path), audioType))
            {
                ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;
                yield return request.SendWebRequest();

                while (request.downloadedBytes < 1024)
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