using UnityEngine;

namespace Core
{
    public class GridModel
    {
        private readonly Cell[,] _cells;

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public Vector2Int PlayerPosition { get; private set; }
        public Vector2Int ExitPosition { get; private set; }
        public int MovesRemaining { get; private set; }

        public GridModel(int columns, int rows, Vector2Int startPosition, Vector2Int exitPosition, int moveLimit)
        {
            Columns = columns;
            Rows = rows;
            PlayerPosition = startPosition;
            ExitPosition = exitPosition;
            MovesRemaining = moveLimit;

            _cells = new Cell[columns, rows];
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    _cells[x, y] = new Cell(TileState.Solid);
                }
            }

            if (InBounds(exitPosition))
            {
                SetTileState(exitPosition, TileState.Exit);
            }
        }

        public MoveResult TryMove(Direction dir)
        {
            if (MovesRemaining <= 0)
            {
                return new MoveResult(
                    success: false,
                    fromPosition: PlayerPosition,
                    toPosition: PlayerPosition,
                    hitLava: false,
                    reachedExit: false,
                    outOfMoves: true,
                    movesRemaining: MovesRemaining
                );
            }

            Vector2Int delta = GetDirectionVector(dir);
            Vector2Int targetPos = PlayerPosition + delta;

            if (!InBounds(targetPos))
            {
                return new MoveResult(
                    success: false,
                    fromPosition: PlayerPosition,
                    toPosition: targetPos,
                    hitLava: false,
                    reachedExit: false,
                    outOfMoves: MovesRemaining == 0,
                    movesRemaining: MovesRemaining
                );
            }

            Vector2Int fromPos = PlayerPosition;
            PlayerPosition = targetPos;
            MovesRemaining--;

            TileState tileState = GetTileState(targetPos);
            bool reachedExit = (targetPos == ExitPosition || tileState == TileState.Exit);
            bool hitLava = (tileState == TileState.Lava);
            bool outOfMoves = (MovesRemaining == 0 && !reachedExit);

            return new MoveResult(
                success: true,
                fromPosition: fromPos,
                toPosition: targetPos,
                hitLava: hitLava,
                reachedExit: reachedExit,
                outOfMoves: outOfMoves,
                movesRemaining: MovesRemaining
            );
        }

        public void RestoreState(Vector2Int position, int movesRemaining)
        {
            PlayerPosition = position;
            MovesRemaining = movesRemaining;
        }

        public bool InBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < Columns && pos.y >= 0 && pos.y < Rows;
        }

        public TileState GetTileState(Vector2Int pos)
        {
            if (!InBounds(pos))
            {
                return TileState.Empty;
            }
            return _cells[pos.x, pos.y].State;
        }

        public void SetTileState(Vector2Int pos, TileState state)
        {
            if (InBounds(pos))
            {
                _cells[pos.x, pos.y] = new Cell(state);
            }
        }

        private static Vector2Int GetDirectionVector(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return new Vector2Int(0, 1);
                case Direction.Down:
                    return new Vector2Int(0, -1);
                case Direction.Left:
                    return new Vector2Int(-1, 0);
                case Direction.Right:
                    return new Vector2Int(1, 0);
                default:
                    return Vector2Int.zero;
            }
        }
    }
}