using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;
using static CybergrindMusicExplorer.Util.CustomTrackUtil;
using Object = UnityEngine.Object;

namespace CybergrindMusicExplorer.Components
{
    public class EnhancedMusicPlaylistEditor : DirectoryTreeBrowser<TrackReference>
    {
        private static readonly string UltrakillPath =
            Directory.GetParent(Application.dataPath)?.FullName ?? FallbackUltrakillPath;

        private const string FallbackUltrakillPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ULTRAKILL";
        private const string PanelCanvasPath = "/FirstRoom/Room/CyberGrindSettings/Canvas/Playlist/";

        private static readonly string PlaylistJsonPath =
            Path.Combine(UltrakillPath, "Preferences", "EnhancedPlaylist.json");

        private static readonly string CustomSongsPath = Path.Combine(UltrakillPath, "CyberGrind", "Music");
        
        private readonly CybergrindMusicExplorerManager musicExplorerManager =
            MonoSingleton<CybergrindMusicExplorerManager>.Instance;

        [SerializeField] private EnhancedMusicBrowser browser;
        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private Sprite loopSprite;
        [SerializeField] private Sprite loopOnceSprite;

        [Header("UI Elements")] [SerializeField]
        private Image loopModeImage;

        [SerializeField] private Image shuffleImage;
        [SerializeField] private RectTransform selectedControls;
        [SerializeField] private List<Transform> anchors;
        public CustomPlaylist customPlaylist = new CustomPlaylist();
        private Dictionary<Transform, Coroutine> changeAnchorRoutines = new Dictionary<Transform, Coroutine>();
        private List<Transform> buttons = new List<Transform>();

        protected override int maxPageLength => 4;

        protected override IDirectoryTree<TrackReference> baseDirectory =>
            new FakeDirectoryTree<TrackReference>("Songs", customPlaylist.References);

        private CustomContentButton currentButton => buttons.ElementAtOrDefault(customPlaylist.selected % maxPageLength)
            ?.GetComponent<CustomContentButton>();

        private void Awake()
        {
            SetPrivate(this, typeof(DirectoryTreeBrowser<TrackReference>), "currentDirectory", baseDirectory);
            CloneObsoleteInstance(
                FindObjectOfType<CustomMusicPlaylistEditor>(),
                this,
                privateFieldsToCopy: new List<string> { "plusButton", "backButton", "pageText", "cleanupActions" },
                fieldsToIgnore: new List<string> { "browser" });

            browser = FindObjectOfType<EnhancedMusicBrowser>();

            RewireButtonTo(ControlButton("Panel/NextButton"), NextPage);
            RewireButtonTo(ControlButton("Panel/PreviousButton"), PreviousPage);

            RewireButtonTo(ControlButton("Panel/Controls/DeleteButton/Remove"), Remove);
            RewireButtonTo(ControlButton("Panel/Controls/MoveButtons/Up"), MoveUp);
            RewireButtonTo(ControlButton("Panel/Controls/MoveButtons/Down"), MoveDown);

            RewireButtonTo(ControlButton("OrderControls/LoopButton"), ToggleLoopMode);
            RewireButtonTo(ControlButton("OrderControls/ShufflleButton"), ToggleShuffle);
        }

        private void Start()
        {
            try
            {
                LoadPlaylist();
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError("Error loading Playlist.json: '" + ex.Message + "'. Recreating file.");
                File.Delete(PlaylistJsonPath);
                LoadPlaylist();
            }

            Select(customPlaylist.selected);
            SetLoopMode(customPlaylist.loopMode);
            SetShuffle(customPlaylist.shuffled);
            customPlaylist.Changed += SavePlaylist;
        }

        private void OnDestroy() => customPlaylist.Changed -= SavePlaylist;

        public void SavePlaylist() => File.WriteAllText(PlaylistJsonPath, JsonConvert.SerializeObject(customPlaylist));

