using System;
using UnityEngine;
using GridPuzzle.Core;

namespace GridPuzzle.Input
{
    
    public class SwipeInputController : MonoBehaviour
    {
        [SerializeField] private float swipeThreshold = 50f;

        public event Action<Direction> OnSwipe;

        private Vector2 _touchStart;
        private bool _tracking;

        private void Update()
        {
            HandleTouch();
            HandleKeyboard();
        }

        private void HandleTouch()
        {
            if (UnityEngine.Input.touchCount == 0) return;

            Touch touch = UnityEngine.Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _touchStart = touch.position;
                    _tracking = true;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (!_tracking) return;
                    _tracking = false;
                    Vector2 delta = touch.position - _touchStart;
                    ClassifyAndFire(delta);
                    break;
            }
        }

        private void HandleKeyboard()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W))
                OnSwipe?.Invoke(Direction.Up);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S))
                OnSwipe?.Invoke(Direction.Down);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
                OnSwipe?.Invoke(Direction.Left);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D))
                OnSwipe?.Invoke(Direction.Right);
        }

        private void ClassifyAndFire(Vector2 delta)
        {
            if (delta.magnitude < swipeThreshold) return;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                OnSwipe?.Invoke(delta.x > 0 ? Direction.Right : Direction.Left);
            else
                OnSwipe?.Invoke(delta.y > 0 ? Direction.Up : Direction.Down);
        }
    }
}