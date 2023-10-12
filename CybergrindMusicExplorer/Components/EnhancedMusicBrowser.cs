using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
using SubtitlesParser.Classes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;
using static CybergrindMusicExplorer.Util.CustomTracksNamingUtil;
using static CybergrindMusicExplorer.Util.PathsUtil;

namespace CybergrindMusicExplorer.Components
{
    public class EnhancedMusicBrowser : DirectoryTreeBrowser<TrackReference>
    {
        private const string BaseCanvasPath = "/FirstRoom/Room/CyberGrindSettings/Canvas/SoundtrackMusic/Panel/";
        private readonly CybergrindMusicExplorerManager manager = CybergrindMusicExplorerManager.Instance;
        
        public static event Action OnInit;

        public List<AssetReferenceSoundtrackSong> rootFolder = new List<AssetReferenceSoundtrackSong>();
        public List<SoundtrackFolder> levelFolders = new List<SoundtrackFolder>();

        private GameObject playlistEditorPanel;
        private CyberGrindSettingsNavigator navigator;
        private GameObject loadingPrefab;
        private Sprite lockedLevelSprite;
        private Sprite defaultIcon;
        private GameObject loadAllButton;

        private FakeDirectoryTree<TrackReference> _baseDirectory =
            new FakeDirectoryTree<TrackReference>("", new TrackReference[] { });
        private FileDirectoryTree customSongsDirectory => new FileDirectoryTree(CustomSongsPath);
        protected override int maxPageLength => 4;
        protected override IDirectoryTree<TrackReference> baseDirectory => _baseDirectory;
        
        private TracksLoader tracksLoader;
        private EnhancedMusicPlaylistEditor playlistEditor;

        private void Awake()
        {
            CloneInstance(
                FindObjectOfType<CustomMusicSoundtrackBrowser>(),
                this,
                privateFieldsToCopy: new List<string>
                {
                    "plusButton", "backButton", "pageText", "cleanupActions"
                },
                fieldsToIgnore: new List<string>
                {
                    "_baseDirectory", "referenceCache"
                });
            DestroyObsoleteInstance();

            RewireButtonTo(ControlButton("NextButton"), NextPage);
            RewireButtonTo(ControlButton("PreviousButton"), PreviousPage);
            RewireButtonTo(ControlButton("ImageSelectorWrapper/BackButton"), StepUp);
            InstantiateLoadAllButton();

            _baseDirectory = Folder("Songs",
                children: new List<IDirectoryTree<TrackReference>>
                {
                    CustomSongsFolder(),
                    OstFolder()
                });
            tracksLoader = new TracksLoader(defaultIcon);

            SetPrivate(this, typeof(DirectoryTreeBrowser<TrackReference>), "currentDirectory", baseDirectory);
            OnInit?.Invoke();
        }

        private void OnEnable() => Rebuild();

        private IDirectoryTree<TrackReference> OstFolder() => 
            Folder("OST", children: levelFolders
                .Select(f => new FakeDirectoryTree<TrackReference>(f.name, f.songs
                    .Select(s => new TrackReference(SoundtrackType.Asset, s.AssetGUID))
                    .ToList()))
                .Cast<IDirectoryTree<TrackReference>>()
                .ToList());

        private IDirectoryTree<TrackReference> CustomSongsFolder() => 
            TraverseFileTree(customSongsDirectory, "Custom tracks");

        private void SelectSong(TrackReference reference, CustomSongData song)
        {
            if (song.clips.Count > 0)
            {
                if (manager.PreventDuplicateTracks && playlistEditor.Playlist.References.Contains(reference))
                    Debug.Log("Attempted to add duplicate track, ignoring.");
                else {
                    var target = playlistEditor.PageOf(playlistEditor.Playlist.Count);
                    playlistEditor.Playlist.AddTrack(reference, song);
                    playlistEditor.Rebuild(false);
                    playlistEditor.SetPage(target);
                }

                navigator.GoToNoMenu(playlistEditorPanel);
            }
            else
                Debug.LogWarning("Attempted to add song with no clips to playlist.");
        }

