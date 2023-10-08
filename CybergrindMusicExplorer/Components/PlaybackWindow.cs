using System;
using System.Collections.Generic;
using System.Linq;
using CybergrindMusicExplorer.Data;
using UnityEngine;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.Components
{
    public class PlaybackWindow : DirectoryTreeBrowser<TrackReference>
    {
        public event Action<int> OnSongSelected;
        
        private EnhancedMusicPlaylistEditor playlistEditor;
        private List<TrackReference> order;
        private List<Transform> buttons = new List<Transform>();
        private Sprite defaultIcon;

        private int current;

        protected override IDirectoryTree<TrackReference> baseDirectory =>
            new FakeDirectoryTree<TrackReference>("Playlist", order);
        
        private CustomContentButton CurrentButton => buttons.ElementAtOrDefault(current % maxPageLength)
            ?.GetComponent<CustomContentButton>();
        
        protected override int maxPageLength => 8;

        private void Awake()
        {
            playlistEditor = CybergrindMusicExplorer.GetEnhancedPlaylistEditor();
            CloneInstance(
                (DirectoryTreeBrowser<TrackReference>)playlistEditor,
                (DirectoryTreeBrowser<TrackReference>)this,
                fieldsToIgnore: new List<string>
                {
                    "baseDirectory",
                    "currentDirectory"
                });
            
            itemParent = gameObject.transform.Find("SongsSelector");
            SetPrivate(this, typeof(DirectoryTreeBrowser<TrackReference>), "pageText", gameObject.transform.Find("Page").GetComponent<Text>());
            defaultIcon = (Sprite)GetPrivate(playlistEditor, typeof(EnhancedMusicPlaylistEditor), "defaultIcon");
            
            gameObject.transform.Find("Back").GetComponent<Button>().onClick.AddListener(PreviousPage);
            gameObject.transform.Find("Forward").GetComponent<Button>().onClick.AddListener(NextPage);
        }

        private void OnEnable()
        {
            GoToBase();
        }
        
        public void ChangeOrder(List<TrackReference> newOrder)
        {
            EnablePlaybackMenu();
            order = newOrder;
            if (gameObject.activeSelf)
                Rebuild();
        }

        public void EnablePlaybackMenu()
        {
            gameObject.transform.Find("Message").gameObject.SetActive(false);
            gameObject.transform.Find("Page").gameObject.SetActive(true);
        }

        public void ChangeCurrent(int index)
        {
            current = index;
            if (gameObject.activeSelf)
                Select(index);
        }

        protected override Action BuildLeaf(TrackReference item, int currentIndex)
        {
            CustomSongData data;
            playlistEditor.Playlist.GetSongData(item, out data);

            var go = Instantiate(itemButtonTemplate, itemParent);
            var contentButton = go.GetComponent<CustomContentButton>();
            contentButton.text.text = data.name.ToUpper();
            contentButton.icon.sprite = data.icon != null ? data.icon : defaultIcon;
            go.SetActive(true);
            buttons.Add(go.transform);
            if (PageOf(current) == currentPage && contentButton == CurrentButton)
            {
                contentButton.border.color = Color.green;
            }
            
            contentButton.button.onClick.AddListener(() =>
            {
                current = currentPage * maxPageLength + currentIndex - 1;
                OnSongSelected?.Invoke(current);
            });
            return () => Destroy(go);
        }

        private void Select(int newIndex, bool rebuild = true)
        {
            if (newIndex < 0 || newIndex >= playlistEditor.Playlist.Count)
            {
                Debug.LogWarning(
                    $"Attempted to set current index {newIndex} outside bounds of playlist {playlistEditor.Playlist.Count}");
            }
            else
            {
                var num = PageOf(newIndex) == currentPage ? 1 : 0;
                if ((bool)CurrentButton)
                    CurrentButton.border.color = Color.white;

                current = newIndex;
                
                if ((bool)CurrentButton)
                    CurrentButton.border.color = Color.green;

                if (!rebuild)
                    return;

                Rebuild(false);
            }
        }

        public override void Rebuild(bool setToPageZero = true)
        {
            buttons.Clear();
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
            base.Rebuild(setToPageZero);
        }
    }
}