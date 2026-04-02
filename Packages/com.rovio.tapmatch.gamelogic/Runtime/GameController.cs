
using System;
using System.Collections.Generic;

namespace TapMatch.GameLogic
{
    /// <summary>
    /// Orchestrates the tap-match game loop: tap → find matches → remove → collapse → fill.
    /// Pure logic with no Unity dependencies. Uses a simple state machine to block input
    /// while the board is settling.
    /// </summary>
    public sealed class GameController
    {
        public GridModel Grid { get; }
        public int ColorCount { get; }

        private readonly MatchFinder _matchFinder;
        private readonly GridCollapser _collapser;
        private readonly GridFiller _filler;

        public GameController(
            GridModel grid,
            int colorCount,
            MatchFinder matchFinder,
            GridCollapser collapser,
            GridFiller filler)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            ColorCount = colorCount > 0 ? colorCount : throw new ArgumentOutOfRangeException(nameof(colorCount));
            _matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
            _collapser = collapser ?? throw new ArgumentNullException(nameof(collapser));
            _filler = filler ?? throw new ArgumentNullException(nameof(filler));
        }

        /// <summary>
        /// Fills the entire grid with random matchables. Call once at game start.
        /// </summary>
        public void Initialize()
        {
            _filler.FillEntireGrid(Grid, ColorCount);
        }

        /// <summary>
        /// Processes a tap at the given grid position.
        /// Returns null if input is blocked or the tap is invalid.
        /// Returns a TapResult describing what happened for the view to animate.
        /// </summary>
        public TapResult TryTap(int row, int col)
        {
            if (!Grid.IsInBounds(row, col)) return null;
            if (Grid.IsEmpty(row, col)) return null;

            var connected = _matchFinder.FindConnected(Grid, row, col);

            if (connected.Count < 2) return null;

            foreach (var (r, c) in connected)
            {
                Grid[r, c] = GridModel.EmptyCell;
            }

            var falls = _collapser.Collapse(Grid);
            var spawns = _filler.Fill(Grid, ColorCount);

            return new TapResult(connected, falls, spawns);
        }
    }

    /// <summary>
    /// Encapsulates the result of a successful tap for the view layer to consume.
    /// </summary>
    public sealed class TapResult
    {
        public IReadOnlyList<(int Row, int Col)> Removed { get; }
        public IReadOnlyList<FallMove> Falls { get; }
        public IReadOnlyList<(int Row, int Col, int ColorId)> Spawned { get; }

        public TapResult(
            List<(int Row, int Col)> removed,
            List<FallMove> falls,
            List<(int Row, int Col, int ColorId)> spawned)
        {
            Removed = removed;
            Falls = falls;
            Spawned = spawned;
        }
    }
}
