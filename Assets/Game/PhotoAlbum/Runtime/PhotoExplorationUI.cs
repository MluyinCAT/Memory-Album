using System.Collections;
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

        private IEnumerator Start()
        {
            // 场景进入时淡入
            var fade = GameObject.Find("ScreenFade")?.GetComponent<UnityEngine.UI.Image>();
            if (fade != null)
            {
                fade.color = Color.black;
                float t = 0f;
                while (t < 1f) { t += Time.deltaTime; fade.color = new Color(0, 0, 0, 1f - t / 1f); yield return null; }
                fade.color = new Color(0, 0, 0, 0);
            }

            // 首次进入显示引导
            if (!VNovelizer.Core.API.VNAPI.GetBoolFlag("tutorial_shown"))
            {
                yield return new WaitForSeconds(0.5f);
                yield return ShowTutorial();
                VNovelizer.Core.API.VNAPI.SetBoolFlag("tutorial_shown", true);
            }
        }

        private IEnumerator ShowTutorial()
        {
            var infoPanel = GameObject.Find("ObjectInfoPanel")?.GetComponent<ObjectInfoPanel>();
            if (infoPanel == null) yield break;

            var steps = new (string speaker, string text)[] {
                ("Tips", "欢迎来到零的房间。这里藏着许多记忆的碎片。"),
                ("Tips", "点击场景中的物品可以查看它们的介绍。"),
                ("Tips", "点击左上角的📷按钮进入拍照模式。"),
                ("Tips", "拍照模式中：WASD移动镜头，Q/E缩放，空格键拍照，ESC退出。"),
                ("Tips", "将目标对准屏幕中央的取景框，准星变绿时即可拍摄。"),
                ("Tips", "拍到的回忆碎片会显示在右下角📖相册中。"),
                ("Tips", "收集全部碎片后，在相册中排列它们，揭开真相吧。"),
            };

            foreach (var step in steps)
            {
                infoPanel.ShowInfo(step.speaker, step.text);
                // 等待玩家点击继续
                while (infoPanel.IsShowing) yield return null;
                yield return new WaitForSeconds(0.2f);
            }
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
