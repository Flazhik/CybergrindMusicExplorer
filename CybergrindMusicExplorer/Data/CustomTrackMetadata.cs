using System.IO;
using TagLib;
using UnityEngine;
using File = TagLib.File;

namespace CybergrindMusicExplorer.Data
{
    public class CustomTrackMetadata
    {
        private CustomTrackMetadata(Sprite logo, string title, string artist)
        {
            Logo = logo;
            Title = title;
            Artist = artist;
        }

        public Sprite Logo { get; }
        public string Title { get; }
        public string Artist { get; }


        public static CustomTrackMetadata From(File file)
        {
            var tags = file.Tag;
            Sprite logo = null;
            if (tags.Pictures.Length > 0)
            {
                IPicture pic = tags.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(ms.ToArray());

                logo = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100.0f);
            }

            return new CustomTrackMetadata(logo, tags.Title, tags.FirstAlbumArtist);
        }
    }
}