        private IEnumerator LoadSongButton(TrackReference reference, GameObject btn)
        {
            var placeholder = Instantiate(loadingPrefab, itemParent, false);
            placeholder.SetActive(true);

            switch (reference.Type)
            {
                case SoundtrackType.Asset:
                {
                    CustomSongData song = null;
                    yield return tracksLoader.LoadSongData(reference, data => song = data);
                    
                    Destroy(placeholder);

                    if (song != null)
                        DrawOstButton(song, reference, btn);

                    break;
                }
                case SoundtrackType.External:
                {
                    CustomSongData song = null;
                    yield return LoadSongDataAndSubtitles(reference, data => song = data);
                    
                    Destroy(placeholder);

                    if (song != null)
                        DrawCustomTrackButton(song, reference, btn);
                    break;
                }
                default:
                    Debug.LogError(
                        $"Trying to load unknown soundtrack reference={reference.Reference}, type {reference.Type}, ignoring");
                    break;
            }
        }
        
        private IEnumerator LoadAllFromFolder(IDirectoryTree<TrackReference> folder)
        {
            foreach (var reference in folder.GetFilesRecursive())
            {
                CustomSongData data = null;
                yield return StartCoroutine(LoadSongDataAndSubtitles(reference, song => data = song));
                if (!manager.PreventDuplicateTracks || !playlistEditor.Playlist.References.Contains(reference))
                    playlistEditor.Playlist.AddTrack(reference, data);
            }
            
            playlistEditor.Rebuild();

            // Going back to playlist using... some unconventional methods
            var backButton = GameObject.Find($"{BaseCanvasPath}BackButton").GetComponent<ShopButton>();
            var controllerPointerType = typeof(ShopButton).Assembly.GetType("ControllerPointer");
            var controllerPointer = GetPrivate(backButton, typeof(ShopButton), "pointer");
            var onPressed = (UnityEvent)GetPrivate(controllerPointer, controllerPointerType, "onPressed");
            onPressed.Invoke();
        }

        private IEnumerator LoadSongDataAndSubtitles(TrackReference reference, Action<CustomSongData> callback)
        {
            CustomSongData song = null;
            List<List<SubtitleItem>> subtitles = default;
            var fileInfo = new FileInfo(Path.Combine(CustomSongsPath, reference.Reference));
            yield return tracksLoader.LoadSongData(fileInfo, data => song = data, subs => subtitles = subs);
                    
            if (subtitles != default && subtitles.Count != 0)
                playlistEditor.Playlist.AddSubtitles(reference, subtitles);
            
            callback.Invoke(song);
        }

        public void DrawOstButton(CustomSongData song, TrackReference reference, GameObject button)
        {
            var componentInChildren = button.GetComponentInChildren<CustomContentButton>();
            componentInChildren.button.onClick.RemoveAllListeners();
            if (song.conditions.AllMet())
            {
                componentInChildren.icon.sprite = song.icon != null ? song.icon : defaultIcon;
                componentInChildren.text.text =
                    song.name.ToUpper();
                componentInChildren.costText.text = "<i>UNLOCKED</i>";
                componentInChildren.button.onClick.AddListener(() => SelectSong(reference, song));
                SetActiveAll(componentInChildren.objectsToActivateIfAvailable, true);
                button.SetActive(true);
            }
            else
            {
                SetActiveAll(componentInChildren.objectsToActivateIfAvailable, false);
                componentInChildren.text.text = "????????? " + song.extraLevelBit;
                componentInChildren.costText.text = song.conditions.DescribeAll();
                componentInChildren.icon.sprite = lockedLevelSprite;
                componentInChildren.border.color = Color.grey;
                componentInChildren.text.color = Color.grey;
                componentInChildren.costText.color = Color.grey;
                button.SetActive(true);
            }
        }

        public void DrawCustomTrackButton(CustomSongData song, TrackReference reference, GameObject button)
        {
            var componentInChildren = button.GetComponentInChildren<CustomContentButton>();
            componentInChildren.button.onClick.RemoveAllListeners();

            componentInChildren.icon.sprite = song.icon != null ? song.icon : defaultIcon;
            componentInChildren.text.text = song.name.ToUpper();
            componentInChildren.costText.text = "<i>Custom</i>";
            componentInChildren.button.onClick.AddListener(() => SelectSong(reference, song));
            SetActiveAll(componentInChildren.objectsToActivateIfAvailable, true);
            button.SetActive(true);
        }

