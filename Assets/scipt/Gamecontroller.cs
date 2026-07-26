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

        [Header("Scene references")]
        [SerializeField] private SwipeInputController inputController;
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private GridRenderer gridRenderer;

        private GridModel _model;
        private HistoryManager _history;
        private readonly Queue<Direction> _inputQueue = new();
        private const int MaxQueuedInputs = 3;

        public GridModel Model => _model;
        public string StatusMessage { get; private set; } = "";
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            var exitPosition = new Vector2Int(columns - 1, rows - 1);
            _model = new GridModel(columns, rows, startPosition, exitPosition, moveLimit);
            _history = new HistoryManager();
        }

        private void Start()
        {
            gridRenderer.BuildGrid(_model);
            playerMovement.SnapTo(_model.PlayerPosition);
            inputController.OnSwipe += HandleSwipeInput;
        }

        private void OnDestroy()
        {
            if (inputController != null) inputController.OnSwipe -= HandleSwipeInput;
        }

        private void Update()
        {
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

            playerMovement.MoveTo(result.ToPosition, moveDuration, () =>
            {
                if (result.HitLava) HandleGameOver("Game Over — stepped on lava");
                else if (result.ReachedExit) HandleWin();
                else if (result.OutOfMoves) HandleOutOfMoves();
            });
        }

        public void OnUndoPressed()
        {
            if (IsGameOver || playerMovement.IsMoving) return;
            if (!_history.TryUndo(out var delta)) return;

            _model.RestoreState(delta.PreviousPosition, delta.PreviousMovesRemaining);
            playerMovement.SnapTo(_model.PlayerPosition);
            _inputQueue.Clear();
        }

        private void HandleWin()
        {
            StatusMessage = "You reached the exit! You win.";
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
            var exitPosition = new Vector2Int(columns - 1, rows - 1);
            _model = new GridModel(columns, rows, startPosition, exitPosition, moveLimit);
            _history.Clear();
            _inputQueue.Clear();
            IsGameOver = false;
            StatusMessage = "";
            gridRenderer.BuildGrid(_model);
            playerMovement.SnapTo(_model.PlayerPosition);
        }
    }
}