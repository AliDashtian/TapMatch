using NUnit.Framework;

namespace TapMatch.GameLogic.Tests
{
    [TestFixture]
    public sealed class GridCollapserTests
    {
        private GridCollapser _collapser;

        [SetUp]
        public void SetUp()
        {
            _collapser = new GridCollapser();
        }

        [Test]
        public void Collapse_NoEmptyCells_NoMoves()
        {
            // Grid (fully filled):
            // 0 1
            // 2 3
            var grid = new GridModel(2, 2);
            grid[0, 0] = 0; grid[0, 1] = 1;
            grid[1, 0] = 2; grid[1, 1] = 3;

            var moves = _collapser.Collapse(grid);

            Assert.AreEqual(0, moves.Count);
            Assert.AreEqual(0, grid[0, 0]);
            Assert.AreEqual(1, grid[0, 1]);
            Assert.AreEqual(2, grid[1, 0]);
            Assert.AreEqual(3, grid[1, 1]);
        }

        [Test]
        public void Collapse_EmptyAtBottom_FillsDown()
        {
            // Grid:
            // 1 _
            // _ 2
            // Row 0 is top, Row 1 is bottom
            var grid = new GridModel(2, 2);
            grid[0, 0] = 1; grid[0, 1] = GridModel.EmptyCell;
            grid[1, 0] = GridModel.EmptyCell; grid[1, 1] = 2;

            var moves = _collapser.Collapse(grid);

            // Column 0: '1' should fall from row 0 to row 1
            Assert.AreEqual(1, moves.Count);
            Assert.IsTrue(grid.IsEmpty(0, 0));
            Assert.AreEqual(1, grid[1, 0]);
            // Column 1: '2' already at bottom
            Assert.AreEqual(2, grid[1, 1]);
        }

        [Test]
        public void Collapse_GapInMiddle_ShiftsDown()
        {
            // Column with gap in middle:
            // 1
            // _
            // 2
            var grid = new GridModel(3, 1);
            grid[0, 0] = 1;
            grid[1, 0] = GridModel.EmptyCell;
            grid[2, 0] = 2;

            _collapser.Collapse(grid);

            // Expected:
            // _
            // 1
            // 2
            Assert.IsTrue(grid.IsEmpty(0, 0));
            Assert.AreEqual(1, grid[1, 0]);
            Assert.AreEqual(2, grid[2, 0]);
        }

        [Test]
        public void Collapse_MultipleGaps_ShiftsAllDown()
        {
            // Column:
            // 3
            // _
            // 2
            // _
            // 1
            var grid = new GridModel(5, 1);
            grid[0, 0] = 3;
            grid[1, 0] = GridModel.EmptyCell;
            grid[2, 0] = 2;
            grid[3, 0] = GridModel.EmptyCell;
            grid[4, 0] = 1;

            _collapser.Collapse(grid);

            // Expected:
            // _
            // _
            // 3
            // 2
            // 1
            Assert.IsTrue(grid.IsEmpty(0, 0));
            Assert.IsTrue(grid.IsEmpty(1, 0));
            Assert.AreEqual(3, grid[2, 0]);
            Assert.AreEqual(2, grid[3, 0]);
            Assert.AreEqual(1, grid[4, 0]);
        }

        [Test]
        public void Collapse_EntireColumnEmpty_NoMoves()
        {
            var grid = new GridModel(3, 1);
            // All empty

            var moves = _collapser.Collapse(grid);

            Assert.AreEqual(0, moves.Count);
        }

        [Test]
        public void Collapse_ReturnsMoves_WithCorrectFromTo()
        {
            // Column:
            // 5
            // _
            // _
            var grid = new GridModel(3, 1);
            grid[0, 0] = 5;
            grid[1, 0] = GridModel.EmptyCell;
            grid[2, 0] = GridModel.EmptyCell;

            var moves = _collapser.Collapse(grid);

            Assert.AreEqual(1, moves.Count);
            Assert.AreEqual(0, moves[0].FromRow);
            Assert.AreEqual(0, moves[0].FromCol);
            Assert.AreEqual(2, moves[0].ToRow);
            Assert.AreEqual(0, moves[0].ToCol);
        }

        [Test]
        public void Collapse_MultipleColumns_IndependentGravity()
        {
            // Grid:
            // 1 _
            // _ 2
            // _ 3
            var grid = new GridModel(3, 2);
            grid[0, 0] = 1; grid[0, 1] = GridModel.EmptyCell;
            grid[1, 0] = GridModel.EmptyCell; grid[1, 1] = 2;
            grid[2, 0] = GridModel.EmptyCell; grid[2, 1] = 3;

            _collapser.Collapse(grid);

            // Column 0: 1 falls to bottom
            Assert.IsTrue(grid.IsEmpty(0, 0));
            Assert.IsTrue(grid.IsEmpty(1, 0));
            Assert.AreEqual(1, grid[2, 0]);

            // Column 1: 2 and 3 already at bottom, just shift down
            Assert.IsTrue(grid.IsEmpty(0, 1));
            Assert.AreEqual(2, grid[1, 1]);
            Assert.AreEqual(3, grid[2, 1]);
        }
    }
}
