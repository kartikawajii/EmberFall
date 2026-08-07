using System.Collections.Generic;
using UnityEngine;
using GridPuzzle.Core;
using GridPuzzle.History;
using GridPuzzle.Rendering;
using GridPuzzle.Input;

namespace GridPuzzle.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [Header("Grid setup")]
        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 4;
        [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);
        [SerializeField] private int moveLimit = 20;
        [SerializeField] private float moveDuration = 0.15f;

        [Header("Gems (grid positions, excluding start/exit)")]
        [SerializeField]
        private List<Vector2Int> gemPositions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 2),
            new Vector2Int(2, 1),
            new Vector2Int(3, 2),
        };

        [Header("Scene references")]
        [SerializeField] private SwipeInputController inputController;
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private GridRenderer gridRenderer;
        [SerializeField] private GemRenderer gemRenderer;

        private GridModel _model;
        private HistoryManager _history;
        private CrumbleSystem _crumble;
        private GemSystem _gems;

        private readonly Queue<Direction> _inputQueue = new Queue<Direction>();
        private const int MaxQueuedInputs = 3;

        public GridModel Model => _model;
        public string StatusMessage { get; private set; } = "";
        public bool IsGameOver { get; private set; }
        public int GemsCollected => _gems != null ? _gems.CollectedCount : 0;
        public int GemsTotal => _gems != null ? _gems.TotalCount : 0;

        private void Awake()
        {
            BuildModelsForNewGame();
        }

        private void BuildModelsForNewGame()
        {
            var exitPosition = new Vector2Int(columns - 1, rows - 1);
            _model = new GridModel(columns, rows, startPosition, exitPosition, moveLimit);
            _history = new HistoryManager();
            _crumble = new CrumbleSystem();
            _gems = new GemSystem(gemPositions);
        }

        private void Start()
        {
            gridRenderer.BuildGrid(_model);
            gemRenderer.BuildGems(gemPositions);
            playerMovement.SnapTo(_model.PlayerPosition);
            inputController.OnSwipe += HandleSwipeInput;
        }

        private void OnDestroy()
        {
            if (inputController != null) inputController.OnSwipe -= HandleSwipeInput;
        }

        private void Update()
        {
            // Crumble ticks every frame regardless of game-over state's
            // effect on input, so tiles keep decaying visually right up
            // until the moment the game actually ends.
            if (!IsGameOver)
                _crumble.Tick(Time.time, _model, (pos, state) => gridRenderer.SetTileVisual(pos, state));

            if (IsGameOver) return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Z)) OnUndoPressed();

            if (_inputQueue.Count > 0 && !playerMovement.IsMoving)
                ExecuteMove(_inputQueue.Dequeue());
        }

        private void HandleSwipeInput(Direction dir)
        {
            if (IsGameOver) return;
            if (_inputQueue.Count >= MaxQueuedInputs) return;
            _inputQueue.Enqueue(dir);
        }

        private void ExecuteMove(Direction dir)
        {
            var positionBefore = _model.PlayerPosition;
            var movesBefore = _model.MovesRemaining;

            var result = _model.TryMove(dir);

            if (!result.Success)
            {
                if (result.OutOfMoves) HandleOutOfMoves();
                return;
            }

            _history.Record(positionBefore, movesBefore);

            // Start the tile we're leaving crumbling; stabilize the one we're
            // stepping onto (removes any in-progress crack timer on it).
            _crumble.MarkLeft(positionBefore, Time.time);
            _crumble.MarkStabilized(result.ToPosition);

            if (_gems.TryCollect(result.ToPosition))
                gemRenderer.HideGem(result.ToPosition);

            playerMovement.MoveTo(result.ToPosition, moveDuration, () =>
            {
                if (result.HitLava)
                {
                    HandleGameOver("Game Over — stepped on lava");
                }
                else if (result.ReachedExit)
                {
                    if (_gems.AllCollected) HandleWin();
                    else StatusMessage = $"Collect all gems first! ({_gems.CollectedCount}/{_gems.TotalCount})";
                }
                else if (result.OutOfMoves)
                {
                    HandleOutOfMoves();
                }
            });
        }

        public void OnUndoPressed()
        {
            if (IsGameOver || playerMovement.IsMoving) return;
            if (!_history.TryUndo(out var delta)) return;

            _model.RestoreState(delta.PreviousPosition, delta.PreviousMovesRemaining);
            playerMovement.SnapTo(_model.PlayerPosition);
            _inputQueue.Clear();
            // Note: crumble timers are not rewound on undo in this version —
            // a known, documented simplification given the submission deadline.
        }

        private void HandleWin()
        {
            StatusMessage = "You collected every gem and reached the exit. You win!";
            IsGameOver = true;
        }

        private void HandleGameOver(string reason)
        {
            StatusMessage = reason;
            IsGameOver = true;
        }

        private void HandleOutOfMoves()
        {
            StatusMessage = "Game Over — out of moves";
            IsGameOver = true;
        }

        public void RestartGame()
        {
            BuildModelsForNewGame();
            IsGameOver = false;
            StatusMessage = "";
            _inputQueue.Clear();

            gridRenderer.BuildGrid(_model);
            gemRenderer.BuildGems(gemPositions);
            playerMovement.SnapTo(_model.PlayerPosition);
        }
    }
}