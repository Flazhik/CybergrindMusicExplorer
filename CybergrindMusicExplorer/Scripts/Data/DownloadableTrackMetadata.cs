using UnityEngine;

namespace CybergrindMusicExplorer.Scripts.Data
{
    public class DownloadableTrackMetadata
    {
        public string Title { get; private set; }
        public Texture Cover { get; private set; }
        public string Url { get; private set; }

        public DownloadableTrackMetadata(string title, Texture cover, string trackUrl)
        {
            Title = title;
            Cover = cover;
            Url = trackUrl;
        }
    }
}