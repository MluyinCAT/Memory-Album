using System.Collections.Generic;

namespace MemoryAlbum.CaptureSys
{
    public sealed class CaptureRegistry : ICaptureRegistry
    {
        private static readonly CaptureRegistry InstanceValue = new CaptureRegistry();
        private readonly HashSet<CaptureObj> activeObjects = new HashSet<CaptureObj>();

        public static CaptureRegistry Instance => InstanceValue;

        public IReadOnlyCollection<CaptureObj> ActiveObjects => activeObjects;

        private CaptureRegistry()
        {
        }

        internal void Register(CaptureObj captureObj)
        {
            if (captureObj != null)
            {
                activeObjects.Add(captureObj);
            }
        }

        internal void Unregister(CaptureObj captureObj)
        {
            if (captureObj != null)
            {
                activeObjects.Remove(captureObj);
            }
        }
    }
}
