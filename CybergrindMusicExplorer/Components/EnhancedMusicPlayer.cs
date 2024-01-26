using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
using CybergrindMusicExplorer.GUI;
using CybergrindMusicExplorer.Util;
using SubtitlesParser.Classes;
using UnityEngine;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.Components
{
    public class EnhancedMusicPlayer : MonoBehaviour
    {
        private const int NextTrackTimeout = 100;
        
        private CanvasGroup panelGroup;
        private Text panelText;
        private Image panelIcon;
        private CustomMusicPlaylistEditor playlistEditor;
        private Sprite defaultIcon;
        public float panelApproachTime;
        public float panelStayTime;
        private PlaybackWindow playback;
        private TracksLoader tracksLoader;
        private readonly MusicChanger changer = FindObjectOfType<MusicChanger>();
        private readonly MusicManager musicManager = MusicManager.Instance;
        private readonly CybergrindMusicExplorerManager manager = CybergrindMusicExplorerManager.Instance;
        private readonly OptionsManager optionsManager = OptionsManager.Instance;

        private bool stopped;
        private bool nextTrack;
        private int currentTrackPosition;
        private int timeout;

        private void Awake()
        {
            tracksLoader = new TracksLoader(defaultIcon);
            CloneInstance(FindObjectOfType<CustomMusicPlayer>(), this,
                fieldsToIgnore: new List<string> { "playlistEditor" });
            playlistEditor = CybergrindMusicExplorer.GetPlaylistEditor();
            playback = GUIManager.GUIDeployer.playbackWindow.AddComponent<PlaybackWindow>();
            playback.OnSongSelected += ChangeSongIndex;
            changer.oneTime = true;
            changer.dontStart = false;
            changer.forceOn = true;
            Destroy(FindObjectOfType<CustomMusicPlayer>());
        }

        private void Update()
        {
            if (timeout != 0)
                timeout = (int)Mathf.MoveTowards(timeout, 0, 1);
            if (Input.GetKeyDown((KeyCode)manager.NextTrackBinding)) nextTrack = true;
        }

        public void OnEnable() => StartPlaylist();

        public void StartPlaylist()
        {
            if (playlistEditor.playlist.Count < 1)
                Debug.LogError("No songs in playlist, somehow. Not starting playlist routine...");
            else
                StartCoroutine(PlaylistRoutine());
        }

        public void StopPlaylist() => stopped = true;

        private IEnumerator ShowPanelRoutine(Playlist.SongMetadata song)
        {
            panelText.text = song.displayName.ToUpper();
            panelIcon.sprite = song.icon != null ? song.icon : defaultIcon;
            var time = 0.0f;
            while (time < (double)panelApproachTime)
            {
                time += Time.unscaledDeltaTime;
                panelGroup.alpha = time / panelApproachTime;
                yield return null;
            }

            panelGroup.alpha = 1f;
            if (manager.ShowCurrentTrackPanelIndefinitely)
                yield return new WaitUntil(() => nextTrack);
            else
                yield return new WaitForSecondsRealtime(panelStayTime);
            time = panelApproachTime;
            while (time > 0.0)
            {
                time -= Time.unscaledDeltaTime;
                panelGroup.alpha = time / panelApproachTime;
                yield return null;
            }

            panelGroup.alpha = 0.0f;
        }

        private IEnumerator PlaylistRoutine()
        {
            IEnumerator currentSubtitlesRoutine = default;
            var songFinished = new WaitUntil(() =>
                TrackHasReachedTheEnd(musicManager.targetTheme) && Application.isFocused && !stopped || nextTrack);

            var lastSong = (Playlist.SongIdentifier)null;
            var first = true;
            var playlist = playlistEditor.playlist;
            var currentOrder = playlist.shuffled
                ? new DeckShuffled<Playlist.SongIdentifier>(playlist.ids).AsEnumerable()
                : playlist.ids.AsEnumerable();
            if (playlist.loopMode == Playlist.LoopMode.LoopOne)
                currentOrder = currentOrder.Skip(playlist.selected).Take(1).ToList();

            while (!stopped)
            {
                if (currentOrder is DeckShuffled<Playlist.SongIdentifier> deckShuffled) deckShuffled.Reshuffle();
                var orderAsList = currentOrder.ToList();
                playback.ChangeOrder(orderAsList);
                for (currentTrackPosition = 0; currentTrackPosition < orderAsList.Count; currentTrackPosition++)
                {
                    var id = orderAsList[currentTrackPosition];
                    if (nextTrack)
                        nextTrack = false;
                    playback.ChangeCurrent(currentTrackPosition);
                    if (currentSubtitlesRoutine != default)
                        StopCoroutine(currentSubtitlesRoutine);
                    
                    if (currentSubtitlesRoutine != default)
                        StopCoroutine(currentSubtitlesRoutine);

                    // Only allow boosting for custom tracks
                    manager.allowMusicBoost = id.type == Playlist.SongIdentifier.IdentifierType.File;

                    var songMetadata = playlistEditor.GetSongMetadata(id);
                    if (id != lastSong)
                        StartCoroutine(ShowPanelRoutine(songMetadata));

                    CustomSongData songData = null;
                    List<List<SubtitleItem>> subs = null;

                    if (id.type == Playlist.SongIdentifier.IdentifierType.Addressable)
                        yield return tracksLoader.LoadSongData(id, s => songData = s);
                    else
                    {
                        var fileInfo = new FileInfo(id.path);
                        yield return tracksLoader.LoadSongData(fileInfo, s => songData = s, s => subs = s);
                    }

                    if (songData == null)
                        continue;
                    
                    if (first && songData.introClip != null)
                    {
                        currentSubtitlesRoutine = SubtitlesRoutine(subs, 0);
                        StartCoroutine(currentSubtitlesRoutine);
                        ChangeMusic(songData.introClip, songData.calmIntroClip);

                        yield return WaitForTrackToPlay();
                        yield return songFinished;
                    }

                    var clipsPlayed = 0;
                    for (var j = 0; j < songData.clips.Count; j++)
                    {
                        var clip = songData.clips[j];
                        currentSubtitlesRoutine = SubtitlesRoutine(subs, songData.introClip == null ? j : j + 1);
                        StartCoroutine(currentSubtitlesRoutine);
                        ChangeMusic(clip, songData.calmLoopClip);

                        yield return WaitForTrackToPlay();
                        yield return songFinished;

                        ++clipsPlayed;
                        if (playlist.loopMode != Playlist.LoopMode.LoopOne && songData.maxClipsIfNotRepeating > 0 &&
                            clipsPlayed >= songData.maxClipsIfNotRepeating)
                            break;
                    }

                    first = false;
                }
            }
        }

        private void ChangeMusic(AudioClip battleTheme, AudioClip calmTheme)
        {
            if (calmTheme != null)
            {
                changer.clean = calmTheme;
                changer.battle = battleTheme;
                changer.boss = battleTheme;
                changer.Change();
            }
            else
                changer.ChangeTo(battleTheme);
        }

        private static bool TrackHasReachedTheEnd(AudioSource source) =>
            source != null && source.time == 0.0f && !source.isPlaying;

        private void ChangeSongIndex(int position)
        {
            currentTrackPosition = position;
            nextTrack = true;
        }

        private IEnumerator SubtitlesRoutine(List<List<SubtitleItem>> subs, int clip)
        {
            if (subs == null)
                yield break;
            if (subs.Count <= clip || subs[clip] == default)
                yield break;

            var subtitles = subs[clip];

            for (var i = 0; i < subtitles.Count; i++)
            {
                var waitFor = (float)(i == 0
                    ? subtitles[i].StartTime
                    : subtitles[i].StartTime - subtitles[i - 1].StartTime) / 1000;

                while (waitFor > 0.0)
                {
                    if (!Application.isFocused)
                        yield return null;

                    waitFor -= optionsManager.paused
                        ? Time.unscaledDeltaTime
                        : Time.deltaTime;

                    yield return null;
                }

                if (!optionsManager.paused)
                    MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(string.Join(" ",
                        subtitles[i].Lines.ToArray()));
            }
        }

        private IEnumerator WaitForTrackToPlay()
        {
            timeout = NextTrackTimeout;
            
            return new WaitUntil(
                () => musicManager.targetTheme.isPlaying && musicManager.targetTheme.time != 0.0f || nextTrack || timeout == 0);
        }
    }
}