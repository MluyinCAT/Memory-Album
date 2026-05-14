using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class DialogPopup : BasePanel
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button confirmBtn;

        private System.Action _onConfirmed;

        protected override void Awake()
        {
            base.Awake();
            if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmClicked);
        }

        public void Show(string message, System.Action onConfirmed = null)
        {
            _onConfirmed = onConfirmed;
            if (messageText != null) messageText.text = message;
            gameObject.SetActive(true);

            var uiMgr = UIManager.GetInstance();
            if (uiMgr != null)
            {
                Transform systemLayer = uiMgr.GetLayerFather(E_UI_Layer.System);
                if (systemLayer != null) transform.SetParent(systemLayer, false);
            }
        }

        private void OnConfirmClicked()
        {
            _onConfirmed?.Invoke();
            _onConfirmed = null;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (confirmBtn != null) confirmBtn.onClick.RemoveListener(OnConfirmClicked);
        }
    }
}
