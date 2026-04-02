using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TapMatch.GameLogic;
using TapMatch.Runtime.Board;
using TapMatch.Runtime.Config;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using VContainer;

namespace TapMatch.Runtime.Board
{
    /// <summary>
    /// Manages the visual representation of the game board.
    /// Uses Unity's ObjectPool to recycle matchable GameObjects instead of
    /// constantly instantiating and destroying them — reduces GC pressure
    /// and improves performance, especially on mobile.
    /// </summary>
    public sealed class BoardView : MonoBehaviour, IBoardView
    {
        [SerializeField] private AssetReferenceGameObject matchablePrefabRef;

        private GameConfig _config;
        private GameObject _matchablePrefab;
        private ObjectPool<MatchableView> _pool;
        private readonly Dictionary<(int Row, int Col), MatchableView> _matchables = new();

        [Inject]
        public void Inject(GameConfig config)
        {
            _config = config;
        }

        public async Awaitable LoadAssetsAsync()
        {
            var handle = matchablePrefabRef.LoadAssetAsync<GameObject>();
            await handle.Task;
            _matchablePrefab = handle.Result;

            _pool = new ObjectPool<MatchableView>(
                createFunc: CreatePooledMatchable,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPooled,
                collectionCheck: false,
                defaultCapacity: _config.Rows * _config.Columns,
                maxSize: _config.Rows * _config.Columns * 2
            );
        }

        public void SpawnMatchable(int row, int col, int colorId)
        {
            Debug.Assert(_matchablePrefab != null,
                "BoardView: _matchablePrefab is null. Was LoadAssetsAsync() called?");

            var view = _pool.Get();
            var worldPos = GridToWorldPosition(row, col);
            view.transform.position = worldPos;
            view.Setup(row, col, colorId, _config.GetColor(colorId), _config.GetSprite(colorId));
            _matchables[(row, col)] = view;
        }

        public async Task AnimateRemovalAsync(TapResult result, CancellationToken ct)
        {
            var tasks = new List<Task>();

            foreach (var (row, col) in result.Removed)
            {
                if (_matchables.TryGetValue((row, col), out var view))
                {
                    _matchables.Remove((row, col));
                    tasks.Add(AnimateScaleDownAndRelease(view, ct));
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task AnimateFallsAsync(TapResult result, CancellationToken ct)
        {
            var tasks = new List<Task>();

            foreach (var fall in result.Falls)
            {
                if (_matchables.TryGetValue((fall.FromRow, fall.FromCol), out var view))
                {
                    _matchables.Remove((fall.FromRow, fall.FromCol));
                    view.UpdateGridPosition(fall.ToRow, fall.ToCol);
                    _matchables[(fall.ToRow, fall.ToCol)] = view;

                    var targetPos = GridToWorldPosition(fall.ToRow, fall.ToCol);
                    tasks.Add(AnimateMoveTo(view, targetPos, ct));
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task AnimateSpawnsAsync(TapResult result, CancellationToken ct)
        {
            var tasks = new List<Task>();

            foreach (var (row, col, colorId) in result.Spawned)
            {
                var spawnPos = GridToWorldPosition(-1, col);
                var targetPos = GridToWorldPosition(row, col);

                var view = _pool.Get();
                view.transform.position = spawnPos;
                view.Setup(row, col, colorId, _config.GetColor(colorId), _config.GetSprite(colorId));
                _matchables[(row, col)] = view;

                tasks.Add(AnimateMoveTo(view, targetPos, ct));
            }

            await Task.WhenAll(tasks);
        }

        public void ClearBoard()
        {
            foreach (var view in _matchables.Values)
            {
                if (view != null)
                    _pool.Release(view);
            }

            _matchables.Clear();
        }

        /// <summary>
        /// Converts grid coordinates to world position.
        /// Row 0 is top, Column 0 is left.
        /// The board is centered at the origin.
        /// </summary>
        public Vector3 GridToWorldPosition(int row, int col)
        {
            float boardWidth = _config.Columns * _config.CellSize;
            float boardHeight = _config.Rows * _config.CellSize;

            float x = col * _config.CellSize - boardWidth / 2f + _config.CellSize / 2f;
            float y = -row * _config.CellSize + boardHeight / 2f - _config.CellSize / 2f;

            return new Vector3(x, y, 0f);
        }

        private void OnDestroy()
        {
            _pool?.Dispose();
        }

        #region Object Pool Callbacks

        private MatchableView CreatePooledMatchable()
        {
            var go = Instantiate(_matchablePrefab, transform);
            return go.GetComponent<MatchableView>();
        }

        private void OnGetFromPool(MatchableView view)
        {
            view.ActivateFromPool();
        }

        private void OnReleaseToPool(MatchableView view)
        {
            view.ResetForPool();
        }

        private void OnDestroyPooled(MatchableView view)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        #endregion

        #region Animations

        private async Task AnimateScaleDownAndRelease(MatchableView view, CancellationToken ct)
        {
            float elapsed = 0f;
            float duration = _config.RemoveDuration;
            var t = view.transform;

            try
            {
                while (elapsed < duration)
                {
                    ct.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);
                    float scale = 1f - progress;
                    t.localScale = new Vector3(scale, scale, scale);
                    await Awaitable.NextFrameAsync(ct);
                }
            }
            finally
            {
                // Always return to pool, even if cancelled mid-animation
                if (view != null)
                    _pool.Release(view);
            }
        }

        private async Task AnimateMoveTo(MatchableView view, Vector3 target, CancellationToken ct)
        {
            float elapsed = 0f;
            float duration = _config.FallDuration;
            var t = view.transform;
            var start = t.position;

            try
            {
                while (elapsed < duration)
                {
                    ct.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);
                    // Ease-out quadratic for a natural fall feel
                    float eased = 1f - (1f - progress) * (1f - progress);
                    t.position = Vector3.Lerp(start, target, eased);
                    await Awaitable.NextFrameAsync(ct);
                }
            }
            finally
            {
                // Snap to exact position even if cancelled
                if (view != null)
                    t.position = target;
            }
        }

        #endregion
    }
}
