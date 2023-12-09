using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CybergrindMusicExplorer.Scripts;
using CybergrindMusicExplorer.Scripts.Data;
using CybergrindMusicExplorer.Scripts.UI;

namespace CybergrindMusicExplorer.Downloader
{
    public class TracksDownloadManager : MonoSingleton<TracksDownloadManager>
    {
        private readonly List<AbstractDownloader> downloaders = new List<AbstractDownloader>
        {
            new YoutubeDownloader(),
            new SoundcloudDownloader()
        };

        public bool SupportsUrl(string url) => downloaders.Any(downloader => downloader.SupportsUrl(url));
        
        public IEnumerator GetMetadata(string url, Action<DownloadableTrackMetadata> callback, TracksDownloader downloader)
        {
            return downloaders
                .First(d => d.SupportsUrl(url))
                .GetTracksMetadataByUrl(url, callback, downloader);
        }

        public IEnumerator Download(DownloadableTrackEntry entry, TracksDownloader downloader)
        {
            return downloaders
                .First(d => d.SupportsUrl(entry.Metadata.Url))
                .Download(entry, downloader);
        }
        
        public List<IEnumerator> DownloadAll(List<DownloadableTrackEntry> tracks,
            TracksDownloader downloader)
        {
            return downloaders
                .First(d => d.SupportsUrl(tracks.First().Metadata.Url))
                .DownloadAll(tracks, downloader);
        }

        public void Cancel() => downloaders.ForEach(downloader => downloader.Cancel());
    }
}