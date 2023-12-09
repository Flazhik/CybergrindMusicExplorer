using System;
using System.Collections;
using System.Collections.Generic;
using CybergrindMusicExplorer.Scripts.Data;
using CybergrindMusicExplorer.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.Scripts
{
    public class TracksDownloader : MonoBehaviour
    {
        private const int PageCapacity = 20;
        public event Action<string> URLChanged;

        [SerializeField] public GameObject restartButton;
        [SerializeField] public GameObject downloadAllButton;
        [SerializeField] private GameObject urlField;
        [SerializeField] private GameObject loadingRing;
        [SerializeField] private GameObject list;
        [SerializeField] private GameObject trackEntryPrefab;
        [SerializeField] private GameObject paginator;
        [SerializeField] private GameObject easterEgg;
        [SerializeField] private Text errorField;

        private int currentPage;
        private int downloaded;
        private int failed;
        
        private readonly List<DownloadableTrackEntry> tracks = new List<DownloadableTrackEntry>();
        private readonly List<IEnumerator> downloadRoutines = new List<IEnumerator>();
        private Func<DownloadableTrackEntry, TracksDownloader, IEnumerator> downloadCallback;
        private Func<List<DownloadableTrackEntry>, TracksDownloader, List<IEnumerator>> downloadAllCallback;

        private void Start()
        {
            var inputField = urlField.GetComponent<InputField>();
            var delayedField = urlField.GetComponent<DelayedInputField>();
            var pagination = paginator.GetComponent<Paginator>();

            pagination.previousButton.GetComponent<Button>().onClick.AddListener(() => SetPage(currentPage - 1));
            pagination.nextButton.GetComponent<Button>().onClick.AddListener(() => SetPage(currentPage + 1));

            downloadAllButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                downloadAllButton.SetActive(false);
                ShowDownloadProgressMessage();
                downloadAllCallback.Invoke(tracks, this).ForEach(routine =>
                {
                    downloadRoutines.Add(routine);
                    StartCoroutine(routine);
                });
            });

            inputField.onValueChanged.AddListener(__ =>
            {
                errorField.text = string.Empty;
                loadingRing.SetActive(false);
                downloadRoutines.ForEach(StopCoroutine);
                downloadRoutines.Clear();
                downloaded = 0;
                failed = 0;
            });

            if (URLChanged != null)
                delayedField.ValueChanged += URLChanged;

            Rebuild();
        }

        public void SetDownloadCallback(Func<DownloadableTrackEntry, TracksDownloader, IEnumerator> callback) =>
            downloadCallback = callback;

        public void SetDownloadAllCallback(Func<List<DownloadableTrackEntry>,
            TracksDownloader,
            List<IEnumerator>> callback) =>
            downloadAllCallback = callback;

        public void LoadingStarted()
        {
            Clear();
            loadingRing.SetActive(true);
        }

        public void LoadingComplete()
        {
            loadingRing.SetActive(false);
        }

        public void ShowRestartButton()
        {
            restartButton.SetActive(true);
        }

        public void ShowDownloadAllButton()
        {
            downloadAllButton.SetActive(true);
        }

        public void HideDownloadAllButton()
        {
            downloadAllButton.SetActive(false);
        }

        public void DisplayMessage(string error)
        {
            loadingRing.SetActive(false);
            errorField.text = error;
        }

        public void ShowDownloadProgressMessage() =>
            DisplayMessage($"[<color=green>Downloaded</color>: {downloaded}/{tracks.Count}, <color=red>Failed</color>: {failed}/{tracks.Count}]");
        

        public void IncreaseDownloaded()
        {
            downloaded++;
            ShowDownloadProgressMessage();
        }
        
        public void IncreaseFailed()
        {
            downloaded++;
            ShowDownloadProgressMessage();
        }

        public void AddEntry(DownloadableTrackMetadata metadata)
        {
            if (metadata?.Title == null)
                return;
            
            var entry = Instantiate(trackEntryPrefab, list.transform).GetComponent<DownloadableTrackEntry>();
            entry.Metadata = metadata;

            tracks.Add(entry);
            Rebuild();

            entry.statusBar.downloadButton.GetComponent<Button>().onClick.AddListener(() => Download(entry));
            entry.statusBar.retryButton.GetComponent<Button>().onClick.AddListener(() => Download(entry));
        }

        public void SetPage(int page)
        {
            if ((tracks.Count > 0 && page == 0) || page > MaxPage())
                return;

            currentPage = page;
            Rebuild();
        }

        public void EasterEgg() => Instantiate(easterEgg, list.transform);
        
        private int MaxPage() => (int)Math.Ceiling((double)tracks.Count / PageCapacity);

        private void Rebuild()
        {
            currentPage = tracks.Count == 0
                ? 0
                : currentPage == 0
                    ? 1
                    : currentPage;

            for (var i = 0; i < tracks.Count; i++)
                tracks[i].gameObject.SetActive(i >= (currentPage - 1) * PageCapacity && i < currentPage * PageCapacity);

            paginator.GetComponent<Paginator>()
                .Refresh(currentPage, (int)Math.Ceiling((double)tracks.Count / PageCapacity));
        }

        private void Clear()
        {
            tracks.Clear();

            for (var i = 0; i < list.transform.childCount; ++i)
                Destroy(list.transform.GetChild(i).gameObject);

            Rebuild();
        }

        private Coroutine Download(DownloadableTrackEntry entry) =>
            StartCoroutine(downloadCallback.Invoke(entry, this));
    }
}