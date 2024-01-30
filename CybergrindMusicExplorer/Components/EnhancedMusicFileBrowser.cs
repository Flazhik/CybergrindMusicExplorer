using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.GUI;
using CybergrindMusicExplorer.Scripts.UI;
using CybergrindMusicExplorer.Util;
using TagLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;
using static CybergrindMusicExplorer.Util.MetadataUtils;
using static System.IO.Path;
using static CybergrindMusicExplorer.Util.PathsUtils;
using File = TagLib.File;

namespace CybergrindMusicExplorer.Components
{
    public class EnhancedMusicFileBrowser : DirectoryTreeBrowser<FileInfo>
    {
        private const string BaseCanvasPath = "/FirstRoom/Room/CyberGrindSettings/Canvas/CustomMusic/Panel/";

        private CybergrindMusicExplorerManager manager = CybergrindMusicExplorerManager.Instance;
        private CyberGrindSettingsNavigator navigator;
        private CustomMusicPlaylistEditor playlistEditorLogic;
        private GameObject playlistEditor;
        private GameObject loadingPrefab;
        private Sprite defaultIcon;
        private GameObject loadAllButton;
        private AudioClip selectedClip;
        private TerminalBrowserWindow browserWindow;
        private AudioClip currentSong;
        public static event Action OnInit;
        protected override int maxPageLength => 4;

        protected override IDirectoryTree<FileInfo> baseDirectory => new FileDirectoryTree(CustomSongsPath);
        private readonly Dictionary<string, (Tag, Sprite)> metadataCache = new Dictionary<string, (Tag, Sprite)>();

        private void Awake()
        {
            CloneInstance(FindObjectOfType<CustomMusicFileBrowser>(), this,
                privateFieldsToCopy: new List<string> { "plusButton", "backButton", "pageText", "cleanupActions" },
                fieldsToIgnore: new List<string> { "_baseDirectory", "referenceCache" });

            RewireButtonTo(ControlButton("NextButton"), NextPage);
            RewireButtonTo(ControlButton("PreviousButton"), PreviousPage);
            RewireButtonTo(ControlButton("ImageSelectorWrapper/BackButton"), StepUp);
            InstantiateLoadAllButton();

            StartCoroutine(WaitForGui());
        }

        private IEnumerator WaitForGui()
        {
            yield return new WaitUntil(() =>
                GUIManager.GUIDeployer != null && GUIManager.GUIDeployer.terminalBrowserWindow != null);
            browserWindow = GUIManager.GUIDeployer.terminalBrowserWindow.GetComponent<TerminalBrowserWindow>();
            Rebuild();
            OnInit?.Invoke();
        }

        protected override Action BuildLeaf(FileInfo file, int indexInPage)
        {
            if (!CustomMusicFileBrowser.extensionTypeDict.ContainsKey(file.Extension.ToLower())) return null;
            var go = Instantiate(itemButtonTemplate, itemParent, false);
            var component = go.GetComponent<CustomContentButton>();
            component.button.onClick.AddListener(() =>
            {
                var count = playlistEditorLogic.playlist.Count;
                var target = playlistEditorLogic.PageOf(count);
                playlistEditorLogic.playlist.Add(new Playlist.SongIdentifier(file.FullName,
                    Playlist.SongIdentifier.IdentifierType.File));
                playlistEditorLogic.SetPage(target);
                playlistEditorLogic.Select(count);
                navigator.GoToNoMenu(playlistEditor);
            });

            (Tag, Sprite) meta = default;
            if (metadataCache.TryGetValue(file.FullName, out var cachedMetadata))
                meta = cachedMetadata;
            else if (file.Exists)
            {
                var tags = File.Create(file.FullName).Tag;
                var coverSprite = GetAlbumCoverSprite(tags);

                meta = (tags, coverSprite);
                metadataCache.Add(file.FullName, meta);
            }

            component.icon.sprite = meta != default && meta.Item2 ? meta.Item2 : defaultIcon;
            component.text.text = meta != default && meta.Item1.Title != null
                ? meta.Item1.Title
                : GetFileNameWithoutExtension(file.Name);
            component.costText.text =
                meta != default && meta.Item1.FirstPerformer != null ? meta.Item1.FirstPerformer : "";
            go.SetActive(true);
            return () => Destroy(go);
        }

