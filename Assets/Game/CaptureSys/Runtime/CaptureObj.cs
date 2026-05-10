using UnityEngine;

namespace MemoryAlbum.CaptureSys
{
    public enum CaptureTargetPointSource
    {
        Auto = 0,
        TransformPosition = 1,
        RendererBoundsCenter = 2,
        ColliderBoundsCenter = 3
    }

    [DisallowMultipleComponent]
    public sealed class CaptureObj : MonoBehaviour
    {
        [SerializeField] private string objectId = string.Empty;
        [SerializeField] private CaptureTargetPointSource targetPointSource = CaptureTargetPointSource.Auto;

        public string ObjectId
        {
            get => objectId;
            set => objectId = value;
        }

        public CaptureTargetPointSource TargetPointSource
        {
            get => targetPointSource;
            set => targetPointSource = value;
        }

        private void OnEnable()
        {
            CaptureRegistry.Instance.Register(this);
        }

        private void OnDisable()
        {
            CaptureRegistry.Instance.Unregister(this);
        }

        public Vector3 GetTargetPoint()
        {
            switch (targetPointSource)
            {
                case CaptureTargetPointSource.TransformPosition:
                    return transform.position;
                case CaptureTargetPointSource.RendererBoundsCenter:
                    return TryGetRendererBoundsCenter(out var rendererCenter) ? rendererCenter : transform.position;
                case CaptureTargetPointSource.ColliderBoundsCenter:
                    return TryGetColliderBoundsCenter(out var colliderCenter) ? colliderCenter : transform.position;
                case CaptureTargetPointSource.Auto:
                default:
                    if (TryGetRendererBoundsCenter(out rendererCenter))
                    {
                        return rendererCenter;
                    }

                    if (TryGetColliderBoundsCenter(out colliderCenter))
                    {
                        return colliderCenter;
                    }

                    return transform.position;
            }
        }

        public bool ContainsCollider(Collider collider)
        {
            if (collider == null)
            {
                return false;
            }

            var colliders = GetComponentsInChildren<Collider>(false);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == collider)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsCollider(Collider2D collider)
        {
            if (collider == null)
            {
                return false;
            }

            var colliders = GetComponentsInChildren<Collider2D>(false);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == collider)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetRendererBoundsCenter(out Vector3 center)
        {
            var renderers = GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0)
            {
                center = default;
                return false;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            center = bounds.center;
            return true;
        }

        private bool TryGetColliderBoundsCenter(out Vector3 center)
        {
            var colliders2D = GetComponentsInChildren<Collider2D>(false);
            if (colliders2D.Length > 0)
            {
                var bounds = colliders2D[0].bounds;
                for (var i = 1; i < colliders2D.Length; i++)
                {
                    bounds.Encapsulate(colliders2D[i].bounds);
                }

                center = bounds.center;
                return true;
            }

            var colliders3D = GetComponentsInChildren<Collider>(false);
            if (colliders3D.Length > 0)
            {
                var bounds = colliders3D[0].bounds;
                for (var i = 1; i < colliders3D.Length; i++)
                {
                    bounds.Encapsulate(colliders3D[i].bounds);
                }

                center = bounds.center;
                return true;
            }

            center = default;
            return false;
        }
    }
}
