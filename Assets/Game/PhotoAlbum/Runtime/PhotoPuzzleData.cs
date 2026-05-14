using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryAlbum.PhotoAlbum
{
    [CreateAssetMenu(fileName = "PhotoPuzzleData", menuName = "Memory Album/Photo Puzzle Data")]
    public sealed class PhotoPuzzleData : ScriptableObject
    {
        public List<ValidSequence> validSequences = new List<ValidSequence>();

        public ValidSequence MatchSequence(IReadOnlyList<string> currentOrder)
        {
            foreach (var seq in validSequences)
            {
                if (seq.photoOrder == null || seq.photoOrder.Length != currentOrder.Count)
                    continue;

                bool match = true;
                for (int i = 0; i < currentOrder.Count; i++)
                {
                    if (currentOrder[i] != seq.photoOrder[i])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return seq;
            }
            return null;
        }
    }

    [Serializable]
    public sealed class ValidSequence
    {
        [Tooltip("按顺序排列的6个photoId")]
        public string[] photoOrder = new string[6];

        [Tooltip("对应结局剧本名")]
        public string endingScriptName;
    }
}
