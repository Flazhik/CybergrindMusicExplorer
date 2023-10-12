using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using CybergrindMusicExplorer.Components;
using CybergrindMusicExplorer.GUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CybergrindMusicExplorer.Patches.Patches;
using static ControlsOptions;

namespace CybergrindMusicExplorer

{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class CybergrindMusicExplorer : BaseUnityPlugin
    {
        private static Version _newestVersion = new Version("1.0.0");
        
        private static EnhancedMusicPlaylistEditor _editor;
        private EnhancedMusicBrowser browser;
        private GameObject cybergrindMusicExplorerSettings;
        private GUIManager GUIManager;
        private CybergrindEffectsChanger cybergrindEffectsChanger;
        private bool menuMessageIsShown;

        public static EnhancedMusicPlaylistEditor GetEnhancedPlaylistEditor()
        {
            return _editor;
        }
        
        public static Version GetNewestVersion() => _newestVersion;

        private void Awake()
        {
            Logger.LogInfo("Initializing Cybergrind Music Explorer");
            StartCoroutine(Startup());
            
            AssetsManager.Instance.LoadAssets();
            AssetsManager.Instance.RegisterPrefabs();
        }

        private IEnumerator Startup()
        {
            SceneManager.sceneLoaded += (scene, mode) => StartCoroutine(OnSceneLoaded(scene, mode));
            GUIManager.Init();
            cybergrindEffectsChanger = CybergrindEffectsChanger.Instance;
            UpdateNote();
            #pragma warning disable CS4014
            RetrieveNewestVersion();
            #pragma warning restore CS4014
            yield return null;
        }

        private IEnumerator OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene != SceneManager.GetActiveScene())
                yield break;

            switch (SceneHelper.CurrentScene)
            {
                case "Endless":
                {
                    MonoSingleton<CybergrindMusicExplorerManager>.Instance.allowMusicBoost = false;
                    PatchMusicManager();
                    
                    if (browser != null)
                        break;
                    yield return OnCybergrind();
                    break;
                }
                case "Main Menu":
                {
                    PatchOnStartOptionsMenu();
                    PatchOnEnableOptionsMenu();
                    break;
                }
            }
        }

        private IEnumerator OnCybergrind()
        {
            MonoSingleton<CybergrindMusicExplorerManager>.Instance.allowMusicBoost = true;
            CalmThemeManager.Instance.Init();
            PatchAudioMixer();
            PatchFinalCyberRank();

            EnhancedMusicBrowser.OnInit += OnEnhancedBrowserInit;
            var oldBrowser = FindObjectOfType<CustomMusicSoundtrackBrowser>();
            var musicObject = oldBrowser.transform.gameObject;
            browser = musicObject.AddComponent<EnhancedMusicBrowser>();
            Destroy(oldBrowser);
            yield return new WaitForSeconds(2.5f);
            DisplayMenuMessage();
        }

        private void DisplayMenuMessage()
        {
            var manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
            if (menuMessageIsShown)
                return;

            HudMessageReceiver.Instance.SendHudMessage(
                $"To open Cybergrind Music Explorer settings now or midgame, press [<color=orange>{GetKeyName((KeyCode)manager.MenuBinding)}</color>]");
            menuMessageIsShown = true;
        }

        private void OnEnhancedBrowserInit()
        {
            _editor = FindObjectOfType<EnhancedMusicPlaylistEditor>();
            PatchMusicPlayer();
        }

        private static async Task RetrieveNewestVersion()
        {
            await UpdatesManager.GetNewVersion();
            _newestVersion = UpdatesManager.NewestVersion;
        }

        private void UpdateNote()
        {
            File.WriteAllText(
                Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Cybergrind", "Music", "NOTE.txt"),
                @"ULTRAKILL doesn't support custom Cyber Grind music yet.
Unless I have something to say about it.

:)

Note that Playlist.json is ignored while CybergrindMusicExplorer mod is active, and EnhancedPlaylist.json is used instead.
But essentially its structure pretty much is the same, and same rules also apply: it must be valid json.
If you edit it, make sure you don't break the format."
            );
        }
    }
}