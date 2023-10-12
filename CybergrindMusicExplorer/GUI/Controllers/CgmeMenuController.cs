using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using CybergrindMusicExplorer.Components;
using CybergrindMusicExplorer.GUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using static ControlsOptions;

namespace CybergrindMusicExplorer.GUI.Controllers
{
    public class CgmeMenuController : UIController
    {
        private const string ThunderUrl = "https://thunderstore.io/package/download/Flazhik/CybergrindMusicExplorer";
        private const string GithubUrl = "https://github.com/Flazhik/CybergrindMusicExplorer/releases/download";
        public event Action OnClose;
        private readonly PrefsManager prefsManager = PrefsManager.Instance;
        private readonly OptionsMenuToManager optionsManager = OptionsMenuToManager.Instance;
        private readonly AudioMixerController mixer = AudioMixerController.Instance;
        private CalmThemeManager calmThemeManager;

        [PrefabAsset("assets/ui/elements/enemycounter.prefab")]
        private static GameObject enemyCounterPrefab;

        [HudEffect] [UIElement("Header/Tab")] private GameObject tabName;
        [HudEffect] [UIElement("Menu/General")]
        private GameObject general;
        [HudEffect] [UIElement("Menu/Bindings")]
        private GameObject bindings;
        [HudEffect] [UIElement("Menu/Themes")] private GameObject themes;
        [HudEffect] [UIElement("Menu/Manual")] private GameObject manual;
        [HudEffect] [UIElement("Menu/Credits")] private GameObject credits;
        [UIElement("Menu/General/InfinitePanel/Controller/Checkbox")]
        private Toggle showPanelIndefinitely;
        [UIElement("Menu/General/Subtitles/Controller/Checkbox")]
        private Toggle subtitles;
        [UIElement("Menu/General/Boost/Controller/Slider")]
        private SliderWithValue boost;
        [UIElement("Menu/General/PreventDuplicates/Controller/Checkbox")]
        private Toggle preventDuplicates;
        [UIElement("Menu/Bindings/ShowMenuHotkey/Controller/CGMEMenu")]
        private ControlBinding showMenuHotkey;
        [UIElement("Menu/Bindings/NextTrackHotkey/Controller/CGMENextTrack")]
        private ControlBinding nextTrackHotkey;
        [UIElement("Menu/Bindings/PlaybackHotkey/Controller/CGMEPlaybackMenu")]
        private ControlBinding playbackHotkey;
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
        [UIElement("Version")] private Text version;
        [UIElement("Close")] private Button close;

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
            SetupSpecialEnemiesControls();
            HandleCurrentVersion();
        }

        private void BindControls()
        {
            showPanelIndefinitely.isOn = Manager.ShowCurrentTrackPanelIndefinitely;
            showPanelIndefinitely.onValueChanged.AddListener(state =>
                Manager.ShowCurrentTrackPanelIndefinitely = state);

            subtitles.isOn = prefsManager.GetBool("subtitlesEnabled");
            subtitles.onValueChanged.AddListener(state =>
                optionsManager.SetSubtitles(state));

            boost.value.text = $"+{Manager.CustomTracksBoost}dB";
            boost.slider.value = Manager.CustomTracksBoost;
            boost.slider.onValueChanged.AddListener(value =>
            {
                boost.value.text = $"+{value}dB";
                Manager.CustomTracksBoost = value;
                mixer.SetMusicVolume(mixer.musicVolume);
            });
            
            preventDuplicates.isOn = Manager.PreventDuplicateTracks;
            preventDuplicates.onValueChanged.AddListener(state =>
            {
                Manager.PreventDuplicateTracks = state;
                if (state)
                    CybergrindMusicExplorer.GetEnhancedPlaylistEditor().RemoveDuplicates();
            });

        }

        private void BindHotkeys()
        {
            showMenuHotkey.button.onClick.AddListener(() => ChangeKey(showMenuHotkey.button.gameObject));
            showMenuHotkey.value.text = GetKeyName((KeyCode)Manager.MenuBinding);

            nextTrackHotkey.button.onClick.AddListener(() => ChangeKey(nextTrackHotkey.button.gameObject));
            nextTrackHotkey.value.text = GetKeyName((KeyCode)Manager.NextTrackBinding);

            playbackHotkey.button.onClick.AddListener(() => ChangeKey(playbackHotkey.button.gameObject));
            playbackHotkey.value.text = GetKeyName((KeyCode)Manager.PlaybackMenuBinding);
        }

        private void SetupThemes()
        {
            calmTheme.isOn = Manager.PlayCalmTheme;
            calmTheme.onValueChanged.AddListener(value => Manager.PlayCalmTheme = value);
            enemiesThreshold.SetDefaultValue(Manager.CalmThemeEnemiesThreshold);
            enemiesThreshold.OnChanged += newValue => Manager.CalmThemeEnemiesThreshold = newValue;
        }

        private void SetupSpecialEnemiesControls()
        {
            var sprites = SpecialEnemies.LoadEnemiesSprites();
            SpecialEnemies.SpecialEnemiesNames.ToList().ForEach(pair =>
            {
                var enemyType = pair.Key;
                var enemyCounter = Instantiate(enemyCounterPrefab, specialEnemiesList.transform);
                enemyCounter.transform.Find("EnemyName").GetComponent<Text>().text = pair.Value;

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
                var a = R2ModmanIsRunning()
                    ? "Update via r2modman or from:"
                    : "Click to download from:";
                
                version.text =
                    $"<color=#a8a8a8>Version {stringVersion} is available! {a}</color>\n" +
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
        }
        private void ChangeTabName(string newName) => tabName.GetComponent<Text>().text = $"-- {newName} --";
        
        private static void DownloadNewestVersion(string url) => Process.Start(url);

        private static bool R2ModmanIsRunning() => Process.GetProcessesByName("r2modman").Length > 0;
    }
}