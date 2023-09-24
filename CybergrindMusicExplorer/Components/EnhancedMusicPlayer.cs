using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CybergrindMusicExplorer.Data;
using CybergrindMusicExplorer.Util;
using UnityEngine;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.Components
{
    public class EnhancedMusicPlayer : MonoBehaviour
    {
        private readonly CybergrindMusicExplorerManager manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
        private readonly OptionsManager optionsManager = MonoSingleton<OptionsManager>.Instance;

        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private Text panelText;
        [SerializeField] private Image panelIcon;
        [SerializeField] private MusicChanger changer;
        [SerializeField] private EnhancedMusicPlaylistEditor playlistEditor;
        [SerializeField] private Sprite defaultIcon;
        public float panelApproachTime;
        public float panelStayTime;

        private bool stopped;
        private bool nextTrack;

        private void Update()
        {
            if (Input.GetKeyDown((KeyCode)manager.NextTrackBinding))
                nextTrack = true;
        }

        private void Awake()
        {
            CloneObsoleteInstance(
                FindObjectOfType<CustomMusicPlayer>(),
                this,
                fieldsToIgnore: new List<string> { "playlistEditor" });

            playlistEditor = CybergrindMusicExplorer.GetEnhancedPlaylistEditor();
            Destroy(FindObjectOfType<CustomMusicPlayer>());
        }

        public void OnEnable() => StartPlaylist();

        public void StartPlaylist()
        {
            if (playlistEditor.Playlist.Count < 1)
                Debug.LogError(
                    "[CybergrindMusicExplorer] No songs in playlist, somehow. Not starting playlist routine...");
            else
                StartCoroutine(PlaylistRoutine());
        }

        public void StopPlaylist() => stopped = true;
        
        private IEnumerator SubtitlesRoutine(TrackReference reference, int clip)
        {
            if (!playlistEditor.Playlist.GetSubtitles(reference, out var subtitlesList))
                yield break;
            if (subtitlesList.Count <= clip || subtitlesList[clip] == default)
                yield break;

            var subtitles = subtitlesList[clip];
            
            for (var i = 0; i < subtitles.Count; i++)
            {
                var waitFor = (float) (i == 0
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
                    MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(string.Join(" ", subtitles[i].Lines.ToArray()));
            }
        }

        private IEnumerator ShowPanelRoutine(Playlist.SongData song)
        {
            panelText.text = song.name.ToUpper();
            panelIcon.sprite = song.icon != null ? song.icon : defaultIcon;
            var time = 0.0f;
            while (time < (double)panelApproachTime)
            {
                time += Time.deltaTime;
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
                time -= Time.deltaTime;
                panelGroup.alpha = time / panelApproachTime;
                yield return null;
            }

            panelGroup.alpha = 0.0f;
        }

        private IEnumerator PlaylistRoutine()
        {
            IEnumerator currentSubtitlesRoutine = default;
            var musicPlayer = this;
            var themeNotPlaying = new WaitUntil(() =>
                Application.isFocused && !MonoSingleton<MusicManager>.Instance.targetTheme.isPlaying && !musicPlayer.stopped || nextTrack);
            var first = true;
            var playlist = musicPlayer.playlistEditor.Playlist;

            Playlist.SongData lastSong = null;

            var order = playlist.shuffled
                ? (IEnumerable<TrackReference>)new DeckShuffled<TrackReference>(playlist.References)
                : playlist.References;

            if (playlist.loopMode == Playlist.LoopMode.LoopOne)
                order = order.Skip(playlist.selected).Take(1);
            
            while (!musicPlayer.stopped)
            {
                foreach (var reference in order)
                {
                    if (nextTrack)
                        nextTrack = false;

                    if (currentSubtitlesRoutine != default)
                        StopCoroutine(currentSubtitlesRoutine);
                    
                    musicPlayer.playlistEditor.Playlist.GetSongData(reference, out var currentSong);
                    Debug.Log($"[CybergrindMusicExplorer] Now playing {currentSong.name}");

                    // Only allow boosting for custom tracks
                    manager.allowMusicBoost = reference.Type == SoundtrackType.External;

                    if (lastSong != currentSong)
                        musicPlayer.StartCoroutine(musicPlayer.ShowPanelRoutine(currentSong));
                    if (first)
                    {
                        if (currentSong.introClip != null)
                        {
                            currentSubtitlesRoutine = SubtitlesRoutine(reference, 0);
                            StartCoroutine(currentSubtitlesRoutine);
                            
                            musicPlayer.changer.ChangeTo(currentSong.introClip);
                            yield return themeNotPlaying;
                        }

                        first = false;
                    }

                    var i = 0;
                    for (var j = 0; j < currentSong.clips.Count; j++)
                    {
                        if (musicPlayer.playlistEditor.Playlist.loopMode == Playlist.LoopMode.LoopOne ||
                            currentSong.maxClips <= -1 || i < currentSong.maxClips)
                        {
                            currentSubtitlesRoutine = SubtitlesRoutine(reference, currentSong.introClip == null ? j : j + 1);
                            StartCoroutine(currentSubtitlesRoutine);
                            musicPlayer.changer.ChangeTo(currentSong.clips[j]);
                            ++i;
                            yield return themeNotPlaying;
                        }
                        else
                            break;
                    }

                    lastSong = currentSong;
                    musicPlayer.changer.ChangeTo(null);
                    currentSong = null;
                }

                yield return null;
            }
        }
    }
}