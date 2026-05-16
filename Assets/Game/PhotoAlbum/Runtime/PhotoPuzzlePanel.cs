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

        [Header("ClueBoard 槽位")]
        [SerializeField] private PuzzleSlot[] slots;

        [Header("操作按钮")]
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button resetBtn;

        // 每个槽位存放的 photoId，空槽为 null
        private readonly string[] _placedPhotoIds = new string[6];
        // 当前从FilmStrip选取中的 photoId
        private string _heldPhotoId;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("[Puzzle] Awake - slots=" + (slots != null ? slots.Length : -1)
                + " confirmBtn=" + (confirmBtn != null) + " resetBtn=" + (resetBtn != null)
                + " puzzleData=" + (puzzleData != null) + " albumData=" + (albumData != null));
            if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmClicked);
            if (resetBtn != null) resetBtn.onClick.AddListener(ResetAll);
            for (int i = 0; i < slots.Length; i++) _placedPhotoIds[i] = null;
            _heldPhotoId = null;
        }

        public string HeldPhotoId => _heldPhotoId;

        public void PickUpPhoto(string photoId)
        {
            if (!string.IsNullOrEmpty(_heldPhotoId))
                ReturnPhoto(_heldPhotoId);
            _heldPhotoId = photoId;
            RefreshAllSlots();
        }

        public void PlacePhoto(int slotIndex, string photoId)
        {
            if (slotIndex < 0 || slotIndex >= _placedPhotoIds.Length) return;
            if (!string.IsNullOrEmpty(_placedPhotoIds[slotIndex]))
                ReturnPhoto(_placedPhotoIds[slotIndex]);
            _placedPhotoIds[slotIndex] = photoId;
            RefreshAllSlots();
        }

        public void ClearHeld()
        {
            _heldPhotoId = null;
            RefreshAllSlots();
        }

        private void ReturnPhoto(string photoId)
        {
            // Nothing to do for FilmStrip - the photo just goes back to available
        }

        private void OnSlotClicked(int index, PuzzleSlot slot)
        {
            if (!string.IsNullOrEmpty(_heldPhotoId))
            {
                // Placing a photo into this slot
                // If slot already has a photo, return it first
                if (!string.IsNullOrEmpty(_placedPhotoIds[index]))
                    ReturnPhoto(_placedPhotoIds[index]);
                _placedPhotoIds[index] = _heldPhotoId;
                _heldPhotoId = null;
                RefreshAllSlots();
            }
            else if (!string.IsNullOrEmpty(_placedPhotoIds[index]))
            {
                // Pick up photo from slot to return it
                ReturnPhoto(_placedPhotoIds[index]);
                _placedPhotoIds[index] = null;
                RefreshAllSlots();
            }
        }

        private void RefreshAllSlots()
        {
            Debug.Log("[Puzzle] RefreshAllSlots albumData=" + (albumData != null) + " entries=" + (albumData != null ? albumData.entries.Count : 0));
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                var pid = _placedPhotoIds[i];
                PhotoEntry entry = null;
                bool hasEntry = !string.IsNullOrEmpty(pid) && albumData != null && albumData.TryGetEntry(pid, out entry);
                Debug.Log("[Puzzle] Slot[" + i + "] pid=" + (pid ?? "null") + " hasEntry=" + hasEntry);
                if (hasEntry && entry != null)
                    slots[i].SetPhoto(entry.photoSprite, entry.photoName);
                else
                    slots[i].SetEmpty();
            }
        }

        private void OnConfirmClicked()
        {
            Debug.Log("[Puzzle] === OnConfirmClicked ===");
            Debug.Log("[Puzzle] puzzleData=" + (puzzleData != null));
            if (puzzleData == null) return;

            var order = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                Debug.Log("[Puzzle] Slot[" + i + "]=" + (_placedPhotoIds[i] ?? "EMPTY"));
                if (string.IsNullOrEmpty(_placedPhotoIds[i])) break;
                order.Add(_placedPhotoIds[i]);
            }

            Debug.Log("[Puzzle] Order count=" + order.Count + " seqs=" + puzzleData.validSequences.Count);
            for (int i = 0; i < order.Count; i++) Debug.Log("[Puzzle]   [" + i + "] " + order[i]);

            if (order.Count < 6)
            {
                ShowHint("请将所有照片放入线索板");
                return;
            }

            var matched = puzzleData.MatchSequence(order);
            Debug.Log("[Puzzle] Matched=" + (matched != null ? matched.endingScriptName : "NULL"));

            if (matched != null)
            {
                if (!string.IsNullOrEmpty(puzzleData.requiredItemFlag)
                    && !VNovelizer.Core.API.VNAPI.GetBoolFlag(puzzleData.requiredItemFlag))
                {
                    Debug.Log("[Puzzle] Required item not read: " + puzzleData.requiredItemFlag);
                    ShowHint(puzzleData.requiredItemHint);
                    return;
                }
                Debug.Log("[Puzzle] Starting game: " + matched.endingScriptName);
                VNManager.GetInstance().StartGame(matched.endingScriptName);
            }
            else
            {
                var hints = puzzleData.GetErrorHints(order);
                Debug.Log("[Puzzle] Error hints count=" + hints.Count);
                if (hints.Count > 0)
                    ShowHint(string.Join("\n", hints));
                else
                    ShowHint("这样显然不合理");
            }
        }

        private void ShowHint(string message)
        {
            // 暂时隐藏相册面板，露出 ObjectInfoPanel
            var albumRoot = GetComponentInParent<PhotoAlbumPanel>()?.gameObject;
            if (albumRoot != null) albumRoot.SetActive(false);

            var infoPanel = GameObject.Find("ObjectInfoPanel")?.GetComponent<ObjectInfoPanel>();
            if (infoPanel != null)
            {
                infoPanel.ShowInfo("黎", message);
            }
        }

        private void ResetAll()
        {
            for (int i = 0; i < 6; i++) _placedPhotoIds[i] = null;
            _heldPhotoId = null;
            RefreshAllSlots();
        }

        public void WireSlotClicks()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                int index = i;
                var slot = slots[i];
                if (slot.slotButton != null)
                {
                    slot.slotButton.onClick.RemoveAllListeners();
                    slot.slotButton.onClick.AddListener(() => OnSlotClicked(index, slot));
                }
            }
        }

        private void OnDestroy()
        {
            if (confirmBtn != null) confirmBtn.onClick.RemoveListener(OnConfirmClicked);
            if (resetBtn != null) resetBtn.onClick.RemoveListener(ResetAll);
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
            if (photoImage != null) { photoImage.sprite = sprite; photoImage.color = Color.white; photoImage.gameObject.SetActive(true); }
            if (photoNameText != null) photoNameText.text = name;
            if (emptyPlaceholder != null) emptyPlaceholder.SetActive(false);
        }

        public void SetEmpty()
        {
            if (photoImage != null) { photoImage.sprite = null; photoImage.color = Color.clear; }
            if (photoNameText != null) photoNameText.text = "";
            if (emptyPlaceholder != null) emptyPlaceholder.SetActive(true);
        }
    }
}
