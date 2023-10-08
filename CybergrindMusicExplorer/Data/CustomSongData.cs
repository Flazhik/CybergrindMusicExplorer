using System.Collections.Generic;
using UnityEngine;

namespace CybergrindMusicExplorer.Data
{
    public class CustomSongData : Playlist.SongData
    {
        public AudioClip CalmLoopClip;
        public AudioClip CalmIntroClip;
        public List<UnlockCondition> conditions;
        public string extraLevelBit;

        public CustomSongData(
            string name,
            Sprite icon,
            AudioClip introClip,
            List<AudioClip> clips,
            int maxClips,
            AudioClip calmLoopClip,
            AudioClip calmIntroClip) : base(name, icon, introClip, clips, maxClips)
        {
            CalmLoopClip = calmLoopClip;
            CalmIntroClip = calmIntroClip;
        }

        public static CustomSongData FromSoundtrack(SoundtrackSong song, AudioClip calmLoopTheme, AudioClip calmIntroClip)
        {
            var data = new CustomSongData(
                song.songName + $" <color=grey>{song.extraLevelBit}</color>",
                song.icon,
                song.introClip,
                song.clips,
                song.maxClipsIfNotRepeating,
                calmLoopTheme,
                calmIntroClip
                )
            {
                conditions = song.conditions,
                extraLevelBit = song.extraLevelBit
            };

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
            var alt = artist ?? "Custom song";
            return new CustomSongData(
                title + $" <color=grey>{alt}</color>",
                icon,
                intro,
                new List<AudioClip> { loop }, 
                1,
                calmLoop,
                calmIntro);
        }
    }
}