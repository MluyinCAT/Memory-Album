using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class PhotoAlbumPanel : BasePanel
    {
        [Header("数据")]
        [SerializeField] private PhotoAlbumData albumData;

        [Header("照片格子")]
        [SerializeField] private PhotoSlot[] photoSlots;

        [Header("详情弹窗")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailImage;
        [SerializeField] private TMP_Text detailNameText;
        [SerializeField] private TMP_Text detailDescText;
        [SerializeField] private Button detailCloseBtn;

        [Header("拼凑真相")]
        [SerializeField] private GameObject puzzleBtn;
        [SerializeField] private Button puzzleButton;
        [SerializeField] private string puzzlePanelPath = "VNovelizerRes/VNPrefabs/UI/PhotoPuzzle";

        private PhotoAlbumManager _albumManager;

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                HideMe();
            }
        }

        public override void HideMe()
        {
            base.HideMe();
            gameObject.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();

            _albumManager = PhotoAlbumManager.GetInstance();

            if (detailCloseBtn != null) detailCloseBtn.onClick.AddListener(HideDetail);
            if (puzzleButton != null) puzzleButton.onClick.AddListener(OnPuzzleClicked);

            if (detailPanel != null) detailPanel.SetActive(false);

            for (int i = 0; i < photoSlots.Length; i++)
            {
                int index = i;
                if (photoSlots[i] != null && photoSlots[i].slotButton != null)
                    photoSlots[i].slotButton.onClick.AddListener(() => OnSlotClicked(index));
            }
        }

        public override void ShowMe()
        {
            base.ShowMe();
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            if (albumData == null || photoSlots == null) return;

            for (int i = 0; i < photoSlots.Length; i++)
            {
                var slot = photoSlots[i];
                if (slot == null) continue;

                string photoId = i < albumData.entries.Count ? albumData.entries[i].photoId : null;
                bool collected = photoId != null && _albumManager.IsPhotoCollected(photoId);

                if (collected)
                {
                    var entry = albumData.entries[i];
                    slot.SetCollected(entry.photoSprite, entry.photoName);
                }
                else
                {
                    slot.SetLocked();
                }
            }

            if (puzzleBtn != null)
                puzzleBtn.SetActive(_albumManager.PuzzleUnlocked);
        }

        private void OnSlotClicked(int index)
        {
            if (albumData == null || index >= albumData.entries.Count) return;

            var entry = albumData.entries[index];
            if (!_albumManager.IsPhotoCollected(entry.photoId)) return;

            ShowDetail(entry);
        }

        private void ShowDetail(PhotoEntry entry)
        {
            if (detailPanel == null) return;
            detailPanel.SetActive(true);

            if (detailImage != null) detailImage.sprite = entry.photoSprite;
            if (detailNameText != null) detailNameText.text = entry.photoName;
            if (detailDescText != null) detailDescText.text = entry.photoDescription;
        }

        private void HideDetail()
        {
            if (detailPanel != null) detailPanel.SetActive(false);
        }

        private void OnPuzzleClicked()
        {
            UIManager.GetInstance().ShowPanel<PhotoPuzzlePanel>(
                "PhotoPuzzlePanel",
                puzzlePanelPath,
                E_UI_Layer.Top,
                null
            );
        }

        private void OnDestroy()
        {
            if (detailCloseBtn != null) detailCloseBtn.onClick.RemoveListener(HideDetail);
            if (puzzleButton != null) puzzleButton.onClick.RemoveListener(OnPuzzleClicked);
            if (photoSlots != null)
            {
                foreach (var slot in photoSlots)
                    if (slot != null && slot.slotButton != null)
                        slot.slotButton.onClick.RemoveAllListeners();
            }
        }
    }

    [System.Serializable]
    public sealed class PhotoSlot
    {
        public Button slotButton;
        public Image thumbnailImage;
        public TMP_Text nameText;
        public GameObject lockedOverlay;
        public GameObject collectedMark;

        public void SetLocked()
        {
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = null;
                thumbnailImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            }
            if (nameText != null) nameText.text = "???";
            if (lockedOverlay != null) lockedOverlay.SetActive(true);
            if (collectedMark != null) collectedMark.SetActive(false);
        }

        public void SetCollected(Sprite sprite, string name)
        {
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = sprite;
                thumbnailImage.color = Color.white;
            }
            if (nameText != null) nameText.text = name;
            if (lockedOverlay != null) lockedOverlay.SetActive(false);
            if (collectedMark != null) collectedMark.SetActive(true);
        }
    }
}
