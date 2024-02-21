using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CybergrindMusicExplorer.Components;
using CybergrindMusicExplorer.Downloader;
using CybergrindMusicExplorer.GUI.Attributes;
using CybergrindMusicExplorer.GUI.Elements;
using CybergrindMusicExplorer.Scripts;
using CybergrindMusicExplorer.Scripts.Data;
using CybergrindMusicExplorer.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.VersionUtils;
using static CybergrindMusicExplorer.Util.KeyUtils;
using Debug = UnityEngine.Debug;

namespace CybergrindMusicExplorer.GUI.Controllers
{
    public class CgmeMenuController : UIController
    {
        private const string ThunderUrl = "https://thunderstore.io/package/download/Flazhik/CybergrindMusicExplorer";
        private const string GithubUrl = "https://github.com/Flazhik/CybergrindMusicExplorer/releases/download";
        public event Action OnClose;
        public event EndSliderDragEvent OnScaled;
        
        private readonly PrefsManager prefsManager = PrefsManager.Instance;
        private readonly OptionsMenuToManager optionsManager = OptionsMenuToManager.Instance;
        private readonly AudioMixerController mixer = AudioMixerController.Instance;
        private readonly TracksDownloadManager tracksDownloadManager = TracksDownloadManager.Instance;
        private CalmThemeManager calmThemeManager;

        [PrefabAsset("assets/ui/elements/enemycounter.prefab")]
        private static GameObject enemyCounterPrefab;

        [HudEffect] [UIElement("Header/Tab")] private GameObject tabName;
        [HudEffect] [UIElement("Menu/General")] private GameObject general;
        [HudEffect] [UIElement("Menu/Bindings")] private GameObject bindings;
        [HudEffect] [UIElement("Menu/Themes")] private GameObject themes;
        [HudEffect] [UIElement("Menu/Downloader")] private TracksDownloader tracksDownloader;
        [HudEffect] [UIElement("Menu/Manual")] private GameObject manual;
        [HudEffect] [UIElement("Menu/Credits")] private GameObject credits;
        [UIElement("Menu/General/ListControl/ScrollArea/Mask/Viewport/List/InfinitePanel/Controller/Checkbox")]
        private Toggle showPanelIndefinitely;
        [UIElement("Menu/General/ListControl/ScrollArea/Mask/Viewport/List/EnablePreview/Controller/Checkbox")]
        private Toggle enablePreview;
        [UIElement("Menu/General/ListControl/ScrollArea/Mask/Viewport/List/Subtitles/Controller/Checkbox")]
        private Toggle subtitles;
        [UIElement("Menu/General/ListControl/ScrollArea/Mask/Viewport/List/Boost/Controller/Slider/Slider")]
        private SliderAndValue boost;
        [UIElement("Menu/General/ListControl/ScrollArea/Mask/Viewport/List/Scale/Controller/Slider/Slider")]
        private SliderAndValue scale;
        [UIElement("Menu/General/ListControl/ScrollArea/Mask/Viewport/List/BigNowPlayingPanel/Controller/Checkbox")]
        private Toggle bigNowPlayingPanel;        
        [UIElement("Menu/General/ListControl/ScrollArea/Mask/Viewport/List/PreventDuplicates/Controller/Checkbox")]
        private Toggle preventDuplicates;
        [UIElement("Menu/Bindings/ShowMenuHotkey/Controller/CGMEMenu")]
        private ControlBinding showMenuHotkey;
        [UIElement("Menu/Bindings/NextTrackHotkey/Controller/CGMENextTrack")]
        private ControlBinding nextTrackHotkey;
        [UIElement("Menu/Bindings/PlaybackHotkey/Controller/CGMEPlaybackMenu")]
        private ControlBinding playbackHotkey;
        [UIElement("Menu/Bindings/DisablePlayerHotkey/Controller/CGMEDisablePlayer")]
        private ControlBinding displayPlayerHotkey;
        [UIElement("Menu/Themes/EnableCalmTheme/Controller/Checkbox")]
        private Toggle calmTheme;
        [UIElement("Menu/Themes/EnemiesThreshold/Controller/Counter")]
        private Counter enemiesThreshold;
        [UIElement("Menu/Themes/CalmThemeSettings/ListControl/Scrollbar")]
        private Scrollbar specialEnemiesScrollbar;
        [UIElement("Menu/Themes/CalmThemeSettings/ListControl/ScrollArea/Mask/Viewport/List")]
        private GameObject specialEnemiesList;
        [UIElement("ThunderDownload")] private Button thunderDownload;
        [UIElement("GithubDownload")] private Button githubDownload;
        [UIElement("Version")] private TextMeshProUGUI version;
        [UIElement("Close")] private Button close;

