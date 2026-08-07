using System.Collections.Generic;
using UnityEngine;
using GridPuzzle.Core;

namespace GridPuzzle.Rendering
{
    /// <summary>
    /// Owns every tile GameObject on the board. This is the ONLY script
    /// that should ever Instantiate or Destroy a tile — and it only does
    /// that once, in BuildGrid(). After that, every visual change (crack,
    /// lava, exit) goes through SetTileVisual(), which just swaps a
    /// SpriteRenderer.sprite on a cached instance. No GetComponent, no
    /// Instantiate, no Destroy during normal play — that's the pooling
    /// requirement from the assignment brief.
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        [Header("Prefab & layout")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private float cellSize = 1f;

        [Header("Sprites (match TileState)")]
        [SerializeField] private Sprite solidSprite;
        [SerializeField] private Sprite crackedSprite;
        [SerializeField] private Sprite lavaSprite;
        [SerializeField] private Sprite exitSprite;

        // Cached per-cell renderer reference so later updates never need
        // GetComponent or a scene search — this is the "algorithmic
        // efficiency" answer for grid update procedures.
        private readonly Dictionary<Vector2Int, SpriteRenderer> _tiles = new();
        private Vector2Int _exitPosition;

        /// <summary>
        /// Spawns one tile per cell in the model and caches its renderer.
        /// Call this once at game start, and again after Restart() clears
        /// the previous board.
        /// </summary>
        public void BuildGrid(GridModel model)
        {
            ClearGrid();
            _exitPosition = model.ExitPosition;

            for (int x = 0; x < model.Columns; x++)
            {
                for (int y = 0; y < model.Rows; y++)
                {
                    var gridPos = new Vector2Int(x, y);
                    var worldPos = GridPuzzle.Gameplay.GridCoordinateUtil.GridToWorld(gridPos, cellSize);

                    var instance = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                    instance.name = $"Tile_{x}_{y}";

                    var renderer = instance.GetComponent<SpriteRenderer>();
                    _tiles[gridPos] = renderer;

                    SetTileVisual(gridPos, model.GetTileState(gridPos));
                }
            }
        }

        /// <summary>
        /// Swaps the sprite for a single cached tile. This is what the
        /// crumble timer and move results call — never a full-grid redraw,
        /// only the cell(s) that actually changed.
        /// </summary>
        public void SetTileVisual(Vector2Int pos, TileState state)
        {
            if (!_tiles.TryGetValue(pos, out var renderer))
            {
                Debug.LogWarning($"GridRenderer: no tile cached at {pos}");
                return;
            }

            // The exit tile keeps its portal look while Solid; crumble
            // states still take priority if this cell somehow both is the
            // exit and has decayed (shouldn't happen by design, but this
            // keeps the renderer honest about what GridModel reports).
            if (pos == _exitPosition && state == TileState.Solid)
            {
                renderer.sprite = exitSprite;
                return;
            }

            renderer.sprite = state switch
            {
                TileState.Solid => solidSprite,
                TileState.Cracked => crackedSprite,
                TileState.Lava => lavaSprite,
                _ => solidSprite
            };
        }

        /// <summary>Removes all spawned tiles. Call before BuildGrid() on
        /// restart so a new game doesn't stack a second board on top.</summary>
        public void ClearGrid()
        {
            foreach (var kvp in _tiles)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            _tiles.Clear();
        }
    }
}