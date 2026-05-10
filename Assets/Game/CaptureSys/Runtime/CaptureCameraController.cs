using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MemoryAlbum.CaptureSys
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CaptureCameraController : MonoBehaviour
    {
        [SerializeField] private Camera captureSourceCamera;
        [SerializeField] private InputActionAsset inputActionsAsset;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string actionName = "Capture";
        [SerializeField] private string clearActionName = "ClearCapturePhotos";
        [SerializeField] private string storageFolderName = "CaptureSys";
        [SerializeField] private LayerMask occlusionLayers = ~0;
        [SerializeField] private bool recordTimestamp = true;
        [SerializeField] private bool verboseLogging = true;

        private ICaptureInputSource inputSource;
        private IPhotoCaptureService photoCaptureService;
        private ICaptureRepository captureRepository;
        private Camera resolvedCamera;
        private bool captureInProgress;

        public bool TryCapture()
        {
            EnsureInitialized();
            if (captureInProgress || resolvedCamera == null)
            {
                return false;
            }

            var detector = new CaptureDetector(CaptureRegistry.Instance, occlusionLayers.value);
            var detectionResult = detector.Detect(resolvedCamera);
            if (!detectionResult.HasVisibleObjects)
            {
                if (verboseLogging)
                {
                    Debug.Log("\u6ca1\u6709\u68c0\u6d4b\u5230CaptureObj", this);
                }

                return false;
            }

            StartCoroutine(CaptureRoutine(detectionResult));
            return true;
        }

        public bool CaptureNow()
        {
            return TryCapture();
        }

        public bool TryClearCaptures()
        {
            EnsureInitialized();
            if (captureInProgress || captureRepository == null)
            {
                return false;
            }

            if (!captureRepository.ClearCaptures(out var clearedPhotoCount))
            {
                if (verboseLogging)
                {
                    Debug.LogError("\u6e05\u7a7a\u62cd\u7167\u8bb0\u5f55\u5931\u8d25\u3002", this);
                }

                return false;
            }

            if (verboseLogging)
            {
                if (clearedPhotoCount > 0)
                {
                    Debug.Log($"\u5df2\u6e05\u7a7a {clearedPhotoCount} \u5f20\u76f8\u7247\u53ca\u5176\u8bb0\u5f55\u3002", this);
                }
                else
                {
                    Debug.Log("\u6ca1\u6709\u53ef\u6e05\u7a7a\u7684\u76f8\u7247\u3002", this);
                }
            }

            return true;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            inputSource?.Enable();
        }

        private void OnDisable()
        {
            inputSource?.Disable();
        }

        private void OnDestroy()
        {
            if (inputSource != null)
            {
                inputSource.CaptureTriggered -= HandleCaptureTriggered;
                inputSource.ClearPhotosTriggered -= HandleClearPhotosTriggered;
                inputSource.Dispose();
                inputSource = null;
            }
        }

        private void Reset()
        {
            captureSourceCamera = GetComponent<Camera>();
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(actionMapName))
            {
                actionMapName = "Player";
            }

            if (string.IsNullOrWhiteSpace(actionName))
            {
                actionName = "Capture";
            }

            if (string.IsNullOrWhiteSpace(clearActionName))
            {
                clearActionName = "ClearCapturePhotos";
            }

            if (string.IsNullOrWhiteSpace(storageFolderName))
            {
                storageFolderName = "CaptureSys";
            }
        }

        private IEnumerator CaptureRoutine(CaptureDetectionResult detectionResult)
        {
            captureInProgress = true;

            byte[] photoBytes = null;
            yield return photoCaptureService.CapturePhotoBytes(bytes => photoBytes = bytes);

            if (photoBytes == null || photoBytes.Length == 0)
            {
                if (verboseLogging)
                {
                    Debug.LogWarning("\u622a\u56fe\u5931\u8d25\uff0c\u672a\u751f\u6210\u7167\u7247\u3002", this);
                }

                captureInProgress = false;
                yield break;
            }

            if (!captureRepository.SaveCapture(photoBytes, detectionResult.VisibleObjects, recordTimestamp, out var photoRecord))
            {
                if (verboseLogging)
                {
                    Debug.LogError("\u62cd\u7167\u6570\u636e\u4fdd\u5b58\u5931\u8d25\u3002", this);
                }

                captureInProgress = false;
                yield break;
            }

            if (verboseLogging)
            {
                Debug.Log($"\u5df2\u4fdd\u5b58\u7167\u7247 {photoRecord.imageFileName}\uff0c\u8bb0\u5f55 {photoRecord.capturedObjects.Count} \u4e2a CaptureObj\u3002", this);
            }

            captureInProgress = false;
        }

        private void HandleCaptureTriggered()
        {
            TryCapture();
        }

        private void HandleClearPhotosTriggered()
        {
            TryClearCaptures();
        }

        private void EnsureInitialized()
        {
            if (resolvedCamera == null)
            {
                resolvedCamera = captureSourceCamera != null ? captureSourceCamera : GetComponent<Camera>();
            }

            if (photoCaptureService == null)
            {
                photoCaptureService = new ScreenCapturePhotoCaptureService();
            }

            if (captureRepository == null)
            {
                captureRepository = new FileCaptureRepository(storageFolderName);
            }

            if (inputSource == null)
            {
                inputSource = new InputSystemCaptureInputSource(inputActionsAsset, actionMapName, actionName, clearActionName);
                inputSource.CaptureTriggered += HandleCaptureTriggered;
                inputSource.ClearPhotosTriggered += HandleClearPhotosTriggered;
            }
        }
    }
}
