using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;
using static CybergrindMusicExplorer.Util.CustomTrackUtil;
using File = TagLib.File;

namespace CybergrindMusicExplorer.Components
{
    public class EnhancedMusicBrowser : DirectoryTreeBrowser<TrackReference>
    {
        private static readonly string UltrakillPath =
            Directory.GetParent(Application.dataPath)?.FullName ?? FallbackUltrakillPath;
        private const string FallbackUltrakillPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ULTRAKILL";
        private const string BaseCanvasPath = "/FirstRoom/Room/CyberGrindSettings/Canvas/SoundtrackMusic/Panel/";

        private static readonly string CustomSongsPath = Path.Combine(UltrakillPath, "CyberGrind", "Music");
        
        public static event Action OnInit;

        [Header("References")] [SerializeField]
        private EnhancedMusicPlaylistEditor playlistEditor;

        [SerializeField] private GameObject playlistEditorPanel;
        [SerializeField] private CyberGrindSettingsNavigator navigator;
        [Header("Assets")] [SerializeField] private GameObject loadingPrefab;
        [SerializeField] private Sprite lockedLevelSprite;
        [SerializeField] private Sprite defaultIcon;

        [SerializeField]
        public List<AssetReferenceSoundtrackSong> rootFolder = new List<AssetReferenceSoundtrackSong>();

        public List<SoundtrackFolder> levelFolders = new List<SoundtrackFolder>();

        private FakeDirectoryTree<TrackReference> _baseDirectory =
            new FakeDirectoryTree<TrackReference>("", new TrackReference[] { }, null, null);

        private FileDirectoryTree customSongsDirectory => new FileDirectoryTree(CustomSongsPath);

        private Dictionary<TrackReference, SoundtrackSong> referenceCache =
            new Dictionary<TrackReference, SoundtrackSong>();

        private Dictionary<TrackReference, Playlist.SongData> customReferenceCache =
            new Dictionary<TrackReference, Playlist.SongData>();

        protected override int maxPageLength => 4;

        protected override IDirectoryTree<TrackReference> baseDirectory => _baseDirectory;

