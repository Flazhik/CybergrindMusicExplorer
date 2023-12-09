namespace CybergrindMusicExplorer.Scripts.Data
{
    public enum DownloadableTrackEntryState
    {
        Idle,
        Downloading,
        Enqueued,
        Processing,
        Downloaded,
        Failed
    }
}