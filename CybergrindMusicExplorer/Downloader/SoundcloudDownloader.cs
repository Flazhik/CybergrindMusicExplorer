using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CybergrindMusicExplorer.Downloader.Data.Soundcloud;
using CybergrindMusicExplorer.Scripts;
using CybergrindMusicExplorer.Scripts.Data;
using CybergrindMusicExplorer.Scripts.UI;
using CybergrindMusicExplorer.Util;
using UnityEngine;
using UnityEngine.Networking;
using Xabe.FFmpeg;
using static CybergrindMusicExplorer.Util.HttpUtils;
using static Xabe.FFmpeg.FFmpeg;

namespace CybergrindMusicExplorer.Downloader
{
    public class SoundcloudDownloader : AbstractDownloader
    {
        private const string SoundCloudBaseUrl = "https://api-v2.soundcloud.com/resolve";
        private const string SoundCloudPlaylistUrl = "https://api-v2.soundcloud.com/tracks";
        private const string ClientId = "a3e059563d7fd3372b49b37f00a00bcf";
        private const string SoundCloudTrackRegex = "^(?:https:\\/\\/)?(?:(?:www)\\.)?soundcloud\\..+?\\/(.*?)\\/[a-zA-Z0-9~@#$^*()_+=[\\]{}|\\\\,.?: -]+";
        private const string SoundCloudPlaylistRegex =
            "^(?:https:\\/\\/)?(?:(?:www)\\.)?soundcloud\\..+?\\/(.*?)\\/sets\\/[a-zA-Z]+";
        private static readonly DirectoryInfo SoundCloudDirPath = new DirectoryInfo(Path.Combine(PathsUtils.CustomSongsPath, "SoundCloud"));
        
        private readonly Queue<IEnumerator> downloadQueue = new Queue<IEnumerator>();
        private readonly Queue<IEnumerator> metadataQueue = new Queue<IEnumerator>();
        private readonly Queue<IEnumerator> urlQueue = new Queue<IEnumerator>();
        private readonly QueueManager processingQueueManager = new QueueManager();

        public override bool SupportsUrl(string url) => UrlIsSingleTrack(url) || UrlIsPlaylist(url);

        public override IEnumerator GetTracksMetadataByUrl(string url,
            Action<DownloadableTrackMetadata> callback,
            TracksDownloader downloader)
        {
            Texture cover = null;
            if (UrlIsPlaylist(url))
            {
                SoundcloudPlaylistResponse playlistMetadata = null;
                yield return GetMetadata<SoundcloudPlaylistResponse>(url, m => playlistMetadata = m);
                if (playlistMetadata?.Tracks == null || playlistMetadata.Tracks.Count == 0)
                {
                    downloader.DisplayMessage("No tracks found for this URL");
                    yield break;
                }
                
                var tracksIds = GetTracksIds(playlistMetadata.Tracks);
                var playlist = new List<SoundcloudTrackResponse>();
                var offset = 0;
                
                foreach (var trackIdsChunk in tracksIds)
                {
                    yield return GetRequest<List<SoundcloudTrackResponse>>(
                        $"{SoundCloudPlaylistUrl}?ids={trackIdsChunk}&client_id={ClientId}&limit=50&offset={offset}", p => playlist.AddRange(p));
                    offset += 50;
                }

                foreach (var playlistEntry in playlist
                             .TakeWhile(playlistEntry => playlistEntry.GetDownloadUrl() != null))
                {
                    cover = null;
                    if (playlistEntry.ArtworkUrl != null)
                        yield return LoadTexture(playlistEntry.ArtworkUrl, c => cover = c);
                    
                    callback.Invoke(new DownloadableTrackMetadata(playlistEntry.Title, cover, playlistEntry.Permalink));
                }
                
                downloader.ShowDownloadAllButton();
                yield break;
            }
            
            SoundcloudTrackResponse metadata = null;
            yield return GetMetadata<SoundcloudTrackResponse>(url, m => metadata = m);

            if (metadata?.Title == null)
            {
                downloader.DisplayMessage("No video found for this URL");
                yield break;
            }
            
            if (metadata?.ArtworkUrl != null)
                yield return LoadTexture(metadata.ArtworkUrl, c => cover = c);

            callback.Invoke(new DownloadableTrackMetadata(metadata.Title, cover, url));
        }

        public override IEnumerator Download(DownloadableTrackEntry entry, TracksDownloader downloader)
        {
            entry.State = DownloadableTrackEntryState.Downloading;
            
            SoundcloudTrackResponse metadata = default;
            var metadataRoutine = GetMetadata<SoundcloudTrackResponse>(entry.Metadata.Url, m => metadata = m);
            metadataQueue.Enqueue(metadataRoutine);
            yield return new WaitUntil(() => metadataQueue.Peek() == metadataRoutine);
            yield return metadataRoutine;
            metadataQueue.Dequeue();

            if (metadata?.GetDownloadUrl() == null)
            {
                entry.State = DownloadableTrackEntryState.Failed;
                downloader.IncreaseFailed();
                yield break;
            }
            
            var sanitizedTitle = PathsUtils.CoerceValidFileName(entry.Metadata.Title);
            var trackFile = Path.Combine(PathsUtils.TemporaryFilesDirectory.FullName, $"{sanitizedTitle}.mp3");
            var coverFile = Path.Combine(PathsUtils.TemporaryFilesDirectory.FullName, $"{sanitizedTitle}.jpg");

            string downloadUrl = null;
            yield return GetRequest<SoundtrackDownloadUrlResponse>($"{metadata?.GetDownloadUrl()}?client_id={ClientId}",
                r => downloadUrl = r?.Url);
            
            var urlRoutine = GetRequest<SoundtrackDownloadUrlResponse>($"{metadata?.GetDownloadUrl()}?client_id={ClientId}",
                r => downloadUrl = r?.Url);
            urlQueue.Enqueue(urlRoutine);
            yield return new WaitUntil(() => urlQueue.Peek() == urlRoutine);
            yield return urlRoutine;
            urlQueue.Dequeue();

            if (downloadUrl == null) {
                entry.State = DownloadableTrackEntryState.Failed;
                downloader.IncreaseFailed();
                yield break;
            }

            CreateFolderIfMissing(PathsUtils.TemporaryFilesDirectory);
            CreateFolderIfMissing(SoundCloudDirPath);

            var webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                if (entry != null)
                    entry.DownloadProgress(e.ProgressPercentage);
            };

