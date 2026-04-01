
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Android.Gradle;
using static Codice.Client.Common.EventTracking.TrackFeatureUseEvent.Features.DesktopGUI.Filters;

namespace TapMatch.GameLogic
{
    /// <summary>
    /// Applies gravity to the grid: non-empty cells fall down to fill gaps.
    /// Row 0 is the top of the grid, Row (Rows-1) is the bottom.
    /// </summary>
    public class GridCollapser
    {
        /// <summary>
        /// Collapses all columns so that empty cells bubble to the top.
        /// Returns a list of moves describing what moved and where,
        /// useful for the view layer to animate falls.
        /// </summary>
        public List<FallMove> Collapse(GridModel grid)
        {
            var moves = new List<FallMove>();

            for (int col = 0; col < grid.Columns; col++)
            {
                CollapseColumn(grid, col, moves);
            }

            return moves;
        }

        /// <summary>
        /// Processes a column, from bottom to top
        /// Use a "write pointer" approach: scan from bottom row upward, compact non-empty cells downward, empty cells bubble to top
        /// Fills a List<FallMove> describing each movement (fromRow, fromCol, toRow, toCol)
        /// </summary>
        private void CollapseColumn(GridModel grid, int col, List<FallMove> moves)
        {
            int writeRow = grid.Rows - 1;

            for (int readRow = grid.Rows - 1; readRow >= 0; readRow--)
            {
                if (grid.IsEmpty(readRow, col)) continue;

                if (readRow != writeRow)
                {
                    moves.Add(new FallMove(readRow, col, writeRow, col));
                    grid[writeRow, col] = grid[readRow, col];
                    grid[readRow, col] = GridModel.EmptyCell;
                }

                writeRow--;
            }
        }
    }


    /// <summary>
    /// Describes a matchable moving from one cell to another during gravity collapse.
    /// </summary>
    public readonly struct FallMove
    {
        public int FromRow { get; }
        public int FromCol { get; }
        public int ToRow { get; }
        public int ToCol { get; }

        public FallMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            FromRow = fromRow;
            FromCol = fromCol;
            ToRow = toRow;
            ToCol = toCol;
        }
    }
}
