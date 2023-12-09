using System;
using System.Collections;
using CybergrindMusicExplorer.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class DownloadableTrackEntry : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private GameObject statusBarGo;
        [SerializeField] public Image preview;
        public DownloadStatusBar statusBar;

        public DownloadableTrackEntryState State
        {
            get => state;
            set
            {
                state = value;
                if (statusBar.gameObject.activeInHierarchy)
                    UpdateStatusBar(value);
            }
        }

        public DownloadableTrackMetadata Metadata
        {
            get => metadata;
            set
            {
                metadata = value;
                titleText.text = value.Title;

                if (value.Cover == null)
                    return;
            
                var cover = value.Cover;
                var wh = cover.width - cover.height;
                var minDimension = Math.Min(cover.width, cover.height);
                preview.sprite = Sprite.Create((Texture2D)cover, new Rect(
                        (float)Math.Max(wh, 0) / 2,
                        (float)Math.Max(-wh, 0) / 2,
                        minDimension,
                        minDimension),
                    new Vector2(0.5f, 0.5f), 100.0f);
            }
        }

        private DownloadableTrackMetadata metadata;
        private DownloadableTrackEntryState state = DownloadableTrackEntryState.Idle;

        private void Awake()
        {
            statusBar = statusBarGo.GetComponent<DownloadStatusBar>();
        }
        
        private void OnEnable()
        {
            StartCoroutine(Enable());
        }

        private IEnumerator Enable()
        {
            yield return new WaitUntil(() => statusBarGo != null && statusBarGo.activeInHierarchy);
            UpdateStatusBar(state);
        }
        
        public void DownloadProgress(int percentage)
        {
            if (!statusBar.gameObject.activeInHierarchy)
                return;
            statusBar.Downloading();
            statusBar.progressBar.UpdateValue(percentage);
        }
        
        private void UpdateStatusBar(DownloadableTrackEntryState newState)
        {
            if (!statusBar.gameObject.activeInHierarchy)
                return;
            
            switch (newState)
            {
                case DownloadableTrackEntryState.Downloading:
                {
                    statusBar.Downloading();
                    break;
                }
                case DownloadableTrackEntryState.Enqueued:
                {
                    statusBar.Enqueued();
                    break;
                }
                case DownloadableTrackEntryState.Processing:
                {
                    statusBar.Processing();
                    break;
                }
                case DownloadableTrackEntryState.Downloaded:
                {
                    statusBar.Downloaded();
                    break;
                }
                case DownloadableTrackEntryState.Failed:
                {
                    statusBar.Failed();
                    break;
                }
            }
        }
    }
}