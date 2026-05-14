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
            var albumPanel = UIManager.GetInstance().GetPanel<PhotoAlbumPanel>("PhotoAlbumPanel");
            if (albumPanel != null)
            {
                albumPanel.ShowMe();
            }
            else
            {
                UIManager.GetInstance().ShowPanel<PhotoAlbumPanel>(
                    "PhotoAlbumPanel",
                    "VNovelizerRes/VNPrefabs/UI/PhotoAlbum",
                    E_UI_Layer.Top,
                    panel => panel?.ShowMe()
                );
            }
        }

        private void OnDestroy()
        {
            if (settingsBtn != null) settingsBtn.onClick.RemoveListener(OnSettingsClicked);
            if (captureBtn != null) captureBtn.onClick.RemoveListener(OnCaptureClicked);
            if (albumBtn != null) albumBtn.onClick.RemoveListener(OnAlbumClicked);
        }
    }
}
