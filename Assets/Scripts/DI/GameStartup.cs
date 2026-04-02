using TapMatch.Runtime.Board;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TapMatch.Runtime.DI
{
    /// <summary>
    /// Entry point that VContainer calls after all dependencies are resolved.
    /// Handles async initialization: load Addressable assets, then start the game.
    ///
    /// Note: [Inject] method injection on MonoBehaviours (BoardView, InputHandler,
    /// BoardPresenter) is handled automatically by VContainer's RegisterComponent.
    /// This class only orchestrates the startup sequence.
    /// </summary>
    public sealed class GameStartup : IStartable
    {
        private readonly BoardPresenter _presenter;
        private readonly BoardView _boardView;

        [Inject]
        public GameStartup(BoardPresenter presenter, BoardView boardView)
        {
            _presenter = presenter;
            _boardView = boardView;
        }

        public async void Start()
        {
            // 1. Load Addressable assets (matchable prefab)
            await _boardView.LoadAssetsAsync();

            // 2. Initialize the game board (fills grid + spawns matchables)
            await _presenter.InitializeAsync();
        }
    }
}
