using System;
using System.Threading.Tasks;
using BepInEx;
using CybergrindMusicExplorer.Components;
using CybergrindMusicExplorer.Downloader;
using CybergrindMusicExplorer.GUI;
using CybergrindMusicExplorer.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CybergrindMusicExplorer.Patches.Patches;

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
            Startup();
            
            AssetsManager.Instance.LoadAssets();
            AssetsManager.Instance.RegisterPrefabs();
        }

        private void Startup()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            GUIManager.Init();
            cybergrindEffectsChanger = CybergrindEffectsChanger.Instance;
            tracksDownloadManager = TracksDownloadManager.Instance;
            #pragma warning disable CS4014
            RetrieveNewestVersion();
            #pragma warning restore CS4014
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene != SceneManager.GetActiveScene())
                return;

            switch (SceneHelper.CurrentScene)
            {
                case "Endless":
                {
                    MonoSingleton<CybergrindMusicExplorerManager>.Instance.allowMusicBoost = false;
                    PatchMusicManager();
                    PatchMusicChanger();
                    
                    if (fileBrowser != null)
                        break;
                    OnCybergrind();
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

        private void OnCybergrind()
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