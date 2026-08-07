using System;
using System.Collections;
using UnityEngine;

namespace GridPuzzle.Gameplay
{
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public bool IsMoving { get; private set; }
        private Coroutine _activeMove;

        public void SnapTo(Vector2Int gridPos)
        {
            if (_activeMove != null) { StopCoroutine(_activeMove); _activeMove = null; }
            IsMoving = false;
            transform.position = GridCoordinateUtil.GridToWorld(gridPos, cellSize);
        }

        public void MoveTo(Vector2Int targetGridPos, float duration, Action onComplete = null)
        {
            if (_activeMove != null) StopCoroutine(_activeMove);
            _activeMove = StartCoroutine(MoveRoutine(targetGridPos, duration, onComplete));
        }

        private IEnumerator MoveRoutine(Vector2Int targetGridPos, float duration, Action onComplete)
        {
            IsMoving = true;
            Vector3 start = transform.position;
            Vector3 end = GridCoordinateUtil.GridToWorld(targetGridPos, cellSize);
            float elapsed = 0f;

            if (duration <= 0f)
            {
                transform.position = end;
            }
            else
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = easeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                    transform.position = Vector3.LerpUnclamped(start, end, t);
                    yield return null;
                }
                transform.position = end;
            }

            IsMoving = false;
            _activeMove = null;
            onComplete?.Invoke();
        }
    }
}