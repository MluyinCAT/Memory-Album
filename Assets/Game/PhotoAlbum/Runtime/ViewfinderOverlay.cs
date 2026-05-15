using UnityEngine;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class ViewfinderOverlay : MonoBehaviour
    {
        [SerializeField] private PhotoCaptureController captureController;
        [SerializeField] private RectTransform topDim;
        [SerializeField] private RectTransform bottomDim;
        [SerializeField] private RectTransform leftDim;
        [SerializeField] private RectTransform rightDim;

        private const float CenterX = 0.5f;
        private const float CenterY = 0.5f;

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (captureController == null) return;

            float zone = captureController.GetDetectionZoneSize();
            float half = zone * 0.5f;
            float cx = CenterX, cy = CenterY;

            if (topDim != null)    { topDim.anchorMin = new Vector2(0, cy + half); topDim.anchorMax = new Vector2(1, 1); topDim.offsetMin = Vector2.zero; topDim.offsetMax = Vector2.zero; }
            if (bottomDim != null) { bottomDim.anchorMin = new Vector2(0, 0); bottomDim.anchorMax = new Vector2(1, cy - half); bottomDim.offsetMin = Vector2.zero; bottomDim.offsetMax = Vector2.zero; }
            if (leftDim != null)   { leftDim.anchorMin = new Vector2(0, cy - half); leftDim.anchorMax = new Vector2(cx - half, cy + half); leftDim.offsetMin = Vector2.zero; leftDim.offsetMax = Vector2.zero; }
            if (rightDim != null)  { rightDim.anchorMin = new Vector2(cx + half, cy - half); rightDim.anchorMax = new Vector2(1, cy + half); rightDim.offsetMin = Vector2.zero; rightDim.offsetMax = Vector2.zero; }
        }
    }
}
