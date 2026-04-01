
using System;

namespace TapMatch.GameLogic
{
    /// <summary>
    /// Represents the NxM grid of matchable color IDs.
    /// Color ID 0..P-1 where P is the number of distinct colors.
    /// A value of -1 represents an empty cell.
    /// </summary>
    public sealed class GridModel
    {
        public const int EmptyCell = -1;

        public int Rows { get; }
        public int Columns { get; }

        private readonly int[,] _cells;

        public GridModel(int rows, int columns)
        {
            if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
            if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));

            Rows = rows; 
            Columns = columns;
            _cells = new int[rows, columns];

            Clear();
        }

        public int this[int row, int column]
        {
            get => _cells[row, column];
            set => _cells[row, column] = value;
        }

        public bool IsInBounds(int row, int column) =>
            row >= 0 && row < Rows && column >= 0 && column < Columns;

        public bool IsEmpty(int row, int column) =>
            _cells[row, column] == EmptyCell;

        public void Clear()
        {
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Columns; c++)
                    _cells[r, c] = EmptyCell;
        }
    }
}
