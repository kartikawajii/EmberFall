using UnityEngine;
using GridPuzzle.Core;
using GridPuzzle.History;

namespace GridPuzzle.Bootstrap
{
    public class GridBootstrap : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 4;
        [SerializeField] private float cellSize = 1.2f;
        [SerializeField] private int moveLimit = 20;

        private GridModel _model;
        private HistoryManager _history;
        private SpriteRenderer[,] _tileRenderers;
        private Transform _playerMarker;
        private static Sprite _squareSprite;

        private void Start()
        {
            _model = new GridModel(
                columns, rows,
                startPosition: new Vector2Int(0, 0),
                exitPosition: new Vector2Int(columns - 1, rows - 1),
                moveLimit: moveLimit);

            _history = new HistoryManager();

            BuildSquareSpriteOnce();
            SpawnTiles();
            SpawnPlayerMarker();
            CenterCamera();
        }

        private void Update()
        {
            // Zero-setup input just for testing — replace with
            // SwipeInputController once that's wired up.
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) TryMove(Direction.Up);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) TryMove(Direction.Down);
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) TryMove(Direction.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) TryMove(Direction.Right);
            if (Input.GetKeyDown(KeyCode.Z)) Undo();
        }

        private void TryMove(Direction dir)
        {
            var before = (_model.PlayerPosition, _model.MovesRemaining);
            var result = _model.TryMove(dir);

            if (!result.Success)
            {
                Debug.Log(result.OutOfMoves ? "Out of moves." : "Blocked — edge of grid.");
                return;
            }

            _history.Record(before.Item1, before.Item2);
            _playerMarker.position = GridToWorld(result.ToPosition);

            if (result.HitLava) Debug.Log("Game over: stepped on lava.");
            else if (result.ReachedExit) Debug.Log("Reached the exit!");
            else if (result.OutOfMoves) Debug.Log("Game over: out of moves.");
        }

        private void Undo()
        {
            if (!_history.TryUndo(out var delta)) return;
            _model.RestoreState(delta.PreviousPosition, delta.PreviousMovesRemaining);
            _playerMarker.position = GridToWorld(_model.PlayerPosition);
        }

        private void BuildSquareSpriteOnce()
        {
            if (_squareSprite != null) return;

            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            _squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }

        private void SpawnTiles()
        {
            _tileRenderers = new SpriteRenderer[_model.Columns, _model.Rows];

            for (int x = 0; x < _model.Columns; x++)
            {
                for (int y = 0; y < _model.Rows; y++)
                {
                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.SetParent(transform);
                    go.transform.position = new Vector3(x * cellSize, y * cellSize, 0f);
                    go.transform.localScale = Vector3.one * (cellSize * 0.9f);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = _squareSprite;
                    _tileRenderers[x, y] = sr;

                    bool isExit = new Vector2Int(x, y) == _model.ExitPosition;
                    sr.color = isExit
                        ? new Color(0.70f, 0.53f, 1f)      // exit — violet
                        : new Color(0.23f, 0.17f, 0.14f);  // solid tile — stone brown
                }
            }
        }

        private void SpawnPlayerMarker()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player.name = "PlayerMarker";
            player.transform.SetParent(transform);
            player.transform.localScale = Vector3.one * (cellSize * 0.5f);
            player.transform.position = GridToWorld(_model.PlayerPosition) + new Vector3(0, 0, -0.1f);
            player.GetComponent<Renderer>().material.color = new Color(0.21f, 0.88f, 0.79f); // teal
            Destroy(player.GetComponent<Collider>()); // no physics needed for this test
            _playerMarker = player.transform;
        }

        private Vector3 GridToWorld(Vector2Int pos) => new Vector3(pos.x * cellSize, pos.y * cellSize, 0f);

        private void CenterCamera()
        {
            var cam = Camera.main;
            if (cam == null || !cam.orthographic) return;

            float midX = (_model.Columns - 1) * cellSize * 0.5f;
            float midY = (_model.Rows - 1) * cellSize * 0.5f;
            cam.transform.position = new Vector3(midX, midY, -10f);
            cam.orthographicSize = Mathf.Max(_model.Columns, _model.Rows) * cellSize * 0.75f;
        }
    }
}