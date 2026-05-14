using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class PhotoPuzzlePanel : BasePanel
    {
        [Header("数据")]
        [SerializeField] private PhotoAlbumData albumData;
        [SerializeField] private PhotoPuzzleData puzzleData;

        [Header("排列槽位")]
        [SerializeField] private PuzzleSlot[] slots;

        [Header("操作按钮")]
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private Button closeBtn;

        [Header("反馈")]
        [SerializeField] private DialogPopup dialogPopup;

        private readonly List<string> _currentOrder = new List<string>();
        private int _selectedSlotIndex = -1;

        protected override void Awake()
        {
            base.Awake();

            var manager = PhotoAlbumManager.GetInstance();
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
            if (closeBtn != null) closeBtn.onClick.AddListener(ClosePanel);
        }

        public override void ShowMe()
        {
            base.ShowMe();
            ResetOrder();
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
                    slots[i].SetPhoto(entry.photoSprite, entry.photoName);
                else
                    slots[i].SetEmpty();
            }
        }

        private void OnConfirmClicked()
        {
            if (puzzleData == null) return;

            var matched = puzzleData.MatchSequence(_currentOrder);
            if (matched != null)
            {
                ClosePanel();
                VNManager.GetInstance().StartGame(matched.endingScriptName);
            }
            else
            {
                if (dialogPopup != null)
                    dialogPopup.Show("这样显然不合理");
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

        private void ClosePanel()
        {
            UIManager.GetInstance().HidePanel("PhotoPuzzlePanel");
        }

        public override void HideMe()
        {
            base.HideMe();
            _selectedSlotIndex = -1;
        }

        private void OnDestroy()
        {
            if (confirmBtn != null) confirmBtn.onClick.RemoveListener(OnConfirmClicked);
            if (resetBtn != null) resetBtn.onClick.RemoveListener(ResetOrder);
            if (closeBtn != null) closeBtn.onClick.RemoveListener(ClosePanel);
            if (slots != null)
            {
                foreach (var slot in slots)
                    if (slot?.slotButton != null)
                        slot.slotButton.onClick.RemoveAllListeners();
            }
        }
    }
}
