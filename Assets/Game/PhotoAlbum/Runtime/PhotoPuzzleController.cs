using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class PhotoPuzzleController : MonoBehaviour
    {
        [Header("数据")]
        [SerializeField] private PhotoAlbumData albumData;
        [SerializeField] private PhotoPuzzleData puzzleData;

        [Header("排列槽位")]
        [SerializeField] private PuzzleSlot[] slots;

        [Header("照片选择器")]
        [SerializeField] private Transform photoSelectorParent;
        [SerializeField] private GameObject photoSelectorPrefab;

        [Header("操作按钮")]
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private Button backBtn;

        [Header("反馈")]
        [SerializeField] private DialogPopup dialogPopup;

        private readonly List<string> _currentOrder = new List<string>();
        private int _selectedSlotIndex = -1;

        private void Start()
        {
            var manager = PhotoAlbumManager.GetInstance();
            var collected = manager.GetCollectedPhotoIds();

            foreach (var photoId in manager.AllPhotoIds)
                _currentOrder.Add(photoId);

            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;
                if (slots[i]?.slotButton != null)
                    slots[i].slotButton.onClick.AddListener(() => OnSlotClicked(index));
            }

            if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmClicked);
            if (resetBtn != null) resetBtn.onClick.AddListener(ResetOrder);
            if (backBtn != null) backBtn.onClick.AddListener(GoBack);

            RefreshAllSlots();
        }

        private void OnSlotClicked(int index)
        {
            if (_selectedSlotIndex < 0)
            {
                _selectedSlotIndex = index;
                HighlightSlot(index, true);
            }
            else if (_selectedSlotIndex == index)
            {
                _selectedSlotIndex = -1;
                HighlightSlot(index, false);
            }
            else
            {
                var temp = _currentOrder[_selectedSlotIndex];
                _currentOrder[_selectedSlotIndex] = _currentOrder[index];
                _currentOrder[index] = temp;

                HighlightSlot(_selectedSlotIndex, false);
                _selectedSlotIndex = -1;
                RefreshAllSlots();
            }
        }

        private void HighlightSlot(int index, bool highlight)
        {
            if (index < 0 || index >= slots.Length || slots[index]?.highlightFrame == null) return;
            slots[index].highlightFrame.SetActive(highlight);
        }

        private void RefreshAllSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                string photoId = i < _currentOrder.Count ? _currentOrder[i] : null;

                if (albumData != null && albumData.TryGetEntry(photoId, out var entry))
                {
                    slots[i].SetPhoto(entry.photoSprite, entry.photoName);
                }
                else
                {
                    slots[i].SetEmpty();
                }
            }
        }

        private void OnConfirmClicked()
        {
            if (puzzleData == null) return;

            var matched = puzzleData.MatchSequence(_currentOrder);
            if (matched != null)
            {
                VNManager.GetInstance().StartGame(matched.endingScriptName);
            }
            else
            {
                ShowDialog("这样显然不合理");
            }
        }

        private void ResetOrder()
        {
            _currentOrder.Clear();
            foreach (var photoId in PhotoAlbumManager.GetInstance().AllPhotoIds)
                _currentOrder.Add(photoId);

            _selectedSlotIndex = -1;
            RefreshAllSlots();
        }

        private void GoBack()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PhotoExploration");
        }

        private void ShowDialog(string message)
        {
            if (dialogPopup != null)
            {
                dialogPopup.Show(message);
            }
        }

        private void OnDestroy()
        {
            if (confirmBtn != null) confirmBtn.onClick.RemoveListener(OnConfirmClicked);
            if (resetBtn != null) resetBtn.onClick.RemoveListener(ResetOrder);
            if (backBtn != null) backBtn.onClick.RemoveListener(GoBack);

            if (slots != null)
            {
                foreach (var slot in slots)
                    if (slot?.slotButton != null)
                        slot.slotButton.onClick.RemoveAllListeners();
            }
        }
    }

    [System.Serializable]
    public sealed class PuzzleSlot
    {
        public Button slotButton;
        public Image photoImage;
        public TMP_Text photoNameText;
        public GameObject highlightFrame;
        public GameObject emptyPlaceholder;

        public void SetPhoto(Sprite sprite, string name)
        {
            if (photoImage != null)
            {
                photoImage.sprite = sprite;
                photoImage.color = Color.white;
            }
            if (photoNameText != null) photoNameText.text = name;
            if (emptyPlaceholder != null) emptyPlaceholder.SetActive(false);
        }

        public void SetEmpty()
        {
            if (photoImage != null)
            {
                photoImage.sprite = null;
                photoImage.color = Color.clear;
            }
            if (photoNameText != null) photoNameText.text = "";
            if (emptyPlaceholder != null) emptyPlaceholder.SetActive(true);
        }
    }
}