        public override void Rebuild(bool setToPageZero = true)
        {
            if (currentDirectory.parent == null && !IsCustomMusicFolder(currentDirectory))
            {
                loadAllButton.SetActive(false);
                browserWindow.removeButton.SetActive(false);
            }
            else
            {
                var confirmationWindow =
                    browserWindow.confirmationWindow.GetComponent<TerminalBrowserConfirmationWindow>();
                var removeButton = browserWindow.removeButton.GetComponent<Button>();
                browserWindow.removeButton.SetActive(true);
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(() => confirmationWindow.ShowWarning(IsCustomMusicFolder(currentDirectory)
                        ? "Are you sure you want to remove all the tracks from the root folder?\n\n<i>CyberGrind will be restarted</i>"
                        : $"Are you sure you want to remove a folder <color=orange>{currentDirectory.name.Truncate(32)}</color> with everything in it?\n\n<i>CyberGrind will be restarted</i>",
                    DeleteFolder(currentDirectory)));

                loadAllButton.SetActive(true);
                loadAllButton.GetComponent<Button>().onClick.RemoveAllListeners();
                loadAllButton.GetComponent<Button>().onClick.AddListener(() => LoadAllFromFolder(currentDirectory));
            }

            base.Rebuild(setToPageZero);
        }

        private void InstantiateLoadAllButton()
        {
            loadAllButton = Instantiate(ControlButton("PreviousButton").gameObject,
                GameObject.Find(BaseCanvasPath).transform);
            loadAllButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 45f);
            loadAllButton.transform.localPosition = new Vector3(70f, 105f, -20f);
            var textComponent = loadAllButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            textComponent.text = "LOAD ALL";
            textComponent.fontSize = 16;
            loadAllButton.name = "Load All";
            loadAllButton.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        private void LoadAllFromFolder(IDirectoryTree<FileInfo> folder)
        {
            foreach (var reference in folder.GetFilesRecursive())
            {
                if (!manager.PreventDuplicateTracks || !playlistEditorLogic.playlist.ids.Select(id => id.path)
                        .Any(id => id.Equals(reference.FullName)))
                    playlistEditorLogic.playlist.Add(new Playlist.SongIdentifier(reference.FullName,
                        Playlist.SongIdentifier.IdentifierType.File));
            }

            var count = playlistEditorLogic.playlist.Count;
            var target = playlistEditorLogic.PageOf(count);
            playlistEditorLogic.SetPage(target);
            playlistEditorLogic.Select(count);
            navigator.GoToNoMenu(playlistEditor);
            playlistEditorLogic.Rebuild();
        }

        private static UnityAction DeleteFolder(IDirectoryTree<FileInfo> directory) =>
            () =>
            {
                var dir = new DirectoryInfo(Combine(CustomSongsPath, RelativePathToDirectory(directory)));
                if (IsCustomMusicFolder(directory))
                    foreach (var f in dir.GetFiles())
                        System.IO.File.Delete(f.FullName);
                else
                    Directory.Delete(Combine(CustomSongsPath, RelativePathToDirectory(directory)), true);
                SceneHelper.RestartScene();
            };

        private static bool IsCustomMusicFolder(IDirectoryTree<FileInfo> folder) =>
            folder.parent == null && folder.name == "Music";

        private static void RewireButtonTo(Button button, UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }

        private static Button ControlButton(string path) =>
            GameObject.Find(BaseCanvasPath + path).GetComponent<Button>();
    }
}