        public void LoadPlaylist()
        {
            Debug.Log("[CybergrindMusicExplorer] Loading Playlist");
            CustomPlaylist loadedPlaylist;

            using (var streamReader = new StreamReader(File.Open(PlaylistJsonPath, FileMode.OpenOrCreate)))
                loadedPlaylist = JsonConvert.DeserializeObject<CustomPlaylist>(streamReader.ReadToEnd());

            if (loadedPlaylist == null || loadedPlaylist.References == null || loadedPlaylist.References.Count == 0)
            {
                Debug.Log("[CybergrindMusicExplorer] No saved playlist found. Creating default...");
                foreach (var referenceSoundtrackSong in browser.rootFolder)
                {
                    var handle = referenceSoundtrackSong.LoadAssetAsync();
                    var song = handle.WaitForCompletion();
                    customPlaylist.AddTrack(new TrackReference(SoundtrackType.Asset, referenceSoundtrackSong.AssetGUID),
                        song);
                    Addressables.Release(handle);
                }
            }
            else
            {
                Debug.Log("[CybergrindMusicExplorer] Playlist exists");
                customPlaylist.loopMode = loadedPlaylist.loopMode;
                customPlaylist.selected = loadedPlaylist.selected;
                customPlaylist.shuffled = loadedPlaylist.shuffled;

                var outdatedTracks = new List<TrackReference>();
                foreach (var reference in loadedPlaylist.References)
                {
                    Debug.Log($"[CybergrindMusicExplorer] {reference.Reference}");
                    switch (reference.Type)
                    {
                        case SoundtrackType.Asset:
                        {
                            var handle = new AssetReferenceSoundtrackSong(reference.Reference).LoadAssetAsync();
                            handle.WaitForCompletion();
                            customPlaylist.AddTrack(reference, handle.Result);
                            Addressables.Release(handle);
                            Debug.Log($"[CybergrindMusicExplorer] Loaded soundtrack id={reference.Reference}");
                            break;
                        }
                        case SoundtrackType.External:
                        {
                            var fileInfo = new FileInfo(Path.Combine(CustomSongsPath, reference.Reference));
                            if (!fileInfo.Exists)
                            {
                                Debug.LogWarning(
                                    $"[CybergrindMusicExplorer] Track file {reference.Reference} doesn't exist, it will be removed.");
                                outdatedTracks.Add(reference);
                                break;
                            }

                            var metadata = CustomTrackMetadata.From(TagLib.File.Create(fileInfo.FullName));
                            var audioClip = LoadCustomSong(fileInfo);
                            
                            if (musicExplorerManager.NormalizeSoundtrack)
                                Normalize(audioClip);
                            
                            customPlaylist.AddTrack(reference,
                                SongDataFromCustomAudioClip(audioClip,
                                    metadata.Title ?? Path.GetFileNameWithoutExtension(fileInfo.Name), metadata.Artist,
                                    metadata.Logo ? metadata.Logo : defaultIcon));
                            Debug.Log($"[CybergrindMusicExplorer] Loaded custom music file={reference.Reference}");
                            break;
                        }
                        default:
                            Debug.LogError(
                                $"[CybergrindMusicExplorer] Reference type {reference.Type} is not supported for reference {reference.Reference}, ignoring");
                            break;
                    }
                }

                if (outdatedTracks.Count > 0)
                {
                    Debug.Log(
                        $"[CybergrindMusicExplorer] Found {outdatedTracks.Count} outdated tracks, removing them from the playlist");
                    outdatedTracks.ForEach(reference => loadedPlaylist.Remove(reference));
                    SavePlaylist();
                }

                // TODO: Bad Idea
                SetPrivate(this, typeof(DirectoryTreeBrowser<TrackReference>), "currentDirectory", baseDirectory);
            }

            Rebuild();
        }

        public void Remove()
        {
            customPlaylist.Remove(customPlaylist.selected);
            if (customPlaylist.selected >= customPlaylist.Count)
                Select(customPlaylist.Count - 1);

            Rebuild(false);
        }

        public void Move(int amount)
        {
            int index1 = customPlaylist.selected % maxPageLength;
            int index2 = index1 + amount;
            bool flag = PageOf(customPlaylist.selected) == PageOf(customPlaylist.selected + amount);
            if (customPlaylist.selected + amount < 0 || customPlaylist.selected + amount >= customPlaylist.Count)
                return;
            customPlaylist.Swap(customPlaylist.selected, customPlaylist.selected + amount);
            if (flag)
            {
                ChangeAnchorOf(buttons[index1], anchors[index2]);
                ChangeAnchorOf(selectedControls, anchors[index2]);
                ChangeAnchorOf(buttons[index2], anchors[index1]);
                CustomContentButton cb = currentButton;
                buttons.RemoveAt(index1);
                buttons.Insert(index2, cb.transform);
                Select(customPlaylist.selected + amount, false);
            }
            else
            {
                selectedControls.gameObject.SetActive(false);
                Select(customPlaylist.selected + amount);
            }
        }

        public void MoveUp() => Move(-1);

        public void MoveDown() => Move(1);

        public void ToggleLoopMode() => SetLoopMode(customPlaylist.loopMode == Playlist.LoopMode.Loop
            ? Playlist.LoopMode.LoopOne
            : Playlist.LoopMode.Loop);

        private void SetLoopMode(Playlist.LoopMode mode)
        {
            customPlaylist.loopMode = mode;
            loopModeImage.sprite = customPlaylist.loopMode == Playlist.LoopMode.Loop ? loopSprite : loopOnceSprite;
        }

        public void ToggleShuffle() => SetShuffle(!customPlaylist.shuffled);

