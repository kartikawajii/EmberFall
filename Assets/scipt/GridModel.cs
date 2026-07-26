using System;
using UnityEngine;

namespace GridPuzzle.Core
{
    public class GridModel
    {
        public int Columns { get; }
        public int Rows { get; }
        public Vector2Int PlayerPosition { get; private set; }
        public Vector2Int ExitPosition { get; }
        public int MovesRemaining { get; private set; }

        private readonly Cell[,] _cells;

        public GridModel(int columns, int rows, Vector2Int startPosition, Vector2Int exitPosition, int moveLimit)
        {
            if (columns <= 0 || rows <= 0)
                throw new ArgumentException("Grid dimensions must be positive.");

            Columns = columns;
            Rows = rows;
            PlayerPosition = startPosition;
            ExitPosition = exitPosition;
            MovesRemaining = moveLimit;

            _cells = new Cell[columns, rows];
            for (int x = 0; x < columns; x++)
                for (int y = 0; y < rows; y++)
                    _cells[x, y] = new Cell(TileState.Solid);
        }

        public bool InBounds(Vector2Int pos)
            => pos.x >= 0 && pos.x < Columns && pos.y >= 0 && pos.y < Rows;

        public TileState GetTileState(Vector2Int pos)
        {
            if (!InBounds(pos)) throw new ArgumentOutOfRangeException(nameof(pos));
            return _cells[pos.x, pos.y].State;
        }

        public void SetTileState(Vector2Int pos, TileState state)
        {
            if (!InBounds(pos)) throw new ArgumentOutOfRangeException(nameof(pos));
            _cells[pos.x, pos.y] = new Cell(state);
        }

        private static Vector2Int DirectionToOffset(Direction dir) => dir switch
        {
            Direction.Up    => new Vector2Int(0, 1),
            Direction.Down  => new Vector2Int(0, -1),
            Direction.Left  => new Vector2Int(-1, 0),
            Direction.Right => new Vector2Int(1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(dir))
        };

        public MoveResult TryMove(Direction dir)
        {
            var from = PlayerPosition;

            if (MovesRemaining <= 0)
                return new MoveResult { Success = false, FromPosition = from, ToPosition = from, OutOfMoves = true, MovesRemaining = MovesRemaining };

            var target = from + DirectionToOffset(dir);

            if (!InBounds(target))
                return new MoveResult { Success = false, FromPosition = from, ToPosition = from, MovesRemaining = MovesRemaining };

            bool targetIsLava = GetTileState(target) == TileState.Lava;

            PlayerPosition = target;
            MovesRemaining -= 1;

            return new MoveResult
            {
                Success = true,
                FromPosition = from,
                ToPosition = target,
                HitLava = targetIsLava,
                ReachedExit = target == ExitPosition,
                OutOfMoves = MovesRemaining <= 0,
                MovesRemaining = MovesRemaining
            };
        }

        /// <summary>Used only by HistoryManager to rewind state on Undo.</summary>
        public void RestoreState(Vector2Int position, int movesRemaining)
        {
            PlayerPosition = position;
            MovesRemaining = movesRemaining;
        }
    }
}