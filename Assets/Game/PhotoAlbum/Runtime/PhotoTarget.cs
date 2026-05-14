using UnityEngine;

namespace MemoryAlbum.PhotoAlbum
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CaptureSys.CaptureObj))]
    public sealed class PhotoTarget : MonoBehaviour
    {
        [Header("相片标识")]
        public string photoId;

        [Header("关联剧本")]
        public string vnScriptName;
        public string vnStartLineID;

        [Header("相片展示")]
        public Sprite photoSprite;
        public string photoName;
        [TextArea(3, 6)]
        public string photoDescription;

        [Header("目标类型")]
        public bool isKeyTarget = true;
    }
}
