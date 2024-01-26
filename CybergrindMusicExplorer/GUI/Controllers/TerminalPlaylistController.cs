using System.Collections;
using System.Collections.Generic;
using System.IO;
using CybergrindMusicExplorer.GUI.Attributes;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.GUI.Controllers
{
    public class TerminalPlaylistController : UIController
    {
        private static readonly List<string> SlotsNames = new List<string>
        {
            "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH", "SIXTH"
        };

        private readonly CybergrindMusicExplorerManager manager = CybergrindMusicExplorerManager.Instance;
        
        [PrefabAsset("assets/ui/elements/button.prefab")] private static GameObject buttonPrefab;
        [UIElement("RemoveButton")] [HudEffect] private GameObject removeButton;
        [UIElement("SelectPlaylist")] [HudEffect] private GameObject selectPlaylistButton;
        [HudEffect] [UIElement("PlaylistSelectionWindow")] private GameObject playlistSelectionWindow;
        [HudEffect] [UIElement("PlaylistSelectionWindow/Border/Grid")] private GameObject grid;

        private new void Awake()
        {
            base.Awake();
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            BindControls();
            yield break;
        }

        private void BindControls()
        {
            var playlistEditor = CybergrindMusicExplorer.GetPlaylistEditor();
            var browser = (CustomMusicSoundtrackBrowser)GetPrivate(playlistEditor, typeof(CustomMusicPlaylistEditor), "browser");
            
            InstantiateButtons(playlistEditor, browser);
            
            foreach (var button in GetComponentsInChildren<Button>())
                AddShopButton(button.gameObject);
            
            selectPlaylistButton.GetComponent<Button>().onClick.AddListener(() =>
                playlistSelectionWindow.SetActive(true));
            removeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ResetPlaylist(playlistEditor, browser);
                playlistEditor.Rebuild();
            });
            
        }

        private void InstantiateButtons(CustomMusicPlaylistEditor playlistEditor, CustomMusicSoundtrackBrowser browser)
        {
            for (var i = 0; i < SlotsNames.Count; i++)
            {
                var slotButton = Instantiate(buttonPrefab, grid.transform);
                AddShopButton(slotButton);
                slotButton.GetComponentInChildren<TextMeshProUGUI>().text = SlotsNames[i];
                var currentSlot = i + 1;
                var button = slotButton.GetComponent<Button>();
                var selectedSlot = currentSlot == manager.SelectedPlaylistSlot;
                
                if (selectedSlot)
                {
                    var colors = button.colors;
                    colors.normalColor = Color.red;
                    colors.highlightedColor = Color.red;
                    button.colors = colors;
                }
                
                slotButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    playlistSelectionWindow.SetActive(false);
                    if (selectedSlot)
                        return;
                    
                    manager.SelectedPlaylistSlot = currentSlot;
                    ReloadPlaylist(playlistEditor, browser);

                    foreach (Transform child in grid.transform)
                        Destroy(child.gameObject);
                    
                    InstantiateButtons(playlistEditor, browser);
                });
            }
        }

        private void ReloadPlaylist(CustomMusicPlaylistEditor playlistEditor, CustomMusicSoundtrackBrowser browser)
        {
            playlistEditor.playlist.OnChanged -= playlistEditor.SavePlaylist;
            playlistEditor.playlist = new Playlist();
            playlistEditor.ToggleLoopMode();

            Playlist loadedPlaylist;
            using (var streamReader = new StreamReader(File.Open(Playlist.currentPath, FileMode.OpenOrCreate)))
                loadedPlaylist = JsonConvert.DeserializeObject<Playlist>(streamReader.ReadToEnd());
            if (loadedPlaylist == null)
            {
                ResetPlaylist(playlistEditor, browser);
                playlistEditor.SavePlaylist();
            }

            CallPrivate(playlistEditor, typeof(CustomMusicPlaylistEditor), "Start");
        }

        private static void AddShopButton(GameObject go) => go.AddComponent<ShopButton>().deactivated = true;
        
        private static void ResetPlaylist(CustomMusicPlaylistEditor playlistEditor, CustomMusicSoundtrackBrowser browser)
        {
            foreach (var referenceSoundtrackSong in browser.rootFolder)
                playlistEditor.playlist.Add(new Playlist.SongIdentifier(referenceSoundtrackSong.AssetGUID, Playlist.SongIdentifier.IdentifierType.Addressable));
            
            while (playlistEditor.playlist.Count != browser.rootFolder.Count)
                playlistEditor.playlist.Remove(0);
        }
    }
}