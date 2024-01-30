using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CybergrindMusicExplorer.Scripts;
using CybergrindMusicExplorer.Scripts.Data;
using CybergrindMusicExplorer.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CybergrindMusicExplorer.Downloader
{
    public abstract class AbstractDownloader
    {
        protected CancellationTokenSource Cancellation = new CancellationTokenSource();

        protected AbstractDownloader()
        {
            SceneManager.activeSceneChanged += (s1, s2) => Cancel();
            Application.quitting += Cancel;
        }

        public abstract bool SupportsUrl(string url);
        
        public abstract IEnumerator GetTracksMetadataByUrl(string url, Action<DownloadableTrackMetadata> callback, TracksDownloader tracksDownloader);
        
        public abstract IEnumerator Download(DownloadableTrackEntry entry, TracksDownloader downloader);
        
        public List<IEnumerator> DownloadAll(IEnumerable<DownloadableTrackEntry> tracks, TracksDownloader downloader) => 
            tracks.Select(track => Download(track, downloader)).ToList();

        public void Cancel()
        {
            CancelAll();
            Cancellation.Cancel();
            Cancellation.Dispose();
            Cancellation = new CancellationTokenSource();
        }

        protected abstract void CancelAll();
        
        protected static void AddTrackToPlaylist(string path)
        {
            if (CybergrindMusicExplorerManager.Instance.AddDownloadedTracks)
                CybergrindMusicExplorer.GetPlaylistEditor().playlist.Add(new Playlist.SongIdentifier(path, Playlist.SongIdentifier.IdentifierType.File));
        }
        
        protected static void CreateFolderIfMissing(DirectoryInfo directory)
        {
            if (!directory.Exists)
                directory.Create();
        }

        protected static void RemoveFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
} 