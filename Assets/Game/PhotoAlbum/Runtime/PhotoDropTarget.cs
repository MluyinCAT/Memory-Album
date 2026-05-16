using UnityEngine;
using UnityEngine.EventSystems;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class PhotoDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public int slotIndex;
        public System.Action<int> onDrop;

        private GameObject _highlight;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            // Find highlight child
            _highlight = transform.Find("HighlightFrame")?.gameObject;
        }

        public void OnDrop(PointerEventData e)
        {
            Debug.Log("[Drop] OnDrop slot=" + slotIndex + " dragging=" + e.dragging);
            onDrop?.Invoke(slotIndex);
            if (_highlight != null) _highlight.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (e.dragging && _highlight != null)
                _highlight.SetActive(true);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (_highlight != null) _highlight.SetActive(false);
        }
    }
}
