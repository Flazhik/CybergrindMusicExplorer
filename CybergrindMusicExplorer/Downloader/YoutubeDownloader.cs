using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CybergrindMusicExplorer.Downloader.Data.Youtube;
using CybergrindMusicExplorer.Scripts;
using CybergrindMusicExplorer.Scripts.Data;
using CybergrindMusicExplorer.Scripts.UI;
using CybergrindMusicExplorer.Util;
using UnityEngine;
using Xabe.FFmpeg;
using static CybergrindMusicExplorer.Util.HttpUtils;
using static Xabe.FFmpeg.FFmpeg;

namespace CybergrindMusicExplorer.Downloader
{
    public class YoutubeDownloader : AbstractDownloader
    {
        private const string YoutubeBrowseUrl = "https://www.youtube.com/youtubei/v1/browse";
        private const string YoutubePlayerUrl = "https://www.youtube.com/youtubei/v1/player";
        private const string YouTubeVideoRegex =
            "^(?:https:\\/\\/)?(?:(?:www|m)\\.)?(?:youtube\\.com|youtu.be)(?:\\/(?:[\\w\\-]+\\?v=|embed\\/|v\\/)?)([\\w\\-]+)(\\S+)?$";
        private const string YouTubePlaylistRegex =
            "^(?:https:\\/\\/)?(?:(?:www|m)\\.)?(?:youtube\\.com|youtu.be).*?list=([a-zA-Z0-9\\-_]*).*(?:&|$)$";

        private const string RickRollId = "dQw4w9WgXcQ";

        private readonly QueueManager queueManager = new QueueManager();
        private readonly Queue<IEnumerator> downloadQueue = new Queue<IEnumerator>();
        private readonly Queue<IEnumerator> metadataQueue = new Queue<IEnumerator>();
        private static readonly DirectoryInfo YouTubeDirPath = new DirectoryInfo(Path.Combine(PathsUtils.CustomSongsPath, "YouTube"));

        public override bool SupportsUrl(string url) => UrlIsSingleTrack(url) || UrlIsPlaylist(url);

        public override IEnumerator GetTracksMetadataByUrl(string url,
            Action<DownloadableTrackMetadata> callback,
            TracksDownloader downloader)
        {
            Texture cover = null;
            
            if (UrlIsPlaylist(url))
            {
                YoutubePlaylistResponse playlist = null;
                yield return GetMetadataForPlaylist(url, p => playlist = p);

                if (playlist?.Entries == null || playlist.Entries.Count == 0)
                {
                    downloader.DisplayMessage("No videos found for this URL");
                    yield break;
                }

                foreach (var playlistEntry in playlist.Entries
                             .Where(playlistEntry => playlistEntry.GetTitle() != null))
                {
                    cover = default;
                    if (playlistEntry.GetThumbnail() != null)
                        yield return LoadTexture(playlistEntry.GetThumbnail(), c => cover = c);
            
                    callback.Invoke(new DownloadableTrackMetadata(playlistEntry.GetTitle(), cover, ConstructUrl(playlistEntry.GetVideoId())));
                }

                downloader.ShowDownloadAllButton();
                yield break;
            }

            if (GetVideoId(url).Equals(RickRollId))
            {
                downloader.EasterEgg();
                yield break;
            }

            YoutubePlayerResponse metadata = null;
            yield return GetMetadataForSingleTrack(url, m => metadata = m);

            if (metadata?.Streaming == null)
            {
                downloader.DisplayMessage("No video found for this URL");
                yield break;
            }

            if (metadata.GetThumbnail() != null)
                yield return LoadTexture(metadata.GetThumbnail(), c => cover = c);

            callback.Invoke(new DownloadableTrackMetadata(metadata.GetTitle(), cover, url));
        }

        public override IEnumerator Download(DownloadableTrackEntry entry, TracksDownloader downloader)
        {
            entry.State = DownloadableTrackEntryState.Downloading;
            
            YoutubePlayerResponse metadata = default;
            var metadataRoutine = GetMetadataForSingleTrack(entry.Metadata.Url, m => metadata = m);
            metadataQueue.Enqueue(metadataRoutine);
            yield return new WaitUntil(() => metadataQueue.Peek() == metadataRoutine);
            yield return metadataRoutine;
            metadataQueue.Dequeue();

            if (metadata?.GetMp4AudioUrl() == null)
            {
                entry.State = DownloadableTrackEntryState.Failed;
                downloader.IncreaseFailed();
                yield break;
            }

            var sanitizedTitle = PathsUtils.CoerceValidFileName(entry.Metadata.Title);
            var inputFile = Path.Combine(PathsUtils.TemporaryFilesDirectory.FullName, $"{sanitizedTitle}.mp4");

            CreateFolderIfMissing(PathsUtils.TemporaryFilesDirectory);
            CreateFolderIfMissing(YouTubeDirPath);
            
            var webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                if (entry != null)
                    entry.DownloadProgress(e.ProgressPercentage);
            };
            
