using UnityEngine;

namespace GridPuzzle.Gameplay
{
    public static class GridCoordinateUtil
    {
        public static Vector3 GridToWorld(Vector2Int gridPos, float cellSize)
        {
            return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0f);
        }

        public static Vector2Int WorldToGrid(Vector3 worldPos, float cellSize)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.y / cellSize);
            return new Vector2Int(x, y);
        }
    }
}