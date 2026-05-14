using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class ObjectInfoPanel : BasePanel
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private GameObject rootGroup;

        public bool IsShowing => rootGroup != null && rootGroup.activeSelf;

        protected override void Awake()
        {
            base.Awake();
            if (rootGroup != null) rootGroup.SetActive(false);
        }

        public void ShowInfo(ClickableObject obj)
        {
            if (obj == null) return;

            if (nameText != null) nameText.text = obj.objectName;
            if (descriptionText != null) descriptionText.text = obj.objectDescription;
            if (rootGroup != null) rootGroup.SetActive(true);

            obj.SetHighlight(true);
        }

        public void HideInfo()
        {
            if (rootGroup != null) rootGroup.SetActive(false);
        }
    }
}
