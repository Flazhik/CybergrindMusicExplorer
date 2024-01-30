using System.IO;
using TagLib;
using UnityEngine;

namespace CybergrindMusicExplorer.Util

{
    public class MetadataUtils
    {
        public static Sprite GetAlbumCoverSprite(Tag tags)
        {
            if (tags.Pictures.Length <= 0)
                return default;
            
            var pic = tags.Pictures[0];
            var ms = new MemoryStream(pic.Data.Data);
            ms.Seek(0, SeekOrigin.Begin);

            var tex = new Texture2D(2, 2);
            tex.LoadImage(ms.ToArray());

            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        }
    }
}