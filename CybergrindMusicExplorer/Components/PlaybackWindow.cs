using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.Components
{
    public class PlaybackWindow : DirectoryTreeBrowser<Playlist.SongIdentifier>
    {
        public event Action<int> OnSongSelected;
        
        private CustomMusicPlaylistEditor playlistEditor;
        private IEnumerable<Playlist.SongIdentifier> order;
        private List<Transform> buttons = new List<Transform>();
        private Sprite defaultIcon;

        private int current;

        protected override IDirectoryTree<Playlist.SongIdentifier> baseDirectory =>
            new FakeDirectoryTree<Playlist.SongIdentifier>("Playlist", order);
        
        private CustomContentButton CurrentButton => buttons.ElementAtOrDefault(current % maxPageLength)
            ?.GetComponent<CustomContentButton>();
        
        protected override int maxPageLength => 8;

        private void Awake()
        {
            playlistEditor = CybergrindMusicExplorer.GetPlaylistEditor();
            CloneInstance(
                (DirectoryTreeBrowser<Playlist.SongIdentifier>)playlistEditor,
                (DirectoryTreeBrowser<Playlist.SongIdentifier>)this,
                fieldsToIgnore: new List<string>
                {
                    "baseDirectory",
                    "currentDirectory"
                });
            
            itemParent = gameObject.transform.Find("SongsSelector");
            SetPrivate(this, typeof(DirectoryTreeBrowser<Playlist.SongIdentifier>), "pageText", gameObject.transform.Find("Page").GetComponent<TMP_Text>());
            defaultIcon = (Sprite)GetPrivate(playlistEditor, typeof(CustomMusicPlaylistEditor), "defaultIcon");
            
            gameObject.transform.Find("Back").GetComponent<Button>().onClick.AddListener(PreviousPage);
            gameObject.transform.Find("Forward").GetComponent<Button>().onClick.AddListener(NextPage);
        }

        private void OnEnable()
        {
            GoToBase();
        }
        
        public void ChangeOrder(IEnumerable<Playlist.SongIdentifier> newOrder)
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

        protected override Action BuildLeaf(Playlist.SongIdentifier item, int currentIndex)
        {
            var data = playlistEditor.GetSongMetadata(item);

            var go = Instantiate(itemButtonTemplate, itemParent);
            var contentButton = go.GetComponent<CustomContentButton>();
            contentButton.text.text = data.displayName.ToUpper();
            contentButton.icon.sprite = data.icon != null ? data.icon : defaultIcon;
            go.SetActive(true);
            buttons.Add(go.transform);
            
            if (PageOf(current) == currentPage && contentButton == CurrentButton)
                contentButton.border.color = Color.green;
            
            contentButton.button.onClick.AddListener(() =>
            {
                current = currentPage * maxPageLength + currentIndex - 1;
                OnSongSelected?.Invoke(current);
            });
            return () => Destroy(go);
        }

        private void Select(int newIndex, bool rebuild = true)
        {
            if (newIndex < 0 || newIndex >= playlistEditor.playlist.Count)
            {
                Debug.LogWarning(
                    $"Attempted to set current index {newIndex} outside bounds of playlist {playlistEditor.playlist.Count}");
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