        private void Awake()
        {
            CloneObsoleteInstance(
                FindObjectOfType<CustomMusicSoundtrackBrowser>(),
                this,
                privateFieldsToCopy: new List<string> { "plusButton", "backButton", "pageText", "cleanupActions" },
                fieldsToIgnore: new List<string> { "_baseDirectory", "referenceCache" });
            DestroyObsoleteInstance();

            RewireButtonTo(ControlButton("NextButton"), NextPage);
            RewireButtonTo(ControlButton("PreviousButton"), PreviousPage);
            RewireButtonTo(ControlButton("ImageSelectorWrapper/BackButton"), StepUp);

            _baseDirectory = Folder("Songs",
                children: new List<IDirectoryTree<TrackReference>> { CustomSongsFolder(), OstFolder() });

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
                    .ToList(), null, null))
                .Cast<IDirectoryTree<TrackReference>>()
                .ToList());
        }

        private IDirectoryTree<TrackReference> CustomSongsFolder()
        {
            var files = customSongsDirectory.GetFilesRecursive()
                .Where(file => file.Extension.ToLower() == ".mp3")
                .Select((file, i) => new TrackReference(SoundtrackType.External, file.Name))
                .ToList();

            return Folder("Custom tracks", files);
        }

        private void SelectSong(TrackReference reference, SoundtrackSong song)
        {
            if (song.clips.Count > 0)
            {
                int target = playlistEditor.PageOf(playlistEditor.customPlaylist.Count);
                playlistEditor.customPlaylist.AddTrack(reference, song);
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
                int target = playlistEditor.PageOf(playlistEditor.customPlaylist.Count);
                playlistEditor.customPlaylist.AddTrack(reference, song);
                playlistEditor.Rebuild(false);
                playlistEditor.SetPage(target);
                navigator.GoToNoMenu(playlistEditorPanel);
            }
            else
                Debug.LogWarning("Attempted to add song with no clips to playlist.");
        }

        public IEnumerator LoadSongButton(TrackReference reference, GameObject btn)
        {
            GameObject placeholder = Instantiate(loadingPrefab, itemParent, false);
            placeholder.SetActive(true);

            switch (reference.Type)
            {
                case SoundtrackType.Asset:
                {
                    SoundtrackSong song;
                    if (referenceCache.ContainsKey(reference))
                    {
                        Debug.LogWarning("[CybergrindMusicExplorer] No referenceCache present");
                        yield return new WaitUntil(() => referenceCache[reference] != null || btn == null);
                        if (btn == null)
                        {
                            Destroy(placeholder);
                            yield break;
                        }
                        else
                            song = referenceCache[reference];
                    }
                    else
                    {
                        AsyncOperationHandle<SoundtrackSong> handle =
                            new AssetReferenceT<SoundtrackSong>(reference.Reference).LoadAssetAsync();
                        referenceCache.Add(reference, null);
                        yield return new WaitUntil(() => handle.IsDone || btn == null);
                        if (btn == null)
                        {
                            Destroy(placeholder);
                            yield return handle;
                        }

                        song = handle.Result;
                        referenceCache[reference] = song;
                        Addressables.Release(handle);
                        if (btn == null)
                            yield break;
                    }

                    Destroy(placeholder);
                    DrawOstButton(song, reference, btn);
                    break;
                }
                case SoundtrackType.External:
                {
                    Playlist.SongData song;
                    if (customReferenceCache.ContainsKey(reference))
                    {
                        Debug.LogWarning("[CybergrindMusicExplorer] No referenceCache present");
                        yield return new WaitUntil(() => customReferenceCache[reference] != null || btn == null);
                        if (btn == null)
                        {
                            Destroy(placeholder);
                            yield break;
                        }
                        else
                            song = customReferenceCache[reference];
                    }
                    else
                    {
                        Debug.LogError($"[CybergrindMusicExplorer] {reference.Reference}");
                        var fileInfo = new FileInfo(Path.Combine(CustomSongsPath, reference.Reference));
                        var fullPath = fileInfo.FullName;

                        if (!fileInfo.Exists)
                        {
                            Debug.LogError(
                                $"[CybergrindMusicExplorer] Soundtrack file {reference.Reference} {fullPath} doesn't exist, ignoring");
                            Destroy(placeholder);
                            yield break;
                        }

                        AudioClip audioClip = null;
                        referenceCache.Add(reference, null);
                        yield return StartCoroutine(LoadCustomSong(fullPath, clip => audioClip = clip));

                        if (btn == null)
                        {
                            Destroy(placeholder);
                        }

                        var metadata = CustomTrackMetadata.From(File.Create(fileInfo.FullName));
                        song = SongDataFromCustomAudioClip(audioClip,
                            metadata.Title ?? Path.GetFileNameWithoutExtension(fileInfo.Name), metadata.Artist,
                            metadata.Logo ? metadata.Logo : defaultIcon);

                        customReferenceCache[reference] = song;
                        if (btn == null)
                            yield break;
                    }

                    Destroy(placeholder);
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
            CustomContentButton componentInChildren = button.GetComponentInChildren<CustomContentButton>();
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
            CustomContentButton componentInChildren = button.GetComponentInChildren<CustomContentButton>();
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
            GameObject btn = Instantiate(folderButtonTemplate, itemParent, false);
            btn.GetComponent<Button>().onClick.RemoveAllListeners();
            btn.GetComponent<Button>().onClick.AddListener(() => StepDown(folder));
            btn.GetComponentInChildren<Text>().text = folder.name.ToUpper();
            btn.SetActive(true);
            return () => Destroy(btn);
        }

        protected override Action BuildLeaf(TrackReference reference, int indexInPage)
        {
            GameObject btn = Instantiate(itemButtonTemplate, itemParent, false);
            StartCoroutine(LoadSongButton(reference, btn));
            return () => Destroy(btn);
        }


        private void RewireButtonTo(Button button, UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }

        private Button ControlButton(string path) => GameObject.Find(BaseCanvasPath + path).GetComponent<Button>();
        private void SetActiveAll(List<GameObject> objects, bool active) => objects.ForEach(o => o.SetActive(active));

        private void DestroyObsoleteInstance()
        {
            var oldPlaylistEditor = FindObjectOfType<CustomMusicPlaylistEditor>();
            var playlistEditorParentObject = oldPlaylistEditor.transform.gameObject;
            playlistEditor = playlistEditorParentObject.AddComponent<EnhancedMusicPlaylistEditor>();
            Destroy(oldPlaylistEditor);
        }
    }
}