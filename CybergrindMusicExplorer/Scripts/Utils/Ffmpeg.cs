using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using CybergrindMusicExplorer.Scripts.UI;
using UnityEngine;

namespace CybergrindMusicExplorer.Scripts.Utils
{
    public static class Ffmpeg
    {
        private static readonly string FfmpegExecutableInnerPath = $"ffmpeg-master-latest-win64-gpl/bin/{FfmpegExecutable}";
        private static readonly string FfprobeExecutableInnerPath = $"ffmpeg-master-latest-win64-gpl/bin/{FfprobeExecutable}";
        private const string FfmpegExecutable = "ffmpeg.exe";
        private const string FfprobeExecutable = "ffprobe.exe";

        private static readonly string CgmePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string FfmpegZip = Path.Combine(CgmePath, "ffmpeg.zip");

        private static readonly string FfmpegPath = Path.Combine(CgmePath, FfmpegExecutable);
        private static readonly string FfprobePath = Path.Combine(CgmePath, FfprobeExecutable);

        private const string FfmpegArchivePath =
            "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";

        public static bool FfmpegInstalled() =>
            new FileInfo(FfmpegPath).Exists && new FileInfo(FfprobePath).Exists;

        public static void InstallFfmpeg(FfmpegDownloadSection downloadSection)
        {
            downloadSection.StartDownloading();
            
            using (var webClient = new WebClient())
            {
                webClient.Proxy = null;
                webClient.DownloadProgressChanged +=
                    (sender, e) => downloadSection.DownloadStatusUpdate(e.ProgressPercentage);
                webClient.DownloadFileCompleted +=
                    (sender, e) => UnzipFfmpeg(downloadSection);
                try
                {
                    webClient.DownloadFileAsync(new Uri(FfmpegArchivePath), FfmpegZip);
                }
                catch (Exception)
                {
                    downloadSection.DownloadFailed();
                }
            }
        }

        private static void UnzipFfmpeg(FfmpegDownloadSection downloadSection)
        {
            downloadSection.Unzipping();

            try
            {
                using (var zip = ZipFile.OpenRead(FfmpegZip))
                {
                    var ffmpeg = zip.Entries.First(e => e.FullName.Equals(FfmpegExecutableInnerPath));
                    var ffprobe = zip.Entries.First(e => e.FullName.Equals(FfprobeExecutableInnerPath));
                    ffmpeg.ExtractToFile(Path.Combine(CgmePath, FfmpegExecutable), true);
                    ffprobe.ExtractToFile(Path.Combine(CgmePath, FfprobeExecutable), true);
                }

                CleanupArchive();
                downloadSection.CheckFfmpegPresence();
            }
            catch (Exception)
            {
                downloadSection.DownloadFailed();
            }
        }

        private static void CleanupArchive() => File.Delete(FfmpegZip);
    }
}