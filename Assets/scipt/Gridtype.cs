using UnityEngine;

namespace Core
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum TileState
    {
        Solid,
        Lava,
        Exit,
        Empty
    }

    public struct Cell
    {
        public TileState State { get; set; }

        public Cell(TileState state)
        {
            State = state;
        }
    }

    public struct MoveResult
    {
        public bool Success { get; }
        public Vector2Int FromPosition { get; }
        public Vector2Int ToPosition { get; }
        public bool HitLava { get; }
        public bool ReachedExit { get; }
        public bool OutOfMoves { get; }
        public int MovesRemaining { get; }

        public MoveResult(
            bool success,
            Vector2Int fromPosition,
            Vector2Int toPosition,
            bool hitLava,
            bool reachedExit,
            bool outOfMoves,
            int movesRemaining)
        {
            Success = success;
            FromPosition = fromPosition;
            ToPosition = toPosition;
            HitLava = hitLava;
            ReachedExit = reachedExit;
            OutOfMoves = outOfMoves;
            MovesRemaining = movesRemaining;
        }
    }
}