using UnityEngine;
using GridPuzzle.Gameplay;

namespace GridPuzzle.UI
{
    /// <summary>
    /// Deadline-friendly HUD using Unity's immediate-mode OnGUI — no Canvas,
    /// no TextMeshPro import, no prefab wiring required. Shows moves
    /// remaining, an Undo button, a Restart button, and the win/lose
    /// message. Swap for a proper Canvas + TextMeshPro HUD later if you
    /// have time after submitting.
    /// </summary>
    public class SimpleHUD : MonoBehaviour
    {
        [SerializeField] private GameController gameController;

        private GUIStyle _labelStyle;
        private GUIStyle _bigMessageStyle;

        private void EnsureStyles()
        {
            if (_labelStyle != null) return;

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, normal = { textColor = Color.white } };
            _bigMessageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 40,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        private void OnGUI()
        {
            if (gameController == null || gameController.Model == null) return;
            EnsureStyles();

            GUI.Label(new Rect(20, 20, 400, 50), $"Moves left: {gameController.Model.MovesRemaining}", _labelStyle);

            if (GUI.Button(new Rect(20, 80, 120, 60), "Undo"))
                gameController.OnUndoPressed();

            if (gameController.IsGameOver)
            {
                GUI.Box(new Rect(Screen.width / 2f - 250, Screen.height / 2f - 80, 500, 160), "");
                GUI.Label(new Rect(Screen.width / 2f - 250, Screen.height / 2f - 60, 500, 80),
                    gameController.StatusMessage, _bigMessageStyle);

                if (GUI.Button(new Rect(Screen.width / 2f - 60, Screen.height / 2f + 30, 120, 50), "Restart"))
                    gameController.RestartGame();
            }
        }
    }
}