        private void SetShuffle(bool shuffle)
        {
            customPlaylist.shuffled = shuffle;
            shuffleImage.color = shuffle ? Color.white : Color.gray;
        }

        private void Select(int newIndex, bool rebuild = true)
        {
            if (newIndex < 0 || newIndex >= customPlaylist.Count)
            {
                Debug.LogWarning(
                    $"Attempted to set current index {newIndex} outside bounds of playlist {customPlaylist.Count}");
            }
            else
            {
                int num = PageOf(newIndex) == currentPage ? 1 : 0;
                if ((bool)(Object)currentButton)
                    currentButton.border.color = Color.white;
                int selected = customPlaylist.selected;
                customPlaylist.selected = newIndex;
                if (PageOf(selected) < PageOf(newIndex))
                    ChangeAnchorOf((Transform)selectedControls, anchors.First(), 0.0f);
                else if (PageOf(selected) > PageOf(newIndex))
                    ChangeAnchorOf(selectedControls, anchors.Last(), 0.0f);
                if ((bool)(Object)currentButton)
                    currentButton.border.color = Color.green;
                Transform anchor = anchors[customPlaylist.selected % maxPageLength];
                if (num != 0)
                {
                    selectedControls.gameObject.SetActive(true);
                    ChangeAnchorOf(selectedControls, anchor);
                }
                else
                {
                    selectedControls.gameObject.SetActive(false);
                    selectedControls.transform.position = anchor.position;
                }

                if (!rebuild)
                    return;
                Rebuild(false);
            }
        }

        protected override Action BuildLeaf(TrackReference reference, int currentIndex)
        {
            if (!customPlaylist.IsSongLoaded(reference))
            {
                Debug.LogWarning("Attempted to load playlist UI while song with ID '" + reference.Reference +
                                 $"', type {reference.Type} has not loaded. Ignoring...");
                return null;
            }

            Playlist.SongData data;
            customPlaylist.GetSongData(reference, out data);

            GameObject go = Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
            CustomContentButton contentButton = go.GetComponent<CustomContentButton>();
            contentButton.text.text = data.name.ToUpper();
            contentButton.icon.sprite = data.icon != null ? data.icon : defaultIcon;
            go.SetActive(true);
            ChangeAnchorOf(go.transform, anchors[currentIndex], 0.0f);
            buttons.Add(go.transform);
            if (PageOf(customPlaylist.selected) == currentPage && contentButton == currentButton)
            {
                contentButton.border.color = Color.green;
                selectedControls.gameObject.SetActive(true);
                ChangeAnchorOf(selectedControls, anchors[currentIndex]);
                return () =>
                {
                    selectedControls.gameObject.SetActive(false);
                    Destroy(go);
                };
            }

            contentButton.button.onClick.AddListener(() =>
                Select(buttons.IndexOf(contentButton.transform) + currentPage * maxPageLength));
            return () => Destroy(go);
        }

        public void ChangeAnchorOf(Transform obj, Transform anchor, float time = 0.15f)
        {
            if (changeAnchorRoutines.ContainsKey(obj))
            {
                if (changeAnchorRoutines[obj] != null)
                    StopCoroutine(changeAnchorRoutines[obj]);
                changeAnchorRoutines.Remove(obj);
            }

            changeAnchorRoutines.Add(obj, StartCoroutine(ChangeAnchorOverTime()));

            IEnumerator ChangeAnchorOverTime()
            {
                float t = 0.0f;
                while (t < (double)time && time > 0.0)
                {
                    obj.position = Vector3.MoveTowards(obj.position, anchor.position, Time.deltaTime * 2f);
                    if (Vector3.Distance(obj.position, anchor.position) > (double)Mathf.Epsilon)
                        yield return null;
                    else
                        break;
                }

                obj.position = anchor.position;
            }
        }

        private Button ControlButton(string path)
        {
            return GameObject.Find(PanelCanvasPath + path).GetComponent<Button>();
        }

        private void RewireButtonTo(Button button, UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }

        public override void Rebuild(bool setToPageZero = true)
        {
            foreach (KeyValuePair<Transform, Coroutine> changeAnchorRoutine in changeAnchorRoutines)
            {
                if (changeAnchorRoutine.Value != null)
                    StopCoroutine(changeAnchorRoutine.Value);
            }

            changeAnchorRoutines.Clear();
            buttons.Clear();
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
            base.Rebuild(setToPageZero);
        }

        private static AudioClip LoadCustomSong(FileInfo fileInfo)
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip(new Uri(fileInfo.FullName), AudioType.MPEG))
            {
                var request = uwr.SendWebRequest();

                // Fucking genius sync await. Re-do this shit.
                while (!request.isDone)
                {
                }

                return DownloadHandlerAudioClip.GetContent(uwr);
            }
        }
    }
}