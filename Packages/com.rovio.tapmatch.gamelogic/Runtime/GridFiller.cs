
using System;
using System.Collections.Generic;

namespace TapMatch.GameLogic
{
    /// <summary>
    /// Fills empty cells in the grid with random colors.
    /// Empty cells are always at the top of each column after a collapse.
    /// </summary>
    public sealed class GridFiller
    {
        private readonly IRandomProvider _random;

        public GridFiller(IRandomProvider random)
        {
            _random = random;
        }

        /// <summary>
        /// fills all empty cells at the top of each column (post-collapse, empties are always at top) with random color IDs in [0, colorCount).
        /// Returns what was filled so the view can spawn new matchables.
        /// </summary>
        public List<(int Row, int Col, int ColorId)> Fill(GridModel grid, int colorCount)
        {
            var filled = new List<(int Row, int Col, int ColorId)>();

            for (int col = 0; col < grid.Columns; col++)
            {
                for (int row = 0; row < grid.Rows; row++)
                {
                    // skip this column if reached the first non-empty
                    if (!grid.IsEmpty(row, col)) break;

                    int colorId = _random.Next(colorCount);
                    grid[row, col] = colorId;
                    filled.Add((row, col, colorId));
                }
            }

            return filled;
        }

        /// <summary>
        /// Fills the entire grid with random colors. Used for initial board setup.
        /// </summary>
        public void FillEntireGrid(GridModel grid, int colorCount)
        {
            for (int r = 0; r < grid.Rows; r++)
                for (int c = 0; c < grid.Columns; c++)
                    grid[r, c] = _random.Next(colorCount);
        }
    }
}
