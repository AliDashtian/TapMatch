using UnityEngine;

namespace TapMatch.Runtime.Board
{
    /// <summary>
    /// Visual representation of a single matchable tile on the board.
    /// Responsible only for its own appearance — no game logic.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MatchableView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private int _colorId;

        public int ColorId => _colorId;
        public int Row { get; private set; }
        public int Col { get; private set; }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
    }
}
