using UnityEngine;

namespace MemoryAlbum.PhotoAlbum
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ClickableObject : MonoBehaviour
    {
        [Header("物体信息")]
        public string objectName;
        [TextArea(3, 8)]
        public string objectDescription;

        [Header("可选照片关联")]
        public string relatedPhotoId;

        [Header("高亮")]
        public Color highlightColor = new Color(1f, 1f, 0.6f, 1f);

        private SpriteRenderer _renderer;
        private Color _originalColor;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null) _originalColor = _renderer.color;
        }

        public void SetHighlight(bool active)
        {
            if (_renderer != null)
                _renderer.color = active ? highlightColor : _originalColor;
        }
    }
}
