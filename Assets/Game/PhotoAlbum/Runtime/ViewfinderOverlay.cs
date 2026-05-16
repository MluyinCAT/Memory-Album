using MemoryAlbum.CaptureSys;
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
        [SerializeField] private Image crosshairDot;
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color focusColor = new Color(0.3f, 1f, 0.4f, 1f);

        private float _nextCheckTime;
        private bool _targetInZone;

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            Refresh();

            // 每 0.3 秒检测一次对焦状态
            if (Time.time >= _nextCheckTime)
            {
                _nextCheckTime = Time.time + 0.3f;
                _targetInZone = CheckTargetInZone();
            }

            if (crosshairDot != null)
                crosshairDot.color = _targetInZone ? focusColor : idleColor;
        }

        private bool CheckTargetInZone()
        {
            if (captureController == null) return false;
            var cam = captureController.GetCamera();
            if (cam == null) return false;

            float zone = captureController.GetDetectionZoneSize();
            float half = zone * 0.5f;

            foreach (var obj in CaptureRegistry.Instance.ActiveObjects)
            {
                if (obj == null || !obj.enabled) continue;
                var pt = obj.GetComponent<PhotoTarget>();
                if (pt == null || !pt.isKeyTarget) continue;

                Vector3 vp = cam.WorldToViewportPoint(obj.GetTargetPoint());
                if (vp.z > 0 && vp.x >= 0.5f - half && vp.x <= 0.5f + half
                    && vp.y >= 0.5f - half && vp.y <= 0.5f + half)
                {
                    return true;
                }
            }
            return false;
        }

        private void Refresh()
        {
            if (captureController == null) return;

            float zone = captureController.GetDetectionZoneSize();
            float half = zone * 0.5f;
            float cx = 0.5f, cy = 0.5f;

            if (topDim != null)    { topDim.anchorMin = new Vector2(0, cy + half); topDim.anchorMax = new Vector2(1, 1); topDim.offsetMin = Vector2.zero; topDim.offsetMax = Vector2.zero; }
            if (bottomDim != null) { bottomDim.anchorMin = new Vector2(0, 0); bottomDim.anchorMax = new Vector2(1, cy - half); bottomDim.offsetMin = Vector2.zero; bottomDim.offsetMax = Vector2.zero; }
            if (leftDim != null)   { leftDim.anchorMin = new Vector2(0, cy - half); leftDim.anchorMax = new Vector2(cx - half, cy + half); leftDim.offsetMin = Vector2.zero; leftDim.offsetMax = Vector2.zero; }
            if (rightDim != null)  { rightDim.anchorMin = new Vector2(cx + half, cy - half); rightDim.anchorMax = new Vector2(1, cy + half); rightDim.offsetMin = Vector2.zero; rightDim.offsetMax = Vector2.zero; }
        }
    }
}