        private Coroutine downloaderCoroutine;

        private new void Awake()
        {
            base.Awake();
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            while (CalmThemeManager.Instance == null || !CalmThemeManager.Instance.loaded)
                yield return null;

            calmThemeManager = CalmThemeManager.Instance;
            close.onClick.AddListener(() => OnClose?.Invoke());
            
            BindControls();
            BindHotkeys();
            SetupThemes();
            SetupDownloader();
            SetupSpecialEnemiesControls();
            HandleCurrentVersion();
        }

        private void BindControls()
        {
            showPanelIndefinitely.isOn = Manager.ShowCurrentTrackPanelIndefinitely;
            showPanelIndefinitely.onValueChanged.AddListener(state =>
                Manager.ShowCurrentTrackPanelIndefinitely = state);
            
            enablePreview.isOn = Manager.EnableTracksPreview;
            enablePreview.onValueChanged.AddListener(state =>
                Manager.EnableTracksPreview = state);            
            
            bigNowPlayingPanel.isOn = Manager.ShowBigNowPlayingPanel;
            bigNowPlayingPanel.onValueChanged.AddListener(state =>
                Manager.ShowBigNowPlayingPanel = state);

            subtitles.isOn = prefsManager.GetBool("subtitlesEnabled");
            subtitles.onValueChanged.AddListener(state =>
                optionsManager.SetSubtitles(state));
            
            boost.value.text = $"+{Manager.CustomTracksBoost}dB";
            boost.Value = Manager.CustomTracksBoost;
            boost.OnValueChanged.AddListener(value =>
            {
                boost.value.text = $"+{value}dB";
                Manager.CustomTracksBoost = value;
                mixer.SetMusicVolume(mixer.musicVolume);
            });
            
            scale.value.text = $"{Manager.MenuUpscale}%";
            scale.Value = Manager.MenuUpscale;
            scale.OnValueChanged.AddListener(value =>
            {
                scale.value.text = $"{value}%";
            });

            scale.EndDrag += value => Manager.MenuUpscale = value;
            scale.EndDrag += value => OnScaled?.Invoke(value);
            
            preventDuplicates.isOn = Manager.PreventDuplicateTracks;
            preventDuplicates.onValueChanged.AddListener(state =>
            {
                Manager.PreventDuplicateTracks = state;
                if (!state)
                    return;
                
                var playlistEditor = CybergrindMusicExplorer.GetPlaylistEditor();
                var playlist = playlistEditor.playlist;
                
                var duplicates = new List<Playlist.SongIdentifier>();
                for (var i = 0; playlist.ids.ElementAtOrDefault(i) != default; i++)
                    if (duplicates.Contains(playlist.ids[i]))
                    {
                        playlist.Remove(i);
                        i--;
                    }
                    else
                        duplicates.Add(playlist.ids[i]);
                
                playlistEditor.Rebuild();
            });
        }

        private void BindHotkeys()
        {
            showMenuHotkey.button.onClick.AddListener(() => ChangeKey(showMenuHotkey.button.gameObject));
            showMenuHotkey.value.text = ToHumanReadable((KeyCode)Manager.MenuBinding);

            nextTrackHotkey.button.onClick.AddListener(() => ChangeKey(nextTrackHotkey.button.gameObject));
            nextTrackHotkey.value.text = ToHumanReadable((KeyCode)Manager.NextTrackBinding);

            playbackHotkey.button.onClick.AddListener(() => ChangeKey(playbackHotkey.button.gameObject));
            playbackHotkey.value.text = ToHumanReadable((KeyCode)Manager.PlaybackMenuBinding);
            
            displayPlayerHotkey.button.onClick.AddListener(() => ChangeKey(displayPlayerHotkey.button.gameObject));
            displayPlayerHotkey.value.text = ToHumanReadable((KeyCode)Manager.DisablePlayerBinding);
        }

        private void SetupThemes()
        {
            calmTheme.isOn = Manager.PlayCalmTheme;
            calmTheme.onValueChanged.AddListener(value => Manager.PlayCalmTheme = value);
            enemiesThreshold.SetDefaultValue(Manager.CalmThemeEnemiesThreshold);
            enemiesThreshold.OnChanged += newValue => Manager.CalmThemeEnemiesThreshold = newValue;
        }

