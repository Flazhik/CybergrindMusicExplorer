using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CybergrindMusicExplorer.Data;
using Newtonsoft.Json;
using SubtitlesParser.Classes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;
using static CybergrindMusicExplorer.Util.PathsUtils;
using Object = UnityEngine.Object;

namespace CybergrindMusicExplorer.Components
{
    public class EnhancedMusicPlaylistEditor : DirectoryTreeBrowser<TrackReference>
    {
        private const string PanelCanvasPath = "/FirstRoom/Room/CyberGrindSettings/Canvas/Playlist/";
        private readonly CybergrindMusicExplorerManager manager = CybergrindMusicExplorerManager.Instance;
        
        private readonly Dictionary<Transform, Coroutine> changeAnchorRoutines = new Dictionary<Transform, Coroutine>();
        private readonly List<Transform> buttons = new List<Transform>();

        public readonly CustomPlaylist Playlist = new CustomPlaylist();
        private EnhancedMusicBrowser browser;
        private TracksLoader tracksLoader;

        private Image loopModeImage;
        private Sprite defaultIcon;
        private Sprite loopSprite;
        private Sprite loopOnceSprite;

        private Image shuffleImage;
        private RectTransform selectedControls;
        private List<Transform> anchors;

        protected override int maxPageLength => 4;

        protected override IDirectoryTree<TrackReference> baseDirectory =>
            new FakeDirectoryTree<TrackReference>("Songs", Playlist.References);

        private CustomContentButton CurrentButton => buttons.ElementAtOrDefault(Playlist.selected % maxPageLength)
            ?.GetComponent<CustomContentButton>();
        private GameObject clearButton;

        private void Awake()
        {
            SetCurrentDirectory();
            CloneInstance(
                FindObjectOfType<CustomMusicPlaylistEditor>(),
                this,
                privateFieldsToCopy: new List<string>
                {
                    "plusButton", "backButton", "pageText", "cleanupActions"
                },
                fieldsToIgnore: new List<string>
                {
                    "browser"
                });

            browser = FindObjectOfType<EnhancedMusicBrowser>();
            tracksLoader = new TracksLoader(defaultIcon);

            RewireButtonTo(ControlButton("Panel/NextButton"), NextPage);
            RewireButtonTo(ControlButton("Panel/PreviousButton"), PreviousPage);

            RewireButtonTo(ControlButton("Panel/Controls/DeleteButton/Remove"), Remove);
            RewireButtonTo(ControlButton("Panel/Controls/MoveButtons/Up"), MoveUp);
            RewireButtonTo(ControlButton("Panel/Controls/MoveButtons/Down"), MoveDown);

            RewireButtonTo(ControlButton("OrderControls/LoopButton"), ToggleLoopMode);
            RewireButtonTo(ControlButton("OrderControls/ShufflleButton"), ToggleShuffle);
            InstantiateClearButton();
        }

        private void Start()
        {
            try
            {
                StartCoroutine(LoadPlaylist());
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError("Error loading Playlist.json: '" + ex.Message + "'. Recreating file.");
                File.Delete(PlaylistJsonPath);
                StartCoroutine(LoadPlaylist());
            }

            Select(Playlist.selected);
            SetLoopMode(Playlist.loopMode);
            SetShuffle(Playlist.shuffled);
            Playlist.Changed += SavePlaylist;
        }

        private void OnDestroy() => Playlist.Changed -= SavePlaylist;

        public void SavePlaylist() => File.WriteAllText(PlaylistJsonPath, JsonConvert.SerializeObject(Playlist));

