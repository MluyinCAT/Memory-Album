using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace MemoryAlbum.CaptureSys.Tests
{
    public sealed class CaptureSystemTests
    {
        private readonly List<Object> createdObjects = new List<Object>();
        private string tempDirectory;

        [SetUp]
        public void SetUp()
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "MemoryAlbumCaptureTests", System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            createdObjects.Clear();

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void Detect_ReturnsVisibleCaptureObject_WhenInsideOrthographicRange()
        {
            var camera = CreateCamera(Vector3.back * 10f);
            var captureObj = CreateCaptureObject("apple", Vector3.zero, add2DCollider: true, add3DCollider: false);

            var detector = new CaptureDetector(CaptureRegistry.Instance, ~0);
            SyncPhysics();

            var result = detector.Detect(camera);

            Assert.That(result.HasVisibleObjects, Is.True);
            Assert.That(result.VisibleObjects.Count, Is.EqualTo(1));
            Assert.That(result.VisibleObjects[0].ObjectId, Is.EqualTo(captureObj.ObjectId));
        }

        [Test]
        public void Detect_FiltersOccluded2DObject()
        {
            var camera = CreateCamera(Vector3.back * 10f);
            CreateCaptureObject("blocked", Vector3.zero, add2DCollider: true, add3DCollider: false);
            CreateOccluder2D(Vector3.zero, -5f);

            var detector = new CaptureDetector(CaptureRegistry.Instance, ~0);
            SyncPhysics();

            var result = detector.Detect(camera);

            Assert.That(result.HasVisibleObjects, Is.False);
        }

        [Test]
        public void Detect_ReturnsMultipleObjects_WhenTheyShareTheFrame()
        {
            var camera = CreateCamera(Vector3.back * 10f);
            CreateCaptureObject("a", new Vector3(-1f, 0f, 0f), add2DCollider: true, add3DCollider: false);
            CreateCaptureObject("b", new Vector3(1f, 0.25f, 0f), add2DCollider: true, add3DCollider: false);

            var detector = new CaptureDetector(CaptureRegistry.Instance, ~0);
            SyncPhysics();

            var result = detector.Detect(camera);

            Assert.That(result.VisibleObjects.Count, Is.EqualTo(2));
        }

        [Test]
        public void Detect_ComputesViewportOffset_FromCameraCenter()
        {
            var camera = CreateCamera(Vector3.back * 10f);
            CreateCaptureObject("edge", new Vector3(5f, 0f, 0f), add2DCollider: true, add3DCollider: false);

            var detector = new CaptureDetector(CaptureRegistry.Instance, ~0);
            SyncPhysics();

            var result = detector.Detect(camera);

            Assert.That(result.VisibleObjects.Count, Is.EqualTo(1));
            Assert.That(result.VisibleObjects[0].ViewportOffset.x, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(result.VisibleObjects[0].ViewportOffset.y, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Repository_PersistsSequenceAndManifest()
        {
            var firstRepository = FileCaptureRepository.CreateForAbsolutePath(tempDirectory);
            var firstObjects = new List<CapturedObjectSnapshot>
            {
                new CapturedObjectSnapshot(null, "photo-1", Vector2.zero, 0f)
            };

            var firstSaveSucceeded = firstRepository.SaveCapture(new byte[] { 1, 2, 3 }, firstObjects, true, out var firstRecord);
            var secondRepository = FileCaptureRepository.CreateForAbsolutePath(tempDirectory);
            var secondSaveSucceeded = secondRepository.SaveCapture(new byte[] { 4, 5, 6 }, firstObjects, true, out var secondRecord);

            Assert.That(firstSaveSucceeded, Is.True);
            Assert.That(secondSaveSucceeded, Is.True);
            Assert.That(firstRecord.imageFileName, Is.EqualTo("capture_0001.png"));
            Assert.That(secondRecord.imageFileName, Is.EqualTo("capture_0002.png"));

            var manifestPath = Path.Combine(tempDirectory, "captures.json");
            var manifest = JsonUtility.FromJson<CaptureManifest>(File.ReadAllText(manifestPath));
            Assert.That(manifest.nextSequence, Is.EqualTo(3));
            Assert.That(manifest.photos.Count, Is.EqualTo(2));
        }

        [Test]
        public void Repository_ClearCaptures_RemovesSavedPhotosAndResetsSequence()
        {
            var repository = FileCaptureRepository.CreateForAbsolutePath(tempDirectory);
            var capturedObjects = new List<CapturedObjectSnapshot>
            {
                new CapturedObjectSnapshot(null, "photo-1", Vector2.zero, 0f)
            };

            repository.SaveCapture(new byte[] { 1, 2, 3 }, capturedObjects, true, out _);
            repository.SaveCapture(new byte[] { 4, 5, 6 }, capturedObjects, true, out _);

            var clearSucceeded = repository.ClearCaptures(out var clearedPhotoCount);
            var thirdRepository = FileCaptureRepository.CreateForAbsolutePath(tempDirectory);
            var thirdSaveSucceeded = thirdRepository.SaveCapture(new byte[] { 7, 8, 9 }, capturedObjects, true, out var thirdRecord);

            Assert.That(clearSucceeded, Is.True);
            Assert.That(clearedPhotoCount, Is.EqualTo(2));
            Assert.That(thirdSaveSucceeded, Is.True);
            Assert.That(thirdRecord.imageFileName, Is.EqualTo("capture_0001.png"));
        }

        [Test]
        public void Detect_SupportsRendererOnlyCaptureObject()
        {
            var camera = CreateCamera(Vector3.back * 10f);
            CreateRendererOnlyCaptureObject("renderer-only", Vector3.zero);

            var detector = new CaptureDetector(CaptureRegistry.Instance, ~0);
            SyncPhysics();

            var result = detector.Detect(camera);

            Assert.That(result.VisibleObjects.Count, Is.EqualTo(1));
        }

        [Test]
        public void Detect_FiltersOccluded3DObject_InMixedColliderScene()
        {
            var camera = CreateCamera(Vector3.back * 10f);
            CreateCaptureObject("mixed", new Vector3(2f, 0f, 0f), add2DCollider: false, add3DCollider: true);
            CreateOccluder3D(new Vector3(2f, 0f, -5f));

            var detector = new CaptureDetector(CaptureRegistry.Instance, ~0);
            SyncPhysics();

            var result = detector.Detect(camera);

            Assert.That(result.HasVisibleObjects, Is.False);
        }

        private Camera CreateCamera(Vector3 position)
        {
            var gameObject = new GameObject("TestCamera");
            createdObjects.Add(gameObject);

            var camera = gameObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.aspect = 1f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.transform.position = position;
            return camera;
        }

        private CaptureObj CreateCaptureObject(string objectId, Vector3 position, bool add2DCollider, bool add3DCollider)
        {
            var gameObject = new GameObject(objectId);
            createdObjects.Add(gameObject);
            gameObject.transform.position = position;

            if (add2DCollider)
            {
                gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
            }

            if (add3DCollider)
            {
                gameObject.AddComponent<BoxCollider>().size = Vector3.one;
            }

            var captureObj = gameObject.AddComponent<CaptureObj>();
            captureObj.ObjectId = objectId;
            captureObj.TargetPointSource = CaptureTargetPointSource.TransformPosition;
            return captureObj;
        }

        private void CreateRendererOnlyCaptureObject(string objectId, Vector3 position)
        {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
            createdObjects.Add(primitive);
            primitive.name = objectId;
            primitive.transform.position = position;

            var primitiveCollider = primitive.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            var captureObj = primitive.AddComponent<CaptureObj>();
            captureObj.ObjectId = objectId;
            captureObj.TargetPointSource = CaptureTargetPointSource.Auto;
        }

        private void CreateOccluder2D(Vector3 position, float zPosition)
        {
            var gameObject = new GameObject("Occluder2D");
            createdObjects.Add(gameObject);
            gameObject.transform.position = new Vector3(position.x, position.y, zPosition);
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 3f);
        }

        private void CreateOccluder3D(Vector3 position)
        {
            var gameObject = new GameObject("Occluder3D");
            createdObjects.Add(gameObject);
            gameObject.transform.position = position;
            gameObject.AddComponent<BoxCollider>().size = new Vector3(3f, 3f, 0.5f);
        }

        private static void SyncPhysics()
        {
            Physics2D.SyncTransforms();
            Physics.SyncTransforms();
        }
    }
}