            var trackDownloaded = false;
            var downloadRoutine = DownloadFile(webClient, downloadUrl, trackFile, s => trackDownloaded = s);;
            downloadQueue.Enqueue(downloadRoutine);
            yield return new WaitUntil(() => downloadQueue.Peek() == downloadRoutine);
            yield return downloadRoutine;
            downloadQueue.Dequeue();
            
            entry.State = DownloadableTrackEntryState.Enqueued;
            
            if (metadata.ArtworkUrl != null)
                yield return DownloadFile(webClient, metadata.ArtworkUrl, coverFile, s => { });

            if (!trackDownloaded)
            {
                entry.State = DownloadableTrackEntryState.Failed;
                downloader.IncreaseFailed();
                yield break;
            }

            var conversionTask = CovertTrack(trackFile, coverFile, metadata, Cancellation.Token);
            var conversion = processingQueueManager.Enqueue(() =>
            {
                entry.State = DownloadableTrackEntryState.Processing;
                return conversionTask;
            });

            yield return new WaitUntil(() => conversion.IsCompleted || conversion.IsFaulted);
            RemoveFile(trackFile);
            RemoveFile(coverFile);

            if (!conversion.IsFaulted && !conversion.IsCanceled)
            {
                downloader.ShowRestartButton();
                downloader.IncreaseDownloaded();
                entry.State = DownloadableTrackEntryState.Downloaded;
                AddTrackToPlaylist(Path.Combine(SoundCloudDirPath.FullName, $"{Path.GetFileNameWithoutExtension(trackFile)}.mp3"));
            }
            else
            {
                entry.State = DownloadableTrackEntryState.Failed;
                downloader.IncreaseFailed();
            }
        }
        
        protected override void CancelAll()
        {
            downloadQueue.Clear();
            metadataQueue.Clear();
            urlQueue.Clear();
        }

        private static async Task CovertTrack(string trackFile,
            string coverFile,
            SoundcloudTrackResponse metadata,
            CancellationToken token)
        {
            var outputFile = Path.Combine(SoundCloudDirPath.FullName, $"{Path.GetFileNameWithoutExtension(trackFile)}.mp3");
            SetExecutablesPath(PathsUtils.CgmePath);
            await ConvertToAudio(trackFile, coverFile, outputFile, metadata, token);
        }

        private static IEnumerator GetMetadata<T>(string url, Action<T> callback)
        {
            T metadata = default;
            yield return GetRequest<T>
                ($"{SoundCloudBaseUrl}?client_id={ClientId}&url={UnityWebRequest.EscapeURL(url)}", m => metadata = m);

            if (metadata != null)
                callback.Invoke(metadata);
        }

        private static async Task ConvertToAudio(string trackFile,
            string coverFile,
            string output,
            SoundcloudTrackResponse metadata,
            CancellationToken token)
        {
            IMediaInfo coverInfo = null;
            var trackInfo = await GetMediaInfo(trackFile, token);
            
            if (File.Exists(coverFile))
                coverInfo = await GetMediaInfo(coverFile, token);
            
            var audioStream = trackInfo.AudioStreams.FirstOrDefault();
            var conversion = FFmpeg.Conversions.New().AddStream(audioStream);

            if (coverInfo != null)
                conversion.AddStream(coverInfo.VideoStreams.First().SetCodec("mjpeg"));
            
            conversion.SetAudioBitrate(128000)
                .AddParameter("-threads 0")
                .SetOverwriteOutput(true)
                .SetOutput(output);

            if (metadata?.Publisher?.Artist != null)
                conversion.AddParameter($"-metadata artist=\"{metadata.Publisher.Artist}\"");

            await conversion.Start(token);
        }

        private static List<string> GetTracksIds(IEnumerable<SoundcloudPlaylistResponse.TrackEntry> trackEntries)
        {
            var tracksIds = trackEntries
                .Where(m => m.Policy.Equals("ALLOW"))
                .Select(m => m.Id)
                .Select((e, i) => new { Index = i, Value = e })
                .GroupBy(e => e.Index / 50)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            return tracksIds.Select(chunk => string.Join(",", chunk)).ToList();
        }
        
        private static bool UrlIsSingleTrack(string url) =>
            Regex.IsMatch(url, SoundCloudTrackRegex, RegexOptions.IgnoreCase);

        private static bool UrlIsPlaylist(string url) =>
            Regex.IsMatch(url, SoundCloudPlaylistRegex, RegexOptions.IgnoreCase) && !url.Contains("in=");
    }
}