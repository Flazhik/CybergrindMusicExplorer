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
        private CybergrindMusicExplorerManager manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;

        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private Text panelText;
        [SerializeField] private Image panelIcon;
        [SerializeField] private MusicChanger changer;
        [SerializeField] private EnhancedMusicPlaylistEditor playlistEditor;
        [SerializeField] private Sprite defaultIcon;
        public float panelApproachTime;
        public float panelStayTime;
        private bool stopped;

        private void Update()
        {
            if (manager == null)
                manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
            if (Input.GetKeyDown((KeyCode)manager.NextTrackBinding))
                MonoSingleton<MusicManager>.Instance.targetTheme.Stop();
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
            if (playlistEditor.customPlaylist.Count < 1)
                Debug.LogError(
                    "[CybergrindMusicExplorer] No songs in playlist, somehow. Not starting playlist routine...");
            else
                StartCoroutine(PlaylistRoutine());
        }

        public void StopPlaylist() => stopped = true;

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
                yield return new WaitUntil(() => !MonoSingleton<MusicManager>.Instance.targetTheme.isPlaying);
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
            var musicPlayer = this;
            var themeNotPlaying = new WaitUntil(() =>
                Application.isFocused && !MonoSingleton<MusicManager>.Instance.targetTheme.isPlaying &&
                !musicPlayer.stopped);
            Playlist.SongData lastSong = null;
            var first = true;
            var playlist = musicPlayer.playlistEditor.customPlaylist;
            var currentOrder = playlist.shuffled
                ? (IEnumerable<TrackReference>)new DeckShuffled<TrackReference>(playlist.References)
                : playlist.References;

            if (playlist.loopMode == Playlist.LoopMode.LoopOne)
                currentOrder = currentOrder.Skip(playlist.selected).Take(1);
            while (!musicPlayer.stopped)
            {
                if (currentOrder is DeckShuffled<TrackReference> deckShuffled)
                    deckShuffled.Reshuffle();
                foreach (var reference in currentOrder)
                {
                    Playlist.SongData currentSong;
                    musicPlayer.playlistEditor.customPlaylist.GetSongData(reference, out currentSong);
                    Debug.Log($"[CybergrindMusicExplorer] Now playing ${currentSong.name}");

                    // Only allow boosting for custom tracks
                    manager.allowMusicBoost = reference.Type == SoundtrackType.External;

                    if (lastSong != currentSong)
                        musicPlayer.StartCoroutine(musicPlayer.ShowPanelRoutine(currentSong));
                    if (first)
                    {
                        if (currentSong.introClip != null)
                        {
                            musicPlayer.changer.ChangeTo(currentSong.introClip);
                            yield return themeNotPlaying;
                        }

                        first = false;
                    }

                    var i = 0;
                    foreach (var clip in currentSong.clips)
                    {
                        if (musicPlayer.playlistEditor.customPlaylist.loopMode == Playlist.LoopMode.LoopOne ||
                            currentSong.maxClips <= -1 || i < currentSong.maxClips)
                        {
                            musicPlayer.changer.ChangeTo(clip);
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