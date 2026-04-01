

using NUnit.Framework;

namespace TapMatch.GameLogic.Tests
{
    [TestFixture]
    public sealed class GridFillerTests
    {
        [Test]
        public void FillEntireGrid_FillsAllCells()
        {
            var filler = new GridFiller(new FakeRandomProvider(0, 1, 2));
            var grid = new GridModel(3, 3);

            filler.FillEntireGrid(grid, 3);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.IsFalse(grid.IsEmpty(r, c), $"Cell [{r},{c}] should not be empty");
        }

        [Test]
        public void FillEntireGrid_UsesProvidedColors()
        {
            // FakeRandom returns 0,1,2,0,1,2,... and we have 3 colors
            var filler = new GridFiller(new FakeRandomProvider(0, 1, 2));
            var grid = new GridModel(2, 3);

            filler.FillEntireGrid(grid, 3);

            // Row 0: 0,1,2  Row 1: 0,1,2
            Assert.AreEqual(0, grid[0, 0]);
            Assert.AreEqual(1, grid[0, 1]);
            Assert.AreEqual(2, grid[0, 2]);
            Assert.AreEqual(0, grid[1, 0]);
            Assert.AreEqual(1, grid[1, 1]);
            Assert.AreEqual(2, grid[1, 2]);
        }

        [Test]
        public void Fill_OnlyFillsEmptyCells()
        {
            var filler = new GridFiller(new FakeRandomProvider(3));
            var grid = new GridModel(3, 1);
            grid[0, 0] = GridModel.EmptyCell; // empty (top)
            grid[1, 0] = 1;                    // filled
            grid[2, 0] = 2;                    // filled

            var filled = filler.Fill(grid, 5);

            Assert.AreEqual(1, filled.Count);
            Assert.AreEqual(0, filled[0].Row);
            Assert.AreEqual(0, filled[0].Col);
            Assert.AreEqual(3, filled[0].ColorId);
            // Existing cells unchanged
            Assert.AreEqual(1, grid[1, 0]);
            Assert.AreEqual(2, grid[2, 0]);
        }

        [Test]
        public void Fill_EmptiesAtTopAfterCollapse_FillsCorrectly()
        {
            // Simulates post-collapse state: empties at top of column
            var filler = new GridFiller(new FakeRandomProvider(0, 1));
            var grid = new GridModel(4, 1);
            grid[0, 0] = GridModel.EmptyCell;
            grid[1, 0] = GridModel.EmptyCell;
            grid[2, 0] = 3;
            grid[3, 0] = 4;

            var filled = filler.Fill(grid, 5);

            Assert.AreEqual(2, filled.Count);
            Assert.AreEqual(0, grid[0, 0]);
            Assert.AreEqual(1, grid[1, 0]);
            Assert.AreEqual(3, grid[2, 0]);
            Assert.AreEqual(4, grid[3, 0]);
        }

        [Test]
        public void Fill_NoEmptyCells_ReturnsEmpty()
        {
            var filler = new GridFiller(new FakeRandomProvider(0));
            var grid = new GridModel(2, 2);
            grid[0, 0] = 0; grid[0, 1] = 1;
            grid[1, 0] = 2; grid[1, 1] = 3;

            var filled = filler.Fill(grid, 4);

            Assert.AreEqual(0, filled.Count);
        }

        [Test]
        public void Fill_ReturnsCorrectColorIds()
        {
            var filler = new GridFiller(new FakeRandomProvider(2, 4));
            var grid = new GridModel(2, 1);
            // Both empty

            var filled = filler.Fill(grid, 5);

            Assert.AreEqual(2, filled.Count);
            Assert.AreEqual(2, filled[0].ColorId);
            Assert.AreEqual(4, filled[1].ColorId);
        }
    }
}
