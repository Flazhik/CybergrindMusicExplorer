using System;
using System.Collections;
using System.Threading.Tasks;
using BepInEx;
using CybergrindMusicExplorer.Components;
using CybergrindMusicExplorer.Downloader;
using CybergrindMusicExplorer.GUI;
using CybergrindMusicExplorer.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CybergrindMusicExplorer.Patches.Patches;
using static CybergrindMusicExplorer.Util.KeyUtils;

namespace CybergrindMusicExplorer

{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class CybergrindMusicExplorer : BaseUnityPlugin
    {
        private static Version _newestVersion = new Version("1.0.0");

        private static CustomMusicPlaylistEditor _editor;
        private EnhancedMusicFileBrowser fileBrowser;
        private GameObject cybergrindMusicExplorerSettings;
        private GUIManager GUIManager;
        private CybergrindEffectsChanger cybergrindEffectsChanger;
        private TracksDownloadManager tracksDownloadManager;
        private bool menuMessageIsShown;

        public static CustomMusicPlaylistEditor GetPlaylistEditor() => _editor;
        
        
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
            tracksDownloadManager = TracksDownloadManager.Instance;
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
                    PatchMusicChanger();
                    
                    if (fileBrowser != null)
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
            PatchPlaylist();
            PatchCustomMusicPlaylistEditor();
            PatchDirectoryTree();
            PatchFinalCyberRank();
            PatchWaveMenu();
            PatchScreenZone();

            EnhancedMusicFileBrowser.OnInit += OnEnhancedBrowserInit;
            var oldBrowser = FindObjectOfType<CustomMusicFileBrowser>();
            var musicObject = oldBrowser.transform.gameObject;
            fileBrowser = musicObject.AddComponent<EnhancedMusicFileBrowser>();
            Destroy(oldBrowser);
            ClearTmpDirectory();
            yield return new WaitForSeconds(2.5f);
            DisplayMenuMessage();
        }

        private void DisplayMenuMessage()
        {
            var manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
            if (menuMessageIsShown)
                return;

            HudMessageReceiver.Instance.SendHudMessage(
                $"To open Cybergrind Music Explorer settings now or midgame, press [<color=orange>{ToHumanReadable((KeyCode)manager.MenuBinding)}</color>]");
            menuMessageIsShown = true;
        }

        private void OnEnhancedBrowserInit()
        {
            _editor = FindObjectOfType<CustomMusicPlaylistEditor>();
            PatchMusicPlayer();
        }

        private static void ClearTmpDirectory()
        {
            if (!PathsUtils.TemporaryFilesDirectory.Exists)
                return;
            
            foreach (var fileInfo in PathsUtils.TemporaryFilesDirectory.GetFiles())
            {
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Can't delete temporary file {fileInfo.Name}");
                }
            }
        }

        private static async Task RetrieveNewestVersion()
        {
            await UpdatesManager.GetNewVersion();
            _newestVersion = UpdatesManager.NewestVersion;
        }
    }
}