using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace MemoryAlbum.PhotoAlbum
{
    [DisallowMultipleComponent]
    public sealed class SceneClickHandler : MonoBehaviour
    {
        [SerializeField] private Camera clickCamera;
        [SerializeField] private LayerMask clickableLayers = -1;
        [SerializeField] private ObjectInfoPanel infoPanel;
        [SerializeField] private GameObject[] hideWhenDialogue;

        private Camera _resolvedCamera;
        private ClickableObject _activeClickable;
        private bool _dialogueVisible;

        private void Awake()
        {
            _resolvedCamera = clickCamera != null ? clickCamera : Camera.main;
        }

        private void Update()
        {
            if (Mouse.current?.leftButton.wasPressedThisFrame != true) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // 如果正在显示对话，点击关闭
            if (_dialogueVisible)
            {
                HideDialogue();
                return;
            }

            // 射线检测点击的物体
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _resolvedCamera.ScreenToWorldPoint(mousePos);
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, clickableLayers);

            if (hit.collider != null)
            {
                var clickable = hit.collider.GetComponent<ClickableObject>();
                if (clickable != null)
                    ShowDialogue(clickable);
            }
        }

        private void ShowDialogue(ClickableObject obj)
        {
            _activeClickable = obj;
            _dialogueVisible = true;
            infoPanel?.ShowInfo(obj);
            SetButtonsVisible(false);

            if (!string.IsNullOrEmpty(obj.setFlagOnClick))
                VNovelizer.Core.API.VNAPI.SetBoolFlag(obj.setFlagOnClick, true);
        }

        private void HideDialogue()
        {
            _dialogueVisible = false;
            infoPanel?.HideInfo();
            _activeClickable?.SetHighlight(false);
            _activeClickable = null;
            SetButtonsVisible(true);
        }

        private void SetButtonsVisible(bool visible)
        {
            if (hideWhenDialogue == null) return;
            foreach (var go in hideWhenDialogue)
                if (go != null) go.SetActive(visible);
        }
    }
}
