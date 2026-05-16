using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class ObjectInfoPanel : BasePanel
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private GameObject rootGroup;
        [SerializeField] private Button closeBtn;

        public bool IsShowing => rootGroup != null && rootGroup.activeSelf;

        protected override void Awake()
        {
            base.Awake();
            if (rootGroup != null) rootGroup.SetActive(false);
            if (closeBtn != null) closeBtn.onClick.AddListener(HideInfo);
        }

        private void Update()
        {
            if (!IsShowing) return;
            if (Mouse.current?.leftButton.wasPressedThisFrame == true
                && EventSystem.current != null
                && !EventSystem.current.IsPointerOverGameObject())
            {
                HideInfo();
            }
        }

        public void ShowInfo(ClickableObject obj)
        {
            if (obj == null) return;
            ShowInfo(obj.objectName, obj.objectDescription);
            obj.SetHighlight(true);
        }

        public void ShowInfo(string speaker, string text)
        {
            if (nameText != null) nameText.text = speaker;
            if (descriptionText != null) descriptionText.text = text;
            if (rootGroup != null) rootGroup.SetActive(true);
        }

        public void HideInfo()
        {
            if (rootGroup != null) rootGroup.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeBtn != null) closeBtn.onClick.RemoveListener(HideInfo);
        }
    }
}
