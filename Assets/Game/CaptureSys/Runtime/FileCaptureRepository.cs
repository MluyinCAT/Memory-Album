using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MemoryAlbum.CaptureSys
{
    public sealed class FileCaptureRepository : ICaptureRepository
    {
        private readonly string rootDirectory;
        private readonly string photosDirectory;
        private readonly string manifestPath;

        private CaptureManifest manifest;
        private bool isLoaded;

        public FileCaptureRepository(string storageFolderName)
            : this(ResolveRootDirectory(storageFolderName), true)
        {
        }

        public static FileCaptureRepository CreateForAbsolutePath(string rootDirectory)
        {
            return new FileCaptureRepository(rootDirectory, true);
        }

        private FileCaptureRepository(string rootDirectory, bool _)
        {
            this.rootDirectory = rootDirectory;
            photosDirectory = Path.Combine(rootDirectory, "Photos");
            manifestPath = Path.Combine(rootDirectory, "captures.json");
        }

        public bool SaveCapture(byte[] photoBytes, IReadOnlyList<CapturedObjectSnapshot> capturedObjects, bool includeTimestamp, out CapturePhotoRecord photoRecord)
        {
            photoRecord = null;
            if (photoBytes == null || photoBytes.Length == 0)
            {
                return false;
            }

            EnsureLoaded();
            Directory.CreateDirectory(rootDirectory);
            Directory.CreateDirectory(photosDirectory);

            var workingManifest = CloneManifest(manifest);
            var sequence = Mathf.Max(1, workingManifest.nextSequence);
            var imageFileName = string.Empty;
            var finalImagePath = string.Empty;
            var tempImagePath = string.Empty;
            do
            {
                imageFileName = $"capture_{sequence:0000}.png";
                finalImagePath = Path.Combine(photosDirectory, imageFileName);
                tempImagePath = finalImagePath + ".tmp";
                sequence++;
            }
            while (File.Exists(finalImagePath) || File.Exists(tempImagePath));

            var tempManifestPath = manifestPath + ".tmp";

            var newRecord = BuildPhotoRecord(sequence - 1, imageFileName, capturedObjects, includeTimestamp);
            workingManifest.photos.Add(newRecord);
            workingManifest.nextSequence = sequence;

            var imageCommitted = false;
            try
            {
                File.WriteAllBytes(tempImagePath, photoBytes);
                File.WriteAllText(tempManifestPath, JsonUtility.ToJson(workingManifest, true));

                ReplaceFile(tempImagePath, finalImagePath);
                imageCommitted = true;
                ReplaceFile(tempManifestPath, manifestPath);

                manifest = workingManifest;
                photoRecord = newRecord;
                return true;
            }
            catch
            {
                TryDeleteFile(tempImagePath);
                TryDeleteFile(tempManifestPath);

                if (imageCommitted)
                {
                    TryDeleteFile(finalImagePath);
                }

                return false;
            }
        }

        public bool ClearCaptures(out int clearedPhotoCount)
        {
            EnsureLoaded();
            clearedPhotoCount = manifest?.photos?.Count ?? 0;

            try
            {
                if (Directory.Exists(photosDirectory))
                {
                    Directory.Delete(photosDirectory, true);
                }

                if (File.Exists(manifestPath))
                {
                    File.Delete(manifestPath);
                }

                manifest = new CaptureManifest();
                manifest.EnsureInitialized();
                isLoaded = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void EnsureLoaded()
        {
            if (isLoaded)
            {
                return;
            }

            if (File.Exists(manifestPath))
            {
                try
                {
                    manifest = JsonUtility.FromJson<CaptureManifest>(File.ReadAllText(manifestPath));
                }
                catch
                {
                    manifest = null;
                }
            }

            manifest ??= new CaptureManifest();
            manifest.EnsureInitialized();
            isLoaded = true;
        }

        private static CapturePhotoRecord BuildPhotoRecord(int sequence, string imageFileName, IReadOnlyList<CapturedObjectSnapshot> capturedObjects, bool includeTimestamp)
        {
            var photoRecord = new CapturePhotoRecord
            {
                sequence = sequence,
                imageFileName = imageFileName,
                capturedAtUtc = includeTimestamp ? DateTime.UtcNow.ToString("O") : string.Empty,
                capturedObjects = new List<CapturedObjectRecord>()
            };

            if (capturedObjects == null)
            {
                return photoRecord;
            }

            for (var i = 0; i < capturedObjects.Count; i++)
            {
                var capturedObject = capturedObjects[i];
                photoRecord.capturedObjects.Add(new CapturedObjectRecord
                {
                    objectId = capturedObject.ObjectId,
                    viewportOffset = capturedObject.ViewportOffset,
                    distanceFromCenter = capturedObject.DistanceFromCenter
                });
            }

            return photoRecord;
        }

        private static CaptureManifest CloneManifest(CaptureManifest source)
        {
            source ??= new CaptureManifest();
            source.EnsureInitialized();

            var clone = new CaptureManifest
            {
                nextSequence = source.nextSequence,
                photos = new List<CapturePhotoRecord>(source.photos.Count)
            };

            for (var i = 0; i < source.photos.Count; i++)
            {
                var sourcePhoto = source.photos[i];
                var photoClone = new CapturePhotoRecord
                {
                    sequence = sourcePhoto.sequence,
                    imageFileName = sourcePhoto.imageFileName,
                    capturedAtUtc = sourcePhoto.capturedAtUtc,
                    capturedObjects = new List<CapturedObjectRecord>()
                };

                if (sourcePhoto.capturedObjects != null)
                {
                    for (var j = 0; j < sourcePhoto.capturedObjects.Count; j++)
                    {
                        var sourceObject = sourcePhoto.capturedObjects[j];
                        photoClone.capturedObjects.Add(new CapturedObjectRecord
                        {
                            objectId = sourceObject.objectId,
                            viewportOffset = sourceObject.viewportOffset,
                            distanceFromCenter = sourceObject.distanceFromCenter
                        });
                    }
                }

                clone.photos.Add(photoClone);
            }

            return clone;
        }

        private static void ReplaceFile(string tempPath, string finalPath)
        {
            if (File.Exists(finalPath))
            {
                File.Delete(finalPath);
            }

            File.Move(tempPath, finalPath);
        }

        private static void TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string ResolveRootDirectory(string storageFolderName)
        {
            var folderName = string.IsNullOrWhiteSpace(storageFolderName) ? "CaptureSys" : storageFolderName;
            return Path.Combine(Application.persistentDataPath, folderName);
        }
    }
}
