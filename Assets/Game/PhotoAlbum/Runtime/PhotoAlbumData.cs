using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryAlbum.PhotoAlbum
{
    [CreateAssetMenu(fileName = "PhotoAlbumData", menuName = "Memory Album/Photo Album Data")]
    public sealed class PhotoAlbumData : ScriptableObject
    {
        public List<PhotoEntry> entries = new List<PhotoEntry>();

        public bool TryGetEntry(string photoId, out PhotoEntry entry)
        {
            entry = entries.Find(e => e.photoId == photoId);
            return entry != null;
        }
    }

    [Serializable]
    public sealed class PhotoEntry
    {
        public string photoId;
        public Sprite photoSprite;
        public string photoName;
        [TextArea(3, 6)]
        public string photoDescription;
    }
}