        public IEnumerator LoadPlaylist()
        {
            Debug.Log("Loading Playlist");
            CustomPlaylist loadedPlaylist;

            using (var streamReader = new StreamReader(File.Open(PlaylistJsonPath, FileMode.OpenOrCreate)))
                loadedPlaylist = JsonConvert.DeserializeObject<CustomPlaylist>(streamReader.ReadToEnd());

            if (loadedPlaylist?.References == null || loadedPlaylist.References.Count == 0)
            {
                ResetPlaylist();
            }
            else
            {
                Debug.Log("Playlist exists");
                Playlist.loopMode = loadedPlaylist.loopMode;
                Playlist.selected = loadedPlaylist.selected;
                Playlist.shuffled = loadedPlaylist.shuffled;

                var outdatedTracks = new List<TrackReference>();
                foreach (var reference in loadedPlaylist.References)
                {
                    Debug.Log($"Loading track {reference.Reference}");
                    CustomSongData songData = null;
                    
                    if (manager.PreventDuplicateTracks && Playlist.References.Contains(reference))
                        continue;
                    
                    switch (reference.Type)
                    {
                        case SoundtrackType.Asset:
                        {
                            yield return tracksLoader.LoadSongData(reference, data => songData = data);
                            Debug.Log($"Loaded soundtrack id={reference.Reference}");
                            break;
                        }
                        case SoundtrackType.External:
                        {
                            var fileInfo = new FileInfo(Path.Combine(CustomSongsPath, reference.Reference));
                            List<List<SubtitleItem>> subtitles = default;
                            yield return tracksLoader.LoadSongData(fileInfo, data => songData = data, subs => subtitles = subs);

                            if (songData == null)
                            {
                                Debug.LogWarning(
                                    $"Track file {reference.Reference} doesn't exist, it will be removed.");
                                outdatedTracks.Add(reference);
                                break;
                            }

                            if (subtitles != default && subtitles.Count != 0)
                                Playlist.AddSubtitles(reference, subtitles);

                            Debug.Log($"Loaded soundtrack filename={reference.Reference}");
                            break;
                        }
                        default:
                            Debug.LogError(
                                $"Reference type {reference.Type} is not supported for reference {reference.Reference}, ignoring");
                            yield break;
                    }
                    
                    Playlist.AddTrack(reference, songData);
                }

                if (outdatedTracks.Count > 0)
                {
                    Debug.Log(
                        $"Found {outdatedTracks.Count} outdated tracks, removing them from the playlist");
                    outdatedTracks.ForEach(reference => loadedPlaylist.Remove(reference));
                    SavePlaylist();
                }
                SetPrivate(this, typeof(DirectoryTreeBrowser<TrackReference>), "currentDirectory", baseDirectory);
            }

            Rebuild();
        }

        public void Remove()
        {
            Playlist.Remove(Playlist.selected);
            if (Playlist.selected >= Playlist.Count)
                Select(Playlist.Count - 1);

            Rebuild(false);
        }

        public void Move(int amount)
        {
            var index1 = Playlist.selected % maxPageLength;
            var index2 = index1 + amount;
            var flag = PageOf(Playlist.selected) == PageOf(Playlist.selected + amount);
            if (Playlist.selected + amount < 0 || Playlist.selected + amount >= Playlist.Count)
                return;

            Playlist.Swap(Playlist.selected, Playlist.selected + amount);
            if (flag)
            {
                ChangeAnchorOf(buttons[index1], anchors[index2]);
                ChangeAnchorOf(selectedControls, anchors[index2]);
                ChangeAnchorOf(buttons[index2], anchors[index1]);
                var cb = CurrentButton;
                buttons.RemoveAt(index1);
                buttons.Insert(index2, cb.transform);
                Select(Playlist.selected + amount, false);
            }
            else
            {
                selectedControls.gameObject.SetActive(false);
                Select(Playlist.selected + amount);
            }
        }

        public void MoveUp() => Move(-1);

        public void MoveDown() => Move(1);

        public void ToggleLoopMode() => SetLoopMode(Playlist.loopMode == global::Playlist.LoopMode.Loop
            ? global::Playlist.LoopMode.LoopOne
            : global::Playlist.LoopMode.Loop);
        
        public void RemoveDuplicates()
        {
            Playlist.References = Playlist.References.Distinct().ToList();
            baseDirectory.Refresh();
            SetCurrentDirectory();
            SavePlaylist();
            Rebuild();
        }

        private void SetLoopMode(Playlist.LoopMode mode)
        {
            Playlist.loopMode = mode;
            loopModeImage.sprite = Playlist.loopMode == global::Playlist.LoopMode.Loop
                ? loopSprite
                : loopOnceSprite;

            SavePlaylist();
        }

