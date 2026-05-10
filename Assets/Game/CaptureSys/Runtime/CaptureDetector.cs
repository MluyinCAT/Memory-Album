using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryAlbum.CaptureSys
{
    public sealed class CaptureDetector : ICaptureDetector
    {
        private readonly ICaptureRegistry registry;
        private readonly int occlusionLayerMask;

        public CaptureDetector(ICaptureRegistry registry, int occlusionLayerMask)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.occlusionLayerMask = occlusionLayerMask;
        }

        public CaptureDetectionResult Detect(Camera camera)
        {
            if (camera == null)
            {
                return CaptureDetectionResult.Empty;
            }

            var visibleObjects = new List<CapturedObjectSnapshot>();
            foreach (var captureObj in registry.ActiveObjects)
            {
                if (captureObj == null || !captureObj.isActiveAndEnabled || string.IsNullOrWhiteSpace(captureObj.ObjectId))
                {
                    continue;
                }

                var targetPoint = captureObj.GetTargetPoint();
                if (!IsWithinCaptureArea(camera, targetPoint, out var viewportOffset))
                {
                    continue;
                }

                if (IsOccluded(camera, captureObj, targetPoint))
                {
                    continue;
                }

                visibleObjects.Add(new CapturedObjectSnapshot(captureObj, captureObj.ObjectId, viewportOffset, viewportOffset.magnitude));
            }

            return visibleObjects.Count == 0 ? CaptureDetectionResult.Empty : new CaptureDetectionResult(visibleObjects);
        }

        private bool IsOccluded(Camera camera, CaptureObj captureObj, Vector3 targetPoint)
        {
            return camera.orthographic
                ? IsOccludedInOrthographicView(camera, captureObj, targetPoint)
                : IsOccludedInPerspectiveView(camera, captureObj, targetPoint);
        }

        private bool IsOccludedInOrthographicView(Camera camera, CaptureObj captureObj, Vector3 targetPoint)
        {
            return IsOccludedBy2DDepthStack(camera, captureObj, targetPoint) || IsOccludedBy3DParallelRay(camera, captureObj, targetPoint);
        }

        private bool IsOccludedInPerspectiveView(Camera camera, CaptureObj captureObj, Vector3 targetPoint)
        {
            return IsOccludedBy2DPerspectiveRay(camera, captureObj, targetPoint) || IsOccludedBy3DPerspectiveRay(camera, captureObj, targetPoint);
        }

        private bool IsOccludedBy2DDepthStack(Camera camera, CaptureObj captureObj, Vector3 targetPoint)
        {
            var targetDepth = GetViewDepth(camera, targetPoint);
            if (targetDepth <= Mathf.Epsilon)
            {
                return false;
            }

            var hits = Physics2D.OverlapPointAll(new Vector2(targetPoint.x, targetPoint.y), occlusionLayerMask);
            for (var i = 0; i < hits.Length; i++)
            {
                var hitCollider = hits[i];
                if (hitCollider == null || captureObj.ContainsCollider(hitCollider))
                {
                    continue;
                }

                var hitDepth = GetViewDepth(camera, hitCollider.bounds.center);
                if (hitDepth > Mathf.Epsilon && hitDepth < targetDepth)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsOccludedBy3DParallelRay(Camera camera, CaptureObj captureObj, Vector3 targetPoint)
        {
            var direction = -camera.transform.forward;
            var distance = GetViewDepth(camera, targetPoint);
            if (distance <= Mathf.Epsilon)
            {
                return false;
            }

            var hits = Physics.RaycastAll(targetPoint, direction, distance, occlusionLayerMask, QueryTriggerInteraction.Collide);
            Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));

            for (var i = 0; i < hits.Length; i++)
            {
                var hitCollider = hits[i].collider;
                if (hitCollider == null || captureObj.ContainsCollider(hitCollider))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool IsOccludedBy2DPerspectiveRay(Camera camera, CaptureObj captureObj, Vector3 targetPoint)
        {
            var origin = new Vector2(targetPoint.x, targetPoint.y);
            var destination = new Vector2(camera.transform.position.x, camera.transform.position.y);
            var delta = destination - origin;
            var distance = delta.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return false;
            }

            var hits = Physics2D.RaycastAll(origin, delta / distance, distance, occlusionLayerMask);
            for (var i = 0; i < hits.Length; i++)
            {
                var hitCollider = hits[i].collider;
                if (hitCollider == null || captureObj.ContainsCollider(hitCollider))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool IsOccludedBy3DPerspectiveRay(Camera camera, CaptureObj captureObj, Vector3 targetPoint)
        {
            var direction = camera.transform.position - targetPoint;
            var distance = direction.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return false;
            }

            var hits = Physics.RaycastAll(targetPoint, direction / distance, distance, occlusionLayerMask, QueryTriggerInteraction.Collide);
            Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));

            for (var i = 0; i < hits.Length; i++)
            {
                var hitCollider = hits[i].collider;
                if (hitCollider == null)
                {
                    continue;
                }

                if (captureObj.ContainsCollider(hitCollider))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static float GetViewDepth(Camera camera, Vector3 worldPoint)
        {
            return Vector3.Dot(worldPoint - camera.transform.position, camera.transform.forward);
        }

        private static bool IsWithinCaptureArea(Camera camera, Vector3 worldPoint, out Vector2 viewportOffset)
        {
            var viewportPoint = camera.WorldToViewportPoint(worldPoint);
            viewportOffset = new Vector2(viewportPoint.x - 0.5f, viewportPoint.y - 0.5f);

            if (viewportPoint.z < camera.nearClipPlane)
            {
                return false;
            }

            if (camera.orthographic)
            {
                var cameraPosition = camera.transform.position;
                var halfHeight = camera.orthographicSize;
                var halfWidth = halfHeight * camera.aspect;
                var insideWorldRect =
                    worldPoint.x >= cameraPosition.x - halfWidth &&
                    worldPoint.x <= cameraPosition.x + halfWidth &&
                    worldPoint.y >= cameraPosition.y - halfHeight &&
                    worldPoint.y <= cameraPosition.y + halfHeight;
                if (!insideWorldRect)
                {
                    return false;
                }
            }

            return viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
        }
    }
}
