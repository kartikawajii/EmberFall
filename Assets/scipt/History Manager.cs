using System.Collections.Generic;
using UnityEngine;

namespace GridPuzzle.History
{
    public struct MoveDelta
    {
        public Vector2Int PreviousPosition;
        public int PreviousMovesRemaining;
    }

    public class HistoryManager
    {
        private readonly Stack<MoveDelta> _history = new Stack<MoveDelta>();

        public bool CanUndo => _history.Count > 0;

        public void Record(Vector2Int previousPosition, int previousMovesRemaining)
        {
            _history.Push(new MoveDelta
            {
                PreviousPosition = previousPosition,
                PreviousMovesRemaining = previousMovesRemaining
            });
        }

        public bool TryUndo(out MoveDelta delta)
        {
            if (_history.Count == 0)
            {
                delta = default;
                return false;
            }
            delta = _history.Pop();
            return true;
        }

        public void Clear() => _history.Clear();
    }
}