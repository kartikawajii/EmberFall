using System;
using UnityEngine; // only for Vector2Int — a plain value struct, not a scene object

namespace GridPuzzle.Core
{
    public enum Direction { Up, Down, Left, Right }

    public enum TileState { Solid, Cracked, Lava }

    [Serializable]
    public struct Cell
    {
        public TileState State;
        public Cell(TileState state) { State = state; }
    }

    public struct MoveResult
    {
        public bool Success;
        public Vector2Int FromPosition;
        public Vector2Int ToPosition;
        public bool HitLava;
        public bool ReachedExit;
        public bool OutOfMoves;
        public int MovesRemaining;
    }
}