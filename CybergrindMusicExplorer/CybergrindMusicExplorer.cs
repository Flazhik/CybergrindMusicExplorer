using System.Collections;
using System.IO;
using BepInEx;
using CybergrindMusicExplorer.Components;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CybergrindMusicExplorer

{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class CybergrindMusicExplorer : BaseUnityPlugin
    {
        private readonly Harmony _harmony = new Harmony("Flazhik.ULTRAKILL.CybergrindMusicExplorer");
        private static EnhancedMusicPlaylistEditor _editor;
        private EnhancedMusicBrowser _browser;
        private static bool _patched;

        private void Awake()
        {
            Logger.LogInfo("Initializing Cybergrind Music Explorer");
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            UpdateNote();
            yield return null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene != SceneManager.GetActiveScene())
                return;

            // Cybergrind
            if (SceneHelper.CurrentScene == "Endless" && _browser == null)
            {
                EnhancedMusicBrowser.OnInit += OnEnhancedBrowserInit;
                var oldBrowser = FindObjectOfType<CustomMusicSoundtrackBrowser>();
                var musicObject = oldBrowser.transform.gameObject;
                _browser = musicObject.AddComponent<EnhancedMusicBrowser>();
                Destroy(oldBrowser);
            }
        }

        private void OnEnhancedBrowserInit()
        {
            _editor = FindObjectOfType<EnhancedMusicPlaylistEditor>();

            if (_patched) return;
            _harmony.PatchAll();
            _patched = true;
        }

        public static EnhancedMusicPlaylistEditor GetEnhancedPlaylistEditor()
        {
            return _editor;
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