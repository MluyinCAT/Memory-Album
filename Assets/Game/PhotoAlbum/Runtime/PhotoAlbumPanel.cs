using System.Collections.Generic;
using System.Reflection;
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
            if (detailPanel != null) detailPanel.SetActive(false);

            // Auto-detect slots from FilmStrip children if array is empty
            if (photoSlots == null || photoSlots.Length == 0)
            {
                var film = transform.Find("FilmStrip");
                if (film != null)
                {
                    var list = new System.Collections.Generic.List<PhotoSlot>();
                    for (int i = 0; i < film.childCount; i++)
                    {
                        var child = film.GetChild(i);
                        if (!child.name.StartsWith("Slot_")) continue;
                        var btn = child.GetComponent<UnityEngine.UI.Button>();
                        var thumb = child.Find("Thumbnail")?.GetComponent<UnityEngine.UI.Image>();
                        var mark = child.Find("CollectedMark");
                        list.Add(new PhotoSlot { slotButton = btn, thumbnailImage = thumb, collectedMark = mark?.gameObject });
                    }
                    photoSlots = list.ToArray();
                }
            }

            for (int i = 0; i < photoSlots.Length; i++)
            {
                int index = i;
                if (photoSlots[i] != null && photoSlots[i].slotButton != null)
                    photoSlots[i].slotButton.onClick.AddListener(() => OnSlotClicked(index));
            }

            // Wire ClueBoard puzzle slots
            var pzPanel = GetComponent<PhotoPuzzlePanel>();
            if (pzPanel != null)
            {
                // Auto-detect puzzle slots from ClueBoard children
                var cb = transform.Find("ClueBoard");
                var pzSlots = typeof(PhotoPuzzlePanel).GetField("slots",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(pzPanel) as PuzzleSlot[];
                if (cb != null && (pzSlots == null || pzSlots.Length == 0))
                {
                    var pzList = new List<PuzzleSlot>();
                    for (int i = 0; i < cb.childCount; i++)
                    {
                        var child = cb.GetChild(i);
                        if (!child.name.StartsWith("PuzzleSlot_")) continue;
                        var btn = child.GetComponent<UnityEngine.UI.Button>();
                        var img = child.Find("Thumbnail")?.GetComponent<UnityEngine.UI.Image>();
                        var hf = child.Find("HighlightFrame")?.gameObject;
                        pzList.Add(new PuzzleSlot { slotButton = btn, photoImage = img, highlightFrame = hf });
                    }
                    typeof(PhotoPuzzlePanel).GetField("slots",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(pzPanel, pzList.ToArray());
                }
                pzPanel.WireSlotClicks();
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
                    slot.SetCollected(albumData.entries[i].photoSprite, albumData.entries[i].photoName);
                else
                    slot.SetLocked();
            }
        }

        private void OnSlotClicked(int index)
        {
            if (albumData == null || index >= albumData.entries.Count) return;

            var entry = albumData.entries[index];
            if (!_albumManager.IsPhotoCollected(entry.photoId)) return;

            // 选取照片：通知 PuzzlePanel
            var puzzle = GetComponent<PhotoPuzzlePanel>();
            if (puzzle != null)
            {
                puzzle.PickUpPhoto(entry.photoId);
            }
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

        private void OnDestroy()
        {
            if (detailCloseBtn != null) detailCloseBtn.onClick.RemoveListener(HideDetail);
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

        // Cache slot background image
        private Image _bgImage;
        private Image BgImage
        {
            get
            {
                if (_bgImage == null && slotButton != null)
                    _bgImage = slotButton.GetComponent<Image>();
                return _bgImage;
            }
        }

        public void SetLocked()
        {
            if (thumbnailImage != null) thumbnailImage.gameObject.SetActive(false);
            if (collectedMark != null) collectedMark.SetActive(false);
            if (BgImage != null) BgImage.gameObject.SetActive(false);
        }

        public void SetCollected(Sprite sprite, string name)
        {
            if (BgImage != null) BgImage.gameObject.SetActive(true);
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = sprite;
                thumbnailImage.color = Color.white;
                thumbnailImage.gameObject.SetActive(true);
            }
            if (collectedMark != null) collectedMark.SetActive(true);
        }
    }
}
