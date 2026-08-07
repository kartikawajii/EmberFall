using System.Collections.Generic;
using UnityEngine;

namespace GridPuzzle.Rendering
{
    /// <summary>
    /// Spawns one sprite per gem position and hides it when collected.
    /// No prefab required — same pattern as the tile grid, just simpler:
    /// creates a plain SpriteRenderer GameObject per gem in code.
    /// </summary>
    public class GemRenderer : MonoBehaviour
    {
        [SerializeField] private Sprite gemSprite;
        [SerializeField] private float cellSize = 1f;

        private readonly Dictionary<Vector2Int, GameObject> _instances = new Dictionary<Vector2Int, GameObject>();

        public void BuildGems(IEnumerable<Vector2Int> positions)
        {
            ClearGems();
            foreach (var pos in positions)
            {
                var go = new GameObject($"Gem_{pos.x}_{pos.y}");
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(pos.x * cellSize, pos.y * cellSize, -0.05f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = gemSprite;
                sr.sortingOrder = 2; // above tiles (order 0), keep Player above this too

                _instances[pos] = go;
            }
        }

        public void HideGem(Vector2Int pos)
        {
            if (_instances.TryGetValue(pos, out var go) && go != null)
                go.SetActive(false);
        }

        public void ClearGems()
        {
            foreach (var kvp in _instances)
                if (kvp.Value != null) Destroy(kvp.Value);
            _instances.Clear();
        }
    }
}