            var succeed = false;
            var downloadRoutine = DownloadFile(webClient, metadata.GetMp4AudioUrl(), inputFile, s => succeed = s);
            downloadQueue.Enqueue(downloadRoutine);
            yield return new WaitUntil(() => downloadQueue.Peek() == downloadRoutine);
            yield return downloadRoutine;
            downloadQueue.Dequeue();

            if (!succeed)
            {
                entry.State = DownloadableTrackEntryState.Failed;
                downloader.IncreaseFailed();
                yield break;
            }

            entry.State = DownloadableTrackEntryState.Enqueued;

            var conversionTask = CovertTrack(inputFile, Cancellation.Token);
            var conversion = queueManager.Enqueue(() =>
            {
                entry.State = DownloadableTrackEntryState.Processing;
                return conversionTask;
            });

            yield return new WaitUntil(() => conversion.IsCompleted || conversion.IsFaulted);
            RemoveFile(inputFile);

            if (!conversion.IsFaulted && !conversion.IsCanceled)
            {
                downloader.ShowRestartButton();
                downloader.IncreaseDownloaded();
                entry.State = DownloadableTrackEntryState.Downloaded;
                AddTrackToPlaylist(Path.Combine(YouTubeDirPath.FullName, $"{Path.GetFileNameWithoutExtension(inputFile)}.mp3"));
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
        }
        
        private static async Task CovertTrack(string inputFile, CancellationToken token)
        {
            var outputFile = Path.Combine(YouTubeDirPath.FullName, $"{Path.GetFileNameWithoutExtension(inputFile)}.mp3");
            SetExecutablesPath(PathsUtils.CgmePath);
            await ConvertToAudio(inputFile, outputFile, token);
        }

        private static IEnumerator GetMetadataForSingleTrack(string url, Action<YoutubePlayerResponse> callback)
        {
            YoutubePlayerResponse metadata = default;
            var request = YoutubePlayerRequest.ForVideoId(GetVideoId(url));
            
            yield return PostRequest<YoutubePlayerRequest, YoutubePlayerResponse>
                (YoutubePlayerUrl, request,m => metadata = m);
            
            if (metadata != null)
                callback.Invoke(metadata);
        }
        
        private static IEnumerator GetMetadataForPlaylist(string url, Action<YoutubePlaylistResponse> callback)
        {
            YoutubePlaylistResponse metadata = default;
            var request = YoutubePlaylistRequest.ForPlaylistId(GetPlaylistId(url));

            yield return PostRequest<YoutubePlaylistRequest, YoutubePlaylistResponse>
                (YoutubeBrowseUrl, request,m => metadata = m);

            if (metadata != null)
                callback.Invoke(metadata);
        }

        private static async Task ConvertToAudio(string input, string output, CancellationToken token)
        {
            var mediaInfo = await GetMediaInfo(input, token);
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            var videoStream = mediaInfo.VideoStreams.First().SetCodec("mjpeg");
            
            // Using the middle frame as an album cover
            var coverFrameOffset = TimeSpan.FromTicks(videoStream.Duration.Ticks / 2)
                .ToFFmpeg()
                .Replace(":", "\\:");
            
            var conversion = FFmpeg.Conversions.New()
                .AddStream(audioStream)
                .SetAudioBitrate(128000)
                .AddStream(videoStream)
                // A tweak to crop the frame for the album cover
                .AddParameter($"-vf trim=start='{coverFrameOffset}',trim=end_frame=1,crop=w='min(iw\\,ih)':h='min(iw\\,ih)',scale=256:256,setsar=1")
                .AddParameter("-threads 0")
                .SetOverwriteOutput(true)
                .SetOutput(output);

            await conversion.Start(token);
        }
        
        private static bool UrlIsSingleTrack(string url) =>
            Regex.IsMatch(url, YouTubeVideoRegex, RegexOptions.IgnoreCase);

        private static bool UrlIsPlaylist(string url) =>
            Regex.IsMatch(url, YouTubePlaylistRegex, RegexOptions.IgnoreCase);

        private static string ConstructUrl(string videoId) => "https://youtube.com/watch?v=" + videoId;

        private static string GetVideoId(string url) =>
            Regex.Match(url, YouTubeVideoRegex).Groups[1].Value;
        
        private static string GetPlaylistId(string url) =>
            Regex.Match(url, YouTubePlaylistRegex).Groups[1].Value;
    }
}