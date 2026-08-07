using System.Collections.Generic;
using UnityEngine;

namespace GridPuzzle.Gameplay
{
    /// <summary>
    /// Plain C# class, no MonoBehaviour. Tracks which cells have gems and
    /// which have been collected. GameController asks TryCollect() after
    /// each move; GemRenderer is told separately to hide the visual.
    /// </summary>
    public class GemSystem
    {
        private readonly HashSet<Vector2Int> _gemPositions;
        private readonly HashSet<Vector2Int> _collected = new HashSet<Vector2Int>();

        public GemSystem(IEnumerable<Vector2Int> gemPositions)
        {
            _gemPositions = new HashSet<Vector2Int>(gemPositions);
        }

        public int TotalCount => _gemPositions.Count;
        public int CollectedCount => _collected.Count;
        public bool AllCollected => _collected.Count >= _gemPositions.Count;

        public bool TryCollect(Vector2Int pos)
        {
            if (!_gemPositions.Contains(pos)) return false;
            if (_collected.Contains(pos)) return false;
            _collected.Add(pos);
            return true;
        }

        public void Reset() => _collected.Clear();
    }
}