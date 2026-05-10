using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryAlbum.CaptureSys
{
    public interface ICaptureInputSource : IDisposable
    {
        event Action CaptureTriggered;
        event Action ClearPhotosTriggered;

        void Enable();
        void Disable();
    }

    public interface ICaptureDetector
    {
        CaptureDetectionResult Detect(Camera camera);
    }

    public interface IPhotoCaptureService
    {
        IEnumerator CapturePhotoBytes(Action<byte[]> onCaptured);
    }

    public interface ICaptureRepository
    {
        bool SaveCapture(byte[] photoBytes, IReadOnlyList<CapturedObjectSnapshot> capturedObjects, bool includeTimestamp, out CapturePhotoRecord photoRecord);
        bool ClearCaptures(out int clearedPhotoCount);
    }

    public interface ICaptureRegistry
    {
        IReadOnlyCollection<CaptureObj> ActiveObjects { get; }
    }

    [Serializable]
    public sealed class CaptureManifest
    {
        public int nextSequence = 1;
        public List<CapturePhotoRecord> photos = new List<CapturePhotoRecord>();

        public void EnsureInitialized()
        {
            if (nextSequence <= 0)
            {
                nextSequence = 1;
            }

            if (photos == null)
            {
                photos = new List<CapturePhotoRecord>();
            }
        }
    }

    [Serializable]
    public sealed class CapturePhotoRecord
    {
        public int sequence;
        public string imageFileName;
        public string capturedAtUtc;
        public List<CapturedObjectRecord> capturedObjects = new List<CapturedObjectRecord>();
    }

    [Serializable]
    public sealed class CapturedObjectRecord
    {
        public string objectId;
        public Vector2 viewportOffset;
        public float distanceFromCenter;
    }

    public sealed class CapturedObjectSnapshot
    {
        public CapturedObjectSnapshot(CaptureObj source, string objectId, Vector2 viewportOffset, float distanceFromCenter)
        {
            Source = source;
            ObjectId = objectId;
            ViewportOffset = viewportOffset;
            DistanceFromCenter = distanceFromCenter;
        }

        public CaptureObj Source { get; }
        public string ObjectId { get; }
        public Vector2 ViewportOffset { get; }
        public float DistanceFromCenter { get; }
    }

    public sealed class CaptureDetectionResult
    {
        private static readonly IReadOnlyList<CapturedObjectSnapshot> EmptyVisibleObjects = Array.Empty<CapturedObjectSnapshot>();

        public static CaptureDetectionResult Empty { get; } = new CaptureDetectionResult(EmptyVisibleObjects);

        public CaptureDetectionResult(IReadOnlyList<CapturedObjectSnapshot> visibleObjects)
        {
            VisibleObjects = visibleObjects ?? EmptyVisibleObjects;
        }

        public IReadOnlyList<CapturedObjectSnapshot> VisibleObjects { get; }

        public bool HasVisibleObjects => VisibleObjects.Count > 0;
    }
}
