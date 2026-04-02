using System;
using TapMatch.Runtime.Config;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace TapMatch.Runtime.Input
{
    /// <summary>
    /// Handles player tap/click input and translates screen position
    /// to grid coordinates using direct math on the board layout.
    /// Follows the Single Responsibility Principle: only deals with input detection.
    /// </summary>
    public sealed class InputHandler : MonoBehaviour
    {
        /// <summary>
        /// Fired when the player taps a valid grid cell.
        /// Parameters: (row, col) in grid coordinates.
        /// </summary>
        public event Action<int, int> OnTileTapped;

        private GameConfig _config;
        private Camera _mainCamera;
        private bool _inputEnabled = true;

        [Inject]
        public void Inject(GameConfig config)
        {
            _config = config;
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
        }

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_inputEnabled) return;

            bool tapped = false;
            Vector2 screenPos = Vector2.zero;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapped = true;
                screenPos = Mouse.current.position.ReadValue();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapped = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            if (!tapped) return;

            var worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            var gridPos = WorldToGridPosition(worldPos);

            if (gridPos.HasValue)
            {
                OnTileTapped?.Invoke(gridPos.Value.row, gridPos.Value.col);
            }
        }

        /// <summary>
        /// Direct math inverse of BoardView.GridToWorldPosition.
        /// Returns null if the world position is outside the grid bounds.
        /// </summary>
        private (int row, int col)? WorldToGridPosition(Vector3 worldPos)
        {
            float boardWidth = _config.Columns * _config.CellSize;
            float boardHeight = _config.Rows * _config.CellSize;

            // Inverse of: x = col * cellSize - boardWidth/2 + cellSize/2
            float colF = (worldPos.x + boardWidth / 2f - _config.CellSize / 2f) / _config.CellSize;
            // Inverse of: y = -row * cellSize + boardHeight/2 - cellSize/2
            float rowF = -(worldPos.y - boardHeight / 2f + _config.CellSize / 2f) / _config.CellSize;

            int col = Mathf.RoundToInt(colF);
            int row = Mathf.RoundToInt(rowF);

            if (row < 0 || row >= _config.Rows || col < 0 || col >= _config.Columns)
                return null;

            // Check that tap is close enough to the cell center (within half a cell)
            float distX = Mathf.Abs(colF - col);
            float distY = Mathf.Abs(rowF - row);

            if (distX > 0.5f || distY > 0.5f)
                return null;

            return (row, col);
        }
    }
}
