
using System.Collections.Generic;

namespace TapMatch.GameLogic
{
    /// <summary>
    /// Finds all connected matchables of the same color using BFS flood-fill.
    /// "Connected" means adjacent horizontally or vertically (not diagonally).
    /// </summary>
    public sealed class MatchFinder
    {
        private static readonly (int directionRow, int directionColumn)[] Directions =
{
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        /// <summary>
        /// Returns all cells connected to (startRow, startCol) that share the same color.
        /// Returns an empty list if the starting cell is empty.
        /// </summary>
        public List<(int Row, int Col)> FindConnected(GridModel grid, int startRow, int startCol)
        {
            var result = new List<(int Row, int Col)>();

            if (!grid.IsInBounds(startRow, startCol) || grid.IsEmpty(startRow, startCol))
                return result;

            int targetColor = grid[startRow, startCol];
            var visited = new bool[grid.Rows, grid.Columns];
            var queue = new Queue<(int Row, int Col)>();

            queue.Enqueue((startRow, startCol));
            visited[startRow, startCol] = true;

            while (queue.Count > 0)
            {
                var (row, col) = queue.Dequeue();
                result.Add((row, col));

                foreach (var (directionRow, directionCol) in Directions)
                {
                    int newRow = row + directionRow;
                    int newCol = col + directionCol;

                    if (!grid.IsInBounds(newRow, newCol)) continue;
                    if (visited[newRow, newCol]) continue;
                    if (grid[newRow, newCol] != targetColor) continue;

                    visited[newRow, newCol] = true;
                    queue.Enqueue((newRow, newCol));
                }
            }

            return result;
        }
    }
}
