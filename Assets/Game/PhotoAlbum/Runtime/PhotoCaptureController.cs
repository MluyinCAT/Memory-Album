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
            Debug.Log("[PhotoCapture] 空格键收到");
            if (_captureInProgress) { Debug.Log("[PhotoCapture] 上一次拍照未完成，跳过"); return; }
            if (cameraMode == null) { Debug.Log("[PhotoCapture] cameraMode 引用为空！"); return; }
            if (!cameraMode.IsPhotoMode) { Debug.Log("[PhotoCapture] 不在拍照模式，跳过"); return; }
            Debug.Log("[PhotoCapture] 开始拍照检测...");
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
            Debug.Log("[PhotoCapture] 检测完成, VisibleObjects数量=" + result.VisibleObjects.Count);

            PhotoTarget matchedTarget = null;
            foreach (var snapshot in result.VisibleObjects)
            {
                if (snapshot.Source == null) { Debug.Log("[PhotoCapture] snapshot.Source为null, 跳过"); continue; }

                Vector3 targetPoint = snapshot.Source.GetTargetPoint();
                Vector3 vp = _resolvedCamera.WorldToViewportPoint(targetPoint);
                Debug.Log("[PhotoCapture] 物体=" + snapshot.Source.name + " objectId=" + snapshot.Source.ObjectId + " worldPos=" + targetPoint + " viewport=" + vp + " inZone=" + IsInsideDetectionZone(targetPoint));

                if (!IsInsideDetectionZone(targetPoint)) continue;

                var target = snapshot.Source.GetComponent<PhotoTarget>();
                if (target != null && target.isKeyTarget)
                {
                    matchedTarget = target;
                    Debug.Log("[PhotoCapture] 匹配到关键目标: " + target.photoId);
                    break;
                }
                else
                {
                    Debug.Log("[PhotoCapture] PhotoTarget=" + (target != null) + " isKey=" + (target != null ? target.isKeyTarget : false));
                }
            }

            if (matchedTarget == null)
            {
                Debug.Log("[PhotoCapture] 未匹配到关键目标，弹出提示");
                ShowDialog("什么都没拍到");
                _captureInProgress = false;
                yield break;
            }

            bool alreadyCollected = PhotoAlbumManager.GetInstance().IsPhotoCollected(matchedTarget.photoId);
            Debug.Log("[PhotoCapture] IsPhotoCollected=" + alreadyCollected);
            if (alreadyCollected)
            {
                ShowDialog("已经拍过了");
                _captureInProgress = false;
                yield break;
            }

            Debug.Log("[PhotoCapture] 开始截图...");
            yield return _photoCaptureService.CapturePhotoBytes(_ => { });
            Debug.Log("[PhotoCapture] 截图完成，开始收集...");

            bool collected = PhotoAlbumManager.GetInstance().CollectPhoto(matchedTarget.photoId);
            Debug.Log("[PhotoCapture] CollectPhoto result=" + collected);

            if (!string.IsNullOrEmpty(matchedTarget.vnScriptName))
            {
                Debug.Log("[PhotoCapture] 跳转VN: " + matchedTarget.vnScriptName);
                UIManager.GetInstance().HidePanel("PhotoAlbumPanel");
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
