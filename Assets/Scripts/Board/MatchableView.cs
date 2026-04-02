using UnityEngine;

namespace TapMatch.Runtime.Board
{
    /// <summary>
    /// Visual representation of a single matchable tile on the board.
    /// Responsible only for its own appearance — no game logic.
    /// Supports object pooling via ResetForPool().
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MatchableView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Vector3 _originalScale;
        private int _colorId;

        public int ColorId => _colorId;
        public int Row { get; private set; }
        public int Col { get; private set; }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalScale = transform.localScale;
        }

        /// <summary>
        /// Initializes the matchable with its grid position and color.
        /// </summary>
        public void Setup(int row, int col, int colorId, Color color)
        {
            Row = row;
            Col = col;
            _colorId = colorId;
            _spriteRenderer.color = color;
        }

        /// <summary>
        /// Updates the grid position this matchable represents (after a fall).
        /// Does NOT move the transform — that's handled by the BoardView.
        /// </summary>
        public void UpdateGridPosition(int newRow, int newCol)
        {
            Row = newRow;
            Col = newCol;
        }

        /// <summary>
        /// Resets the matchable to a clean state for object pool reuse.
        /// Called when the object is returned to the pool.
        /// </summary>
        public void ResetForPool()
        {
            transform.localScale = _originalScale;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Activates the matchable when taken from the pool.
        /// </summary>
        public void ActivateFromPool()
        {
            gameObject.SetActive(true);
        }
    }
}
