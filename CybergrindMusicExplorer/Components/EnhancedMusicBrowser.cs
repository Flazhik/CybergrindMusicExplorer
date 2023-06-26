using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
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
        
        public static event Action OnInit;

        public List<AssetReferenceSoundtrackSong> rootFolder = new List<AssetReferenceSoundtrackSong>();
        public List<SoundtrackFolder> levelFolders = new List<SoundtrackFolder>();

        private GameObject playlistEditorPanel;
        private CyberGrindSettingsNavigator navigator;
        private GameObject loadingPrefab;
        private Sprite lockedLevelSprite;
        private Sprite defaultIcon;

        private FakeDirectoryTree<TrackReference> _baseDirectory =
            new FakeDirectoryTree<TrackReference>("", new TrackReference[] { });
        private FileDirectoryTree customSongsDirectory => new FileDirectoryTree(CustomSongsPath);
        protected override int maxPageLength => 4;
        protected override IDirectoryTree<TrackReference> baseDirectory => _baseDirectory;
        
        private TracksLoader tracksLoader;
        private EnhancedMusicPlaylistEditor playlistEditor;

        private void Awake()
        {
            CloneObsoleteInstance(
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

            _baseDirectory = Folder("Songs",
                children: new List<IDirectoryTree<TrackReference>>
                {
                    CustomSongsFolder(),
                    OstFolder()
                });
            tracksLoader = new TracksLoader(defaultIcon);

            // TODO: Not ideal, but will do for now
            SetPrivate(this, typeof(DirectoryTreeBrowser<TrackReference>), "currentDirectory", baseDirectory);
            OnInit?.Invoke();
        }

        private void OnEnable() => Rebuild();

        private IDirectoryTree<TrackReference> OstFolder()
        {
            return Folder("OST", children: levelFolders
                .Select(f => new FakeDirectoryTree<TrackReference>(f.name, f.songs
                    .Select(s => new TrackReference(SoundtrackType.Asset, s.AssetGUID))
                    .ToList()))
                .Cast<IDirectoryTree<TrackReference>>()
                .ToList());
        }

        private IDirectoryTree<TrackReference> CustomSongsFolder()
        {
            return TraverseFileTree(customSongsDirectory, "Custom tracks");
        }

        private void SelectSong(TrackReference reference, SoundtrackSong song)
        {
            if (song.clips.Count > 0)
            {
                var target = playlistEditor.PageOf(playlistEditor.Playlist.Count);
                playlistEditor.Playlist.AddTrack(reference, song);
                playlistEditor.Rebuild(false);
                playlistEditor.SetPage(target);
                navigator.GoToNoMenu(playlistEditorPanel);
            }
            else
                Debug.LogWarning("Attempted to add song with no clips to playlist.");
        }

        private void SelectSong(TrackReference reference, Playlist.SongData song)
        {
            if (song.clips.Count > 0)
            {
                var target = playlistEditor.PageOf(playlistEditor.Playlist.Count);
                playlistEditor.Playlist.AddTrack(reference, song);
                playlistEditor.Rebuild(false);
                playlistEditor.SetPage(target);
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
                    SoundtrackSong song = null;
                    yield return tracksLoader.LoadSongData(reference, data => song = data);

                    if (btn == null)
                        yield break;

                    Destroy(placeholder);
                    if (song != null)
                        DrawOstButton(song, reference, btn);

                    break;
                }
                case SoundtrackType.External:
                {
                    Playlist.SongData song = null;
                    var fileInfo = new FileInfo(Path.Combine(CustomSongsPath, reference.Reference));
                    yield return tracksLoader.LoadSongData(fileInfo, data => song = data);
                    Destroy(placeholder);

                    if (song != null)
                        DrawCustomTrackButton(song, reference, btn);
                    break;
                }
                default:
                    Debug.LogError(
                        $"[CybergrindMusicExplorer] Trying to load unknown soundtrack reference={reference.Reference}, type {reference.Type}, ignoring");
                    break;
            }
        }

        public void DrawOstButton(SoundtrackSong song, TrackReference reference, GameObject button)
        {
            var componentInChildren = button.GetComponentInChildren<CustomContentButton>();
            componentInChildren.button.onClick.RemoveAllListeners();
            if (song.conditions.AllMet())
            {
                componentInChildren.icon.sprite = song.icon != null ? song.icon : defaultIcon;
                componentInChildren.text.text =
                    song.songName.ToUpper() + " <color=grey>" + song.extraLevelBit + "</color>";
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

        public void DrawCustomTrackButton(Playlist.SongData song, TrackReference reference, GameObject button)
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
                .Where(HasSpecialPostfix)
                .GroupBy(file => WithoutPostfix(file).FullName)
                .Where(HasIntroAndLoop)
                .ToList();

            var regularTracks = tracks
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