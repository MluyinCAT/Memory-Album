using System;
using UnityEngine.InputSystem;

namespace MemoryAlbum.CaptureSys
{
    public sealed class InputSystemCaptureInputSource : ICaptureInputSource
    {
        private readonly InputAction captureAction;
        private readonly InputAction clearAction;
        private readonly bool ownsCaptureAction;
        private readonly bool ownsClearAction;

        public InputSystemCaptureInputSource(InputActionAsset inputActionsAsset, string actionMapName, string captureActionName, string clearActionName)
        {
            var resolvedCaptureAction = FindAction(inputActionsAsset, actionMapName, captureActionName);
            captureAction = resolvedCaptureAction ?? CreateCaptureFallbackAction(captureActionName);
            ownsCaptureAction = resolvedCaptureAction == null;
            captureAction.performed += HandleCapturePerformed;

            var resolvedClearAction = FindAction(inputActionsAsset, actionMapName, clearActionName);
            clearAction = resolvedClearAction ?? CreateClearFallbackAction(clearActionName);
            ownsClearAction = resolvedClearAction == null;
            clearAction.performed += HandleClearPerformed;
        }

        public event Action CaptureTriggered;
        public event Action ClearPhotosTriggered;

        public void Enable()
        {
            captureAction.Enable();

            if (clearAction != captureAction)
            {
                clearAction.Enable();
            }
        }

        public void Disable()
        {
            captureAction.Disable();

            if (clearAction != captureAction)
            {
                clearAction.Disable();
            }
        }

        public void Dispose()
        {
            captureAction.performed -= HandleCapturePerformed;
            clearAction.performed -= HandleClearPerformed;

            if (ownsCaptureAction)
            {
                captureAction.Dispose();
            }

            if (ownsClearAction && clearAction != captureAction)
            {
                clearAction.Dispose();
            }
        }

        private void HandleCapturePerformed(InputAction.CallbackContext _)
        {
            CaptureTriggered?.Invoke();
        }

        private void HandleClearPerformed(InputAction.CallbackContext _)
        {
            ClearPhotosTriggered?.Invoke();
        }

        private static InputAction FindAction(InputActionAsset inputActionsAsset, string actionMapName, string actionName)
        {
            if (inputActionsAsset == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(actionMapName))
            {
                var actionMap = inputActionsAsset.FindActionMap(actionMapName, false);
                if (actionMap != null)
                {
                    var action = actionMap.FindAction(actionName, false);
                    if (action != null)
                    {
                        return action;
                    }
                }
            }

            return inputActionsAsset.FindAction(actionName, false);
        }

        private static InputAction CreateCaptureFallbackAction(string actionName)
        {
            var fallbackAction = new InputAction(string.IsNullOrWhiteSpace(actionName) ? "Capture" : actionName, InputActionType.Button);
            fallbackAction.AddBinding("<Keyboard>/e");
            fallbackAction.AddBinding("<Gamepad>/rightShoulder");
            return fallbackAction;
        }

        private static InputAction CreateClearFallbackAction(string actionName)
        {
            var fallbackAction = new InputAction(string.IsNullOrWhiteSpace(actionName) ? "ClearCapturePhotos" : actionName, InputActionType.Button);
            fallbackAction.AddBinding("<Keyboard>/f");
            return fallbackAction;
        }
    }
}