        public void ToggleShuffle() => SetShuffle(!Playlist.shuffled);

        private void SetShuffle(bool shuffle)
        {
            Playlist.shuffled = shuffle;
            shuffleImage.color = shuffle ? Color.white : Color.gray;
            SavePlaylist();
        }

        private void Select(int newIndex, bool rebuild = true)
        {
            if (newIndex < 0 || newIndex >= Playlist.Count)
            {
                Debug.LogWarning(
                    $"Attempted to set current index {newIndex} outside bounds of playlist {Playlist.Count}");
            }
            else
            {
                var num = PageOf(newIndex) == currentPage ? 1 : 0;
                if ((bool)(Object)CurrentButton)
                    CurrentButton.border.color = Color.white;
                var selected = Playlist.selected;
                Playlist.selected = newIndex;
                if (PageOf(selected) < PageOf(newIndex))
                    ChangeAnchorOf(selectedControls, anchors.First(), 0.0f);
                else if (PageOf(selected) > PageOf(newIndex))
                    ChangeAnchorOf(selectedControls, anchors.Last(), 0.0f);

                if ((bool)(Object)CurrentButton)
                    CurrentButton.border.color = Color.green;
                var anchor = anchors[Playlist.selected % maxPageLength];

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
            if (!Playlist.IsSongLoaded(reference))
            {
                Debug.LogWarning("Attempted to load playlist UI while song with ID '" + reference.Reference +
                                 $"', type {reference.Type} has not loaded. Ignoring...");
                return null;
            }

            CustomSongData data;
            Playlist.GetSongData(reference, out data);
            
            var go = Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
            var contentButton = go.GetComponent<CustomContentButton>();
            contentButton.text.text = data.name.ToUpper();
            contentButton.icon.sprite = data.icon != null ? data.icon : defaultIcon;
            go.SetActive(true);
            ChangeAnchorOf(go.transform, anchors[currentIndex], 0.0f);
            buttons.Add(go.transform);
            if (PageOf(Playlist.selected) == currentPage && contentButton == CurrentButton)
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
                var t = 0.0f;
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

        public override void Rebuild(bool setToPageZero = true)
        {
            foreach (var changeAnchorRoutine in changeAnchorRoutines.Where(changeAnchorRoutine =>
                         changeAnchorRoutine.Value != null))
                StopCoroutine(changeAnchorRoutine.Value);

            changeAnchorRoutines.Clear();
            buttons.Clear();
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
            base.Rebuild(setToPageZero);
        }
        
        private void InstantiateClearButton()
        {
            clearButton = Instantiate(ControlButton("Panel/PreviousButton").gameObject,
                GameObject.Find($"{PanelCanvasPath}Panel").transform);
            clearButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 45f);
            clearButton.transform.localPosition = new Vector3(75f, 105f, -20f);

            var textComponent = clearButton.transform.Find("Text").GetComponent<Text>();
            textComponent.text = "CLEAR";
            textComponent.fontSize = 16;
            
            clearButton.name = "Clear";
            clearButton.GetComponent<Button>().onClick.RemoveAllListeners();
            clearButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ResetPlaylist();
                Rebuild();
            });
        }

        private void ResetPlaylist()
        {
            foreach (var referenceSoundtrackSong in browser.rootFolder)
            {
                var handle = referenceSoundtrackSong.LoadAssetAsync();
                var song = handle.WaitForCompletion();
                Playlist.RemoveAll();
                Playlist.AddOriginalTrack(new TrackReference(SoundtrackType.Asset, referenceSoundtrackSong.AssetGUID),
                    song);
                Addressables.Release(handle);
            }
        }

        private static Button ControlButton(string path)
        {
            return GameObject.Find(PanelCanvasPath + path).GetComponent<Button>();
        }

        private static void RewireButtonTo(Button button, UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }

        private void SetCurrentDirectory() =>
            SetPrivate(this, typeof(DirectoryTreeBrowser<TrackReference>), "currentDirectory", baseDirectory);
    }
}