        private void SetupDownloader()
        {
            tracksDownloader.SetDownloadCallback((entry, downloader) => tracksDownloadManager.Download(entry, downloader));
            
            tracksDownloader.SetDownloadAllCallback((entries, downloader) =>
                tracksDownloadManager.DownloadAll(entries.Where(e => e.State == DownloadableTrackEntryState.Idle).ToList(), downloader));

            tracksDownloader.URLChanged += url =>
            {
                tracksDownloadManager.Cancel();
                if (!tracksDownloadManager.SupportsUrl(url))
                {
                    tracksDownloader.DisplayMessage("URL is invalid or not supported");
                    return;
                }

                tracksDownloader.LoadingStarted();
                
                if (downloaderCoroutine != null)
                    StopCoroutine(downloaderCoroutine);
                
                downloaderCoroutine = StartCoroutine(OnUrlChangedCoroutine(url));
            };
            tracksDownloader.restartButton.GetComponent<Button>().onClick.AddListener(SceneHelper.RestartScene);
        }

        private IEnumerator OnUrlChangedCoroutine(string url)
        {
            tracksDownloader.HideDownloadAllButton();
            if (!tracksDownloadManager.SupportsUrl(url))
            {
                tracksDownloader.DisplayMessage("URL is invalid or not supported");
                yield break;
            }
            tracksDownloader.LoadingStarted();

            var metadataList = new List<DownloadableTrackMetadata>();
            yield return tracksDownloadManager.GetMetadata(url, metadata =>
            {
                try
                {
                    metadataList.Add(metadata);
                    tracksDownloader.AddEntry(metadata);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"An error has occured while receiving metadata for URL {url}: {e.Message}");
                }
            }, tracksDownloader);
            
            tracksDownloader.LoadingComplete();
        }

        private void SetupSpecialEnemiesControls()
        {
            var sprites = SpecialEnemies.LoadEnemiesSprites();
            SpecialEnemies.SpecialEnemiesNames.ToList().ForEach(pair =>
            {
                var enemyType = pair.Key;
                var enemyCounter = Instantiate(enemyCounterPrefab, specialEnemiesList.transform);
                enemyCounter.transform.Find("EnemyName").GetComponent<TextMeshProUGUI>().text = pair.Value;

                var image = enemyCounter.transform.Find("Image").GetComponent<Image>();
                image.color = Color.white;
                image.sprite = sprites[enemyType];

                BindCustomControllers(enemyCounter);
                var counter = enemyCounter.transform.Find("CGMECounter").GetComponent<Counter>();
                var checkbox = enemyCounter.transform.Find("Checkbox").GetComponent<Toggle>();

                var configPresent = calmThemeManager.Preferences.CalmTheme.SpecialEnemies.ContainsKey(enemyType);

                var defaultValue = configPresent
                    ? calmThemeManager.Preferences.CalmTheme.SpecialEnemies[enemyType]
                    : 0;

                counter.SetDefaultValue(defaultValue);
                counter.OnChanged += value => calmThemeManager.ChangeEnemyThreshold(enemyType, value);

                checkbox.isOn = configPresent;
                checkbox.onValueChanged.AddListener(value =>
                    calmThemeManager.ToggleSpecialEnemy(enemyType, value, counter.Value));
            });
        }

        private void HandleCurrentVersion()
        {
            var currentVersion = Version.Parse(PluginInfo.VERSION);
            var newestVersion = CybergrindMusicExplorer.GetNewestVersion();

            if (newestVersion > currentVersion)
            {
                var stringVersion = newestVersion.ToString(3);
                var downloadOption = R2ModmanIsRunning()
                    ? "Update via r2modman or from:"
                    : "Click to download from:";
                
                version.text =
                    $"<color=#a8a8a8>Version {stringVersion} is available! {downloadOption}</color>\n" +
                    "<color=#3498db>Thunderstore</color> <color=#a8a8a8>|</color> <color=#f5f5f5>GitHub</color>";

                thunderDownload.gameObject.SetActive(true);
                githubDownload.gameObject.SetActive(true);
                
                thunderDownload.onClick.AddListener(() =>
                    DownloadNewestVersion(
                        $"{ThunderUrl}/{stringVersion}/"));
                githubDownload.onClick.AddListener(() =>
                    DownloadNewestVersion($"{GithubUrl}/v{stringVersion}/CybergrindMusicExplorer.v{stringVersion}.zip"));

                return;
            }

            version.text = $"<color=#545454>Version {currentVersion.ToString(3)}</color>";
            if (!ComicallyOldVersion.Equals(newestVersion) && currentVersion > newestVersion)
                version.text += " <color=#888>[beta]</color>";
        }

        private static void DownloadNewestVersion(string url) => Process.Start(url);

        private static bool R2ModmanIsRunning() => Process.GetProcessesByName("r2modman").Length > 0;
    }
}