        protected override Action BuildDirectory(IDirectoryTree<TrackReference> folder, int indexInPage)
        {
            var btn = Instantiate(folderButtonTemplate, itemParent, false);
            btn.GetComponent<Button>().onClick.RemoveAllListeners();
            btn.GetComponent<Button>().onClick.AddListener(() => StepDown(folder));
            btn.GetComponentInChildren<Text>().text = folder.name.ToUpper();
            btn.SetActive(true);
            return () => Destroy(btn);
        }
        
        private void InstantiateLoadAllButton()
        {
            loadAllButton = Instantiate(ControlButton("PreviousButton").gameObject,
                GameObject.Find(BaseCanvasPath).transform);
            loadAllButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 45f);
            loadAllButton.transform.localPosition = new Vector3(64f, 105f, -20f);

            var textComponent = loadAllButton.transform.Find("Text").GetComponent<Text>();
            textComponent.text = "LOAD ALL";
            textComponent.fontSize = 16;
            
            loadAllButton.name = "Load All";
            loadAllButton.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        public override void Rebuild(bool setToPageZero = true)
        {
            var currentDirectory = (IDirectoryTree<TrackReference>)GetPrivate(this,
                typeof(DirectoryTreeBrowser<TrackReference>), "currentDirectory");

            if (currentDirectory.parent == null || IsOstFolder(currentDirectory))
                loadAllButton.SetActive(false);
            else {
                loadAllButton.SetActive(true);
                loadAllButton.GetComponent<Button>().onClick.RemoveAllListeners();
                loadAllButton.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(LoadAllFromFolder(currentDirectory)));
            }
            base.Rebuild(setToPageZero);
        }

        private static bool IsOstFolder(IDirectoryTree<TrackReference> folder)
        {
            var current = folder;
            while (current.parent != null)
            {
                if (current.parent.parent == null && current.name == "OST")
                    return true;
                current = current.parent;
            }

            return false;
        }

        protected override Action BuildLeaf(TrackReference reference, int indexInPage)
        {
            var btn = Instantiate(itemButtonTemplate, itemParent, false);
            StartCoroutine(LoadSongButton(reference, btn));
            return () => Destroy(btn);
        }

        private IDirectoryTree<TrackReference> TraverseFileTree(IDirectoryTree<FileInfo> tree, string folderName)
        {
            var children = tree.children
                .Where(child => child.name != SpecialEffectsPath.Name)
                .ToList();
            
            var tracks = tree.files
                .Where(file => AudioTypesByExtension.ContainsKey(file.Extension.ToLower()))
                .ToList();

            var segmentedTracks = tracks
                .Where(IntroOrLoopPart)
                .GroupBy(file => WithoutPostfix(file).FullName)
                .Where(HasIntroAndLoop)
                .ToList();

            var regularTracks = tracks
                .Where(track => !HasSpecialPostfix(track, "calmintro")
                                && !HasSpecialPostfix(track, "calmloop")
                                && !HasSpecialPostfix(track, "calm"))
                .Select(track => track.FullName)
                .Where(track => !segmentedTracks
                    .SelectMany(t => t)
                    .Select(t => t.FullName)
                    .Contains(track))
                .ToList();

            var newTree = new FakeDirectoryTree<TrackReference>(
                folderName,
                segmentedTracks
                    .Select(group => group.Key)
                    .Concat(regularTracks)
                    .Select(track => new TrackReference(SoundtrackType.External,
                        track.Substring(CustomSongsPath.Length + 1)))
                    .ToList(),
                children.Select(TraverseFileTree).ToList()
            );
            foreach (var child in newTree.children)
                child.parent = newTree;

            return newTree;
        }

        private IDirectoryTree<TrackReference> TraverseFileTree(IDirectoryTree<FileInfo> tree) =>
            TraverseFileTree(tree, tree.name);

        private void DestroyObsoleteInstance()
        {
            var oldPlaylistEditor = FindObjectOfType<CustomMusicPlaylistEditor>();
            var playlistEditorParentObject = oldPlaylistEditor.transform.gameObject;
            playlistEditor = playlistEditorParentObject.AddComponent<EnhancedMusicPlaylistEditor>();
            Destroy(oldPlaylistEditor);
        }

        private static void RewireButtonTo(Button button, UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }

        private static Button ControlButton(string path) =>
            GameObject.Find(BaseCanvasPath + path).GetComponent<Button>();

        private static void SetActiveAll(List<GameObject> objects, bool active) =>
            objects.ForEach(o => o.SetActive(active));
    }
}