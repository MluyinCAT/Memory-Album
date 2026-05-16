using UnityEngine;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class PhotoExplorationUI : MonoBehaviour
    {
        [Header("按钮")]
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button captureBtn;
        [SerializeField] private Button albumBtn;

        [Header("控制引用")]
        [SerializeField] private CameraModeController cameraModeController;

        private void Awake()
        {
            // 仅在新游戏时重置状态（避免返回场景时清空已收集的照片）
            if (!VNovelizer.Core.API.VNAPI.GetBoolFlag("photo_session_started"))
            {
                PhotoAlbumManager.GetInstance().ResetAllPhotos();
                VNovelizer.Core.API.VNAPI.SetBoolFlag("photo_session_started", true);
            }

            if (settingsBtn != null)
                settingsBtn.onClick.AddListener(OnSettingsClicked);
            if (captureBtn != null)
                captureBtn.onClick.AddListener(OnCaptureClicked);
            if (albumBtn != null)
                albumBtn.onClick.AddListener(OnAlbumClicked);
        }

        private void OnSettingsClicked()
        {
            UIManager.GetInstance().ShowPanel<PausePanel>(
                "PausePanel",
                VNProjectConfig.Instance.UI_PausePath,
                E_UI_Layer.Top,
                null
            );
        }

        private void OnCaptureClicked()
        {
            cameraModeController?.TogglePhotoMode();
        }

        private void OnAlbumClicked()
        {
            var existing = GameObject.Find("PhotoAlbumUI");
            if (existing != null)
            {
                existing.SetActive(true);
                existing.GetComponent<PhotoAlbumPanel>()?.ShowMe();
                return;
            }

            var prefab = Resources.Load<GameObject>("VNovelizerRes/VNPrefabs/UI/PhotoAlbum/PhotoAlbumPanel");
            if (prefab == null)
            {
                Debug.LogError("[PhotoExplorationUI] 找不到相册预制体");
                return;
            }

            var sys = GameObject.Find("Canvas")?.transform.Find("System");
            var inst = Instantiate(prefab, sys);
            inst.name = "PhotoAlbumUI";

            var panel = inst.GetComponent<PhotoAlbumPanel>();
            if (panel != null)
            {
                var data = Resources.Load<PhotoAlbumData>("VNovelizerRes/VNPrefabs/UI/PhotoAlbum/PhotoAlbumData");
                typeof(PhotoAlbumPanel).GetField("albumData",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(panel, data);
            }

            inst.SetActive(true);
            panel?.ShowMe();
        }

        private void OnDestroy()
        {
            if (settingsBtn != null) settingsBtn.onClick.RemoveListener(OnSettingsClicked);
            if (captureBtn != null) captureBtn.onClick.RemoveListener(OnCaptureClicked);
            if (albumBtn != null) albumBtn.onClick.RemoveListener(OnAlbumClicked);
        }
    }
}
