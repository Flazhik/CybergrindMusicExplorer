using System.Collections.Generic;
using UnityEngine;

namespace CybergrindMusicExplorer.Data
{
    public class CustomSongData : SoundtrackSong
    {
        public AudioClip calmLoopClip;
        public AudioClip calmIntroClip;

        public static CustomSongData FromSoundtrack(SoundtrackSong song, AudioClip calmLoopTheme, AudioClip calmIntroClip)
        {
            var data = CreateInstance<CustomSongData>();
            data.icon = song.icon;
            data.songName = song.songName;
            data.introClip = song.introClip;
            data.clips = song.clips;
            data.maxClipsIfNotRepeating = song.maxClipsIfNotRepeating;
            data.conditions = song.conditions;
            data.levelName = song.levelName;
            data.calmIntroClip = calmIntroClip;
            data.calmLoopClip = calmLoopTheme;
            return data;
        }
        
        public static CustomSongData FromAudioClip(
            AudioClip intro, 
            AudioClip loop,
            AudioClip calmLoop,
            AudioClip calmIntro,
            string title,
            string artist,
            Sprite icon)
        {
            var data = CreateInstance<CustomSongData>();
            data.icon = icon;
            data.songName = title;
            data.introClip = intro;
            data.clips = new List<AudioClip> { loop };
            data.conditions = new List<UnlockCondition>();
            data.levelName = artist;
            data.calmIntroClip = calmIntro;
            data.calmLoopClip = calmLoop;
            return data;
        }
    }
}