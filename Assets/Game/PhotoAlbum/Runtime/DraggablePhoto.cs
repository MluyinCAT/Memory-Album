using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class DraggablePhoto : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public string photoId;
        public System.Action<string> onClicked;
        public System.Action<string> onDragStart;

        private float _downTime;
        private bool _dragging;
        private GameObject _dragVisual;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData e)
        {
            _downTime = Time.time;
            _dragging = false;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!_dragging && Time.time - _downTime < 0.3f)
            {
                onClicked?.Invoke(photoId);
            }
        }

        public void OnBeginDrag(PointerEventData e)
        {
            Debug.Log("[Drag] OnBeginDrag photoId=" + photoId + " heldTime=" + (Time.time - _downTime));
            if (Time.time - _downTime < 0.3f) { Debug.Log("[Drag] Too short, skip"); return; }
            _dragging = true;
            onDragStart?.Invoke(photoId);

            var thumb = transform.Find("Thumbnail")?.GetComponent<Image>();
            Debug.Log("[Drag] Thumbnail found=" + (thumb != null) + " sprite=" + (thumb?.sprite != null));
            if (thumb != null && thumb.sprite != null)
            {
                _dragVisual = new GameObject("DragVisual", typeof(RectTransform), typeof(Image));
                _dragVisual.transform.SetParent(_canvas.transform, false);
                _dragVisual.transform.SetAsLastSibling();
                _dragVisual.GetComponent<Image>().sprite = thumb.sprite;
                _dragVisual.GetComponent<Image>().preserveAspect = true;
                _dragVisual.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
                _dragVisual.GetComponent<Image>().raycastTarget = false;
            }
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_dragging || _dragVisual == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, e.position, _canvas.worldCamera, out var pos);
            _dragVisual.transform.localPosition = pos;
        }

        public void OnEndDrag(PointerEventData e)
        {
            _dragging = false;
            if (_dragVisual != null) Destroy(_dragVisual);
        }

        private void OnDestroy()
        {
            if (_dragVisual != null) Destroy(_dragVisual);
        }
    }
}
