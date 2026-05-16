using UnityEngine;
using UnityEngine.InputSystem;

namespace MemoryAlbum.PhotoAlbum
{
    [DisallowMultipleComponent]
    public sealed class CameraModeController : MonoBehaviour
    {
        [Header("相机")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera captureCamera;

        [Header("取景器UI（后续挂美术素材）")]
        [SerializeField] private GameObject viewfinderOverlay;

        [Header("移动")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private SpriteRenderer backgroundRenderer;

        [Header("缩放")]
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 20f;
        [SerializeField] private float maxZoom = 500f;

        [Header("拍照")]
        [SerializeField] private PhotoCaptureController captureController;

        private bool _photoMode;
        private Vector2 _moveInput;
        private Camera _resolvedMain;
        private Camera _resolvedCapture;

        public bool IsPhotoMode => _photoMode;

        private void Awake()
        {
            _resolvedMain = mainCamera != null ? mainCamera : Camera.main;
            _resolvedCapture = captureCamera;
        }

        private void Start()
        {
            Debug.Log("[CameraMode] 初始化完成, captureController=" + (captureController != null));
            SetPhotoMode(false);
        }

        private void Update()
        {
            if (!_photoMode) return;

            // ESC 退出拍照模式
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                SetPhotoMode(false);
                return;
            }

            // WASD 移动
            var kbd = Keyboard.current;
            if (kbd != null)
            {
                _moveInput = Vector2.zero;
                if (kbd.wKey.isPressed) _moveInput.y += 1;
                if (kbd.sKey.isPressed) _moveInput.y -= 1;
                if (kbd.aKey.isPressed) _moveInput.x -= 1;
                if (kbd.dKey.isPressed) _moveInput.x += 1;
            }

            if (_moveInput != Vector2.zero)
            {
                Vector3 pos = _resolvedCapture.transform.position;
                pos += (Vector3)(_moveInput.normalized * moveSpeed * Time.deltaTime);
                _resolvedCapture.transform.position = ClampToBackground(pos);
            }

            // Q 缩小 / E 放大
            float zoomDelta = 0f;
            if (kbd != null)
            {
                if (kbd.qKey.isPressed) zoomDelta -= zoomSpeed * Time.deltaTime;
                if (kbd.eKey.isPressed) zoomDelta += zoomSpeed * Time.deltaTime;
            }
            if (zoomDelta != 0f)
            {
                float newSize = Mathf.Clamp(_resolvedCapture.orthographicSize + zoomDelta, minZoom, maxZoom);
                _resolvedCapture.orthographicSize = newSize;
                // 缩放后重新 clamp 位置
                _resolvedCapture.transform.position = ClampToBackground(_resolvedCapture.transform.position);
            }
        }

        private Vector3 ClampToBackground(Vector3 pos)
        {
            if (backgroundRenderer == null || _resolvedCapture == null) return pos;

            Bounds bgBounds = backgroundRenderer.bounds;
            float halfH = _resolvedCapture.orthographicSize;
            float halfW = halfH * _resolvedCapture.aspect;
            float z = _resolvedCapture.transform.position.z;

            float minX = bgBounds.min.x + halfW;
            float maxX = bgBounds.max.x - halfW;
            float minY = bgBounds.min.y + halfH;
            float maxY = bgBounds.max.y - halfH;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            pos.z = z;
            return pos;
        }

        public void TogglePhotoMode()
        {
            SetPhotoMode(!_photoMode);
        }

        public void SetPhotoMode(bool active)
        {
            _photoMode = active;
            Debug.Log($"[CameraMode] 拍照模式 = {active}");

            if (_resolvedCapture != null)
            {
                _resolvedCapture.enabled = active;
                _resolvedCapture.rect = new Rect(0, 0, 1, 1);
            }

            if (_resolvedMain != null)
                _resolvedMain.enabled = !active;

            // 切 Canvas 的渲染相机，让 UI 跟随模式相机
            var canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            if (canvas != null)
                canvas.worldCamera = active ? _resolvedCapture : _resolvedMain;

            if (viewfinderOverlay != null)
                viewfinderOverlay.SetActive(active);

            // 隐藏/显示Top层的按钮
            var top = GameObject.Find("Canvas")?.transform.Find("Top");
            if (top != null)
            {
                foreach (Transform child in top)
                {
                    if (child.name != "ViewfinderOverlay")
                        child.gameObject.SetActive(!active);
                }
            }
        }
    }
}
