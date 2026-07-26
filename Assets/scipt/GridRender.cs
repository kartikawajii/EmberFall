using System.Collections.Generic;
using UnityEngine;
using GridPuzzle.Core;

namespace GridPuzzle.Rendering
{
    
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

        
        private readonly Dictionary<Vector2Int, SpriteRenderer> _tiles = new();
        private Vector2Int _exitPosition;

        
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

        
        public void SetTileVisual(Vector2Int pos, TileState state)
        {
            if (!_tiles.TryGetValue(pos, out var renderer))
            {
                Debug.LogWarning($"GridRenderer: no tile cached at {pos}");
                return;
            }

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