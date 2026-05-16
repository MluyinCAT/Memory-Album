using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryAlbum.PhotoAlbum
{
    [CreateAssetMenu(fileName = "PhotoPuzzleData", menuName = "Memory Album/Photo Puzzle Data")]
    public sealed class PhotoPuzzleData : ScriptableObject
    {
        public List<ValidSequence> validSequences = new List<ValidSequence>();
        public List<ErrorCheck> errorChecks = new List<ErrorCheck>();
        public string requiredItemFlag = "read_doll"; // 必须阅读过 娃娃 才能通关
        public string requiredItemHint =
            "如果这么想的话，兴许有点道理——\n不，还差一个关键性证据——不然下定论太早了。\n再调查下房间吧。";

        public ValidSequence MatchSequence(IReadOnlyList<string> currentOrder)
        {
            foreach (var seq in validSequences)
            {
                if (seq.photoOrder == null || seq.photoOrder.Length != currentOrder.Count)
                    continue;
                bool match = true;
                for (int i = 0; i < currentOrder.Count; i++)
                {
                    if (currentOrder[i] != seq.photoOrder[i]) { match = false; break; }
                }
                if (match) return seq;
            }
            return null;
        }

        /// <summary>
        /// 返回按优先级排序的错误提示列表。返回空表示没有错误。
        /// </summary>
        public List<string> GetErrorHints(IReadOnlyList<string> order)
        {
            var hints = new List<string>();
            var sorted = new List<ErrorCheck>(errorChecks);
            sorted.Sort((a, b) => a.priority.CompareTo(b.priority));

            foreach (var check in sorted)
            {
                if (EvaluateCondition(check.condition, order))
                {
                    if (check.hints != null && check.hints.Count > 0)
                        hints.AddRange(check.hints);
                    break; // 只返回第一个命中的检查
                }
            }
            return hints;
        }

        private bool EvaluateCondition(string condition, IReadOnlyList<string> order)
        {
            if (string.IsNullOrEmpty(condition)) return false;
            var parts = condition.Split('|');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                bool negate = false;
                if (trimmed.StartsWith("!")) { negate = true; trimmed = trimmed.Substring(1).Trim(); }

                bool result = false;
                if (trimmed.StartsWith("pos:"))
                    result = CheckPosition(trimmed.Substring(4), order);
                else if (trimmed.StartsWith("adjacent:"))
                    result = CheckAdjacent(trimmed.Substring(9), order);
                else if (trimmed.StartsWith("before:"))
                    result = CheckBefore(trimmed.Substring(7), order);
                else if (trimmed.StartsWith("after:"))
                    result = CheckAfter(trimmed.Substring(6), order);
                else if (trimmed.StartsWith("exact:"))
                    result = CheckExact(trimmed.Substring(6), order);

                if (negate ? !result : result) return true;
            }
            return false;
        }

        private bool CheckPosition(string args, IReadOnlyList<string> order)
        {
            var argParts = args.Split(',');
            if (argParts.Length < 2) return false;
            string photoId = argParts[0].Trim();
            int expectedIndex = int.Parse(argParts[1].Trim());
            for (int i = 0; i < order.Count; i++)
            {
                if (order[i] == photoId) return i == expectedIndex;
            }
            return false; // not found
        }

        private bool CheckAdjacent(string args, IReadOnlyList<string> order)
        {
            var argParts = args.Split(',');
            if (argParts.Length < 2) return false;
            string a = argParts[0].Trim(), b = argParts[1].Trim();
            int ia = IndexOf(order, a), ib = IndexOf(order, b);
            if (ia < 0 || ib < 0) return false;
            return Mathf.Abs(ia - ib) == 1 && ia < ib;
        }

        private bool CheckBefore(string args, IReadOnlyList<string> order)
        {
            var argParts = args.Split(',');
            if (argParts.Length < 2) return false;
            string a = argParts[0].Trim(), b = argParts[1].Trim();
            int ia = IndexOf(order, a), ib = IndexOf(order, b);
            if (ia < 0 || ib < 0) return false;
            return ia < ib;
        }

        private bool CheckAfter(string args, IReadOnlyList<string> order)
        {
            var argParts = args.Split(',');
            if (argParts.Length < 2) return false;
            string a = argParts[0].Trim(), b = argParts[1].Trim();
            int ia = IndexOf(order, a), ib = IndexOf(order, b);
            if (ia < 0 || ib < 0) return false;
            return ia > ib;
        }

        private bool CheckExact(string args, IReadOnlyList<string> order)
        {
            var ids = args.Split(',');
            if (ids.Length != order.Count) return false;
            for (int i = 0; i < order.Count; i++)
                if (order[i].Trim() != ids[i].Trim()) return false;
            return true;
        }

        private int IndexOf(IReadOnlyList<string> order, string id)
        {
            for (int i = 0; i < order.Count; i++)
                if (order[i] == id) return i;
            return -1;
        }
    }

    [Serializable]
    public sealed class ValidSequence
    {
        public string[] photoOrder = new string[6];
        public string endingScriptName;
    }

    [Serializable]
    public sealed class ErrorCheck
    {
        [Tooltip("优先级，数字越小越先检查")]
        public int priority;

        [Tooltip("条件表达式：pos:id,index | adjacent:a,b | before:a,b | after:a,b | exact:id1,id2,...")]
        public string condition;

        [Tooltip("错误时显示的提示文本，每行一条")]
        public List<string> hints = new List<string>();
    }
}
