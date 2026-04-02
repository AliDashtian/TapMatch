using System;
using System.Threading;
using TapMatch.GameLogic;
using TapMatch.Runtime.Board;
using TapMatch.Runtime.Config;
using TapMatch.Runtime.Input;
using UnityEngine;
using VContainer;

namespace TapMatch.Runtime.Board
{
    /// <summary>
    /// Orchestrates the game flow following the MVP (Model-View-Presenter) pattern.
    /// Bridges the pure GameController logic with the Unity BoardView visuals.
    ///
    /// Responsibilities:
    /// - Initializes the game (fills grid, spawns initial matchables)
    /// - Handles tap events from InputHandler
    /// - Delegates to GameController for logic
    /// - Drives BoardView animations in sequence: remove → fall → spawn
    /// - Manages input blocking during animations
    /// </summary>
    public sealed class BoardPresenter : MonoBehaviour
    {
        private GameController _gameController;
        private IBoardView _boardView;
        private InputHandler _inputHandler;
        private GameConfig _config;
        private CancellationTokenSource _cts;

        [Inject]
        public void Inject(
            GameController gameController,
            IBoardView boardView,
            InputHandler inputHandler,
            GameConfig config)
        {
            _gameController = gameController;
            _boardView = boardView;
            _inputHandler = inputHandler;
            _config = config;
        }

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_inputHandler != null)
                _inputHandler.OnTileTapped -= HandleTileTapped;
        }

        /// <summary>
        /// Initializes the game board. Called after DI injection.
        /// </summary>
        public async Awaitable InitializeAsync()
        {
            _gameController.Initialize();

            // Spawn initial matchables
            for (int r = 0; r < _config.Rows; r++)
                for (int c = 0; c < _config.Columns; c++)
                {
                    int colorId = _gameController.Grid[r, c];
                    _boardView.SpawnMatchable(r, c, colorId);
                }

            _inputHandler.OnTileTapped += HandleTileTapped;
        }

        private async void HandleTileTapped(int row, int col)
        {
            try
            {
                var result = _gameController.TryTap(row, col);
                if (result == null) return;

                _inputHandler.SetInputEnabled(false);

                var ct = _cts.Token;

                // 1. Remove matched tiles
                await _boardView.AnimateRemovalAsync(result, ct);

                // 2. Animate existing tiles falling down
                await _boardView.AnimateFallsAsync(result, ct);

                // 3. Spawn new tiles and animate them falling in
                await _boardView.AnimateSpawnsAsync(result, ct);

                _inputHandler.SetInputEnabled(true);
            }
            catch (OperationCanceledException)
            {
                // Expected when the scene is unloaded during animation — safe to ignore
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
