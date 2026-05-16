using System.Collections;
using MemoryAlbum.CaptureSys;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MemoryAlbum.PhotoAlbum
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class PhotoCaptureController : MonoBehaviour
    {
        [SerializeField] private Camera captureCamera;
        [SerializeField] private InputActionAsset inputActionsAsset;
        [SerializeField] private string captureActionName = "Capture";
        [SerializeField] private LayerMask occlusionLayers = ~0;
        [SerializeField] private CameraModeController cameraMode;

        [Header("判定区域")]
        [SerializeField, Range(0.05f, 1f)]
        private float detectionZoneSize = 0.224f;

        [Header("反馈")]
        [SerializeField] private DialogPopup dialogPopup;

        private InputAction _captureAction;
        private bool _ownsCaptureAction;
        private bool _captureInProgress;
        private IPhotoCaptureService _photoCaptureService;
        private Camera _resolvedCamera;

        public bool IsCaptureInProgress => _captureInProgress;

        private void Awake()
        {
            _resolvedCamera = captureCamera != null ? captureCamera : GetComponent<Camera>();
            _photoCaptureService = new ScreenCapturePhotoCaptureService();

            if (inputActionsAsset != null)
                _captureAction = inputActionsAsset.FindAction(captureActionName);

            if (_captureAction == null)
            {
                _captureAction = new InputAction(captureActionName, binding: "<Keyboard>/space");
                _ownsCaptureAction = true;
            }

            _captureAction.performed += _ => HandleCaptureInput();
        }

        private void Start()
        {
            _captureAction.Enable();
            var binding = _captureAction.bindings.Count > 0 ? _captureAction.bindings[0].effectivePath : "none";
            Debug.Log("[PhotoCapture] 初始化完成, action已启用, binding=" + binding + " cameraMode=" + (cameraMode != null));
        }

        private void HandleCaptureInput()
        {
            if (_captureInProgress) return;
            if (cameraMode == null) return;
            if (!cameraMode.IsPhotoMode) return;
            StartCoroutine(CaptureRoutine());
        }

        private void OnDestroy()
        {
            if (_captureAction != null)
            {
                _captureAction.performed -= _ => HandleCaptureInput();
                _captureAction.Disable();
                if (_ownsCaptureAction) _captureAction.Dispose();
            }
        }

        private bool IsInsideDetectionZone(Vector3 worldPoint)
        {
            Vector3 vp = _resolvedCamera.WorldToViewportPoint(worldPoint);
            if (vp.z < 0) return false;

            float half = detectionZoneSize * 0.5f;
            return vp.x >= 0.5f - half && vp.x <= 0.5f + half
                && vp.y >= 0.5f - half && vp.y <= 0.5f + half;
        }

        private IEnumerator CaptureRoutine()
        {
            _captureInProgress = true;

            var detector = new CaptureDetector(CaptureRegistry.Instance, occlusionLayers.value);
            var result = detector.Detect(_resolvedCamera);

            PhotoTarget matchedTarget = null;
            foreach (var snapshot in result.VisibleObjects)
            {
                if (snapshot.Source == null) continue;

                Vector3 targetPoint = snapshot.Source.GetTargetPoint();
                if (!IsInsideDetectionZone(targetPoint)) continue;

                var target = snapshot.Source.GetComponent<PhotoTarget>();
                if (target != null && target.isKeyTarget)
                {
                    matchedTarget = target;
                    break;
                }
            }

            if (matchedTarget == null)
            {
                ShowDialog("什么都没拍到");
                _captureInProgress = false;
                yield break;
            }

            if (PhotoAlbumManager.GetInstance().IsPhotoCollected(matchedTarget.photoId))
            {
                ShowDialog("已经拍过了");
                _captureInProgress = false;
                yield break;
            }

            yield return _photoCaptureService.CapturePhotoBytes(_ => { });
            PhotoAlbumManager.GetInstance().CollectPhoto(matchedTarget.photoId);

            if (!string.IsNullOrEmpty(matchedTarget.vnScriptName))
            {
                UIManager.GetInstance().HidePanel("PhotoAlbumPanel");

                // 淡出到黑
                var fade = GameObject.Find("ScreenFade")?.GetComponent<UnityEngine.UI.Image>();
                if (fade != null)
                {
                    float t = 0f;
                    while (t < 1.5f) { t += Time.deltaTime; fade.color = new Color(0, 0, 0, t / 1.5f); yield return null; }
                    fade.color = Color.black;
                }

                VNManager.GetInstance().StartGame(
                    matchedTarget.vnScriptName,
                    string.IsNullOrEmpty(matchedTarget.vnStartLineID) ? "" : matchedTarget.vnStartLineID
                );
            }

            _captureInProgress = false;
        }

        private void ShowDialog(string message)
        {
            if (dialogPopup != null)
                dialogPopup.Show(message);
        }

        public Camera GetCamera() => _resolvedCamera;
        public float GetDetectionZoneSize() => detectionZoneSize;
    }
}
