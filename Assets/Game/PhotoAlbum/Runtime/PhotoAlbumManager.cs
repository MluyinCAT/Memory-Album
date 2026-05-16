using System.Collections.Generic;
using UnityEngine;
using VNovelizer.Core.API;

namespace MemoryAlbum.PhotoAlbum
{
    public sealed class PhotoAlbumManager : BaseManager<PhotoAlbumManager>
    {
        private const int TotalPhotoCount = 6;
        private const string CollectedFlagPrefix = "photo_collected_";
        private const string PuzzleUnlockedFlag = "puzzle_unlocked";

        public bool AllCollected => GetCollectedCount() >= TotalPhotoCount;
        public bool PuzzleUnlocked
        {
            get => VNAPI.GetBoolFlag(PuzzleUnlockedFlag);
            private set => VNAPI.SetBoolFlag(PuzzleUnlockedFlag, value);
        }

        public IReadOnlyList<string> AllPhotoIds { get; } = new[]
        {
            "photo_01", "photo_02", "photo_03",
            "photo_04", "photo_05", "photo_06"
        };

        public bool CollectPhoto(string photoId)
        {
            if (string.IsNullOrEmpty(photoId)) return false;
            if (IsPhotoCollected(photoId)) return false;

            VNAPI.SetBoolFlag(CollectedFlagPrefix + photoId, true);
            EventCenter.GetInstance().EventTrigger("PhotoCollected", photoId);

            if (AllCollected)
            {
                PuzzleUnlocked = true;
                EventCenter.GetInstance().EventTrigger("AllPhotosCollected");
            }

            return true;
        }

        public bool IsPhotoCollected(string photoId)
        {
            if (string.IsNullOrEmpty(photoId)) return false;
            return VNAPI.GetBoolFlag(CollectedFlagPrefix + photoId);
        }

        public int GetCollectedCount()
        {
            int count = 0;
            foreach (var id in AllPhotoIds)
                if (IsPhotoCollected(id)) count++;
            return count;
        }

        public List<string> GetCollectedPhotoIds()
        {
            var result = new List<string>();
            foreach (var id in AllPhotoIds)
                if (IsPhotoCollected(id)) result.Add(id);
            return result;
        }

        public void ResetAllPhotos()
        {
            foreach (var id in AllPhotoIds)
                VNAPI.SetBoolFlag(CollectedFlagPrefix + id, false);
            VNAPI.SetBoolFlag(PuzzleUnlockedFlag, false);
            VNAPI.SetBoolFlag("photo_session_started", false);
            VNAPI.SetBoolFlag("read_doll", false);
            VNAPI.SetBoolFlag("tutorial_shown", false);
        }
    }
}
