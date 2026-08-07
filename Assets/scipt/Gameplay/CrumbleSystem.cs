using System;
using System.Collections.Generic;
using UnityEngine;
using GridPuzzle.Core;

namespace GridPuzzle.Gameplay
{
    /// <summary>
    /// Plain C# class, no MonoBehaviour. Tracks when each cell was left and
    /// reports state transitions (Solid -> Cracked -> Lava) as time passes.
    /// GameController owns the actual GridModel; this only decides WHEN a
    /// tile should change, GridModel.SetTileState still does the mutation.
    /// </summary>
    public class CrumbleSystem
    {
        private const float CrackAfterSeconds = 1f;
        private const float LavaAfterSeconds = 2f;

        private readonly Dictionary<Vector2Int, float> _leaveTimes = new();

        /// <summary>Call when the player steps OFF a cell.</summary>
        public void MarkLeft(Vector2Int pos, float currentTime) => _leaveTimes[pos] = currentTime;

        /// <summary>Call when the player steps back ONTO a cell — stops its timer.</summary>
        public void MarkStabilized(Vector2Int pos) => _leaveTimes.Remove(pos);

        /// <summary>Call every frame. Reports each cell that just changed state
        /// via onStateChanged so the caller can repaint just that tile.</summary>
        public void Tick(float currentTime, GridModel model, Action<Vector2Int, TileState> onStateChanged)
        {
            List<Vector2Int> finished = null;

            foreach (var kvp in _leaveTimes)
            {
                float elapsed = currentTime - kvp.Value;
                TileState current = model.GetTileState(kvp.Key);
                TileState next = current;

                if (elapsed >= LavaAfterSeconds) next = TileState.Lava;
                else if (elapsed >= CrackAfterSeconds) next = TileState.Cracked;

                if (next != current)
                {
                    model.SetTileState(kvp.Key, next);
                    onStateChanged(kvp.Key, next);

                    if (next == TileState.Lava)
                        (finished ??= new List<Vector2Int>()).Add(kvp.Key); // Lava is terminal, stop tracking it
                }
            }

            if (finished != null)
                foreach (var pos in finished) _leaveTimes.Remove(pos);
        }

        public void Clear() => _leaveTimes.Clear();
    }
}