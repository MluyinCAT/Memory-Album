using System;
using System.Collections;
using UnityEngine;

namespace MemoryAlbum.CaptureSys
{
    public sealed class ScreenCapturePhotoCaptureService : IPhotoCaptureService
    {
        public IEnumerator CapturePhotoBytes(Action<byte[]> onCaptured)
        {
            yield return new WaitForEndOfFrame();

            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            if (screenshot == null)
            {
                onCaptured?.Invoke(null);
                yield break;
            }

            byte[] pngBytes;
            try
            {
                pngBytes = screenshot.EncodeToPNG();
            }
            finally
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(screenshot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(screenshot);
                }
            }

            onCaptured?.Invoke(pngBytes);
        }
    }
}
