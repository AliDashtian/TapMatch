

using NUnit.Framework;
using System;

namespace TapMatch.GameLogic.Tests
{
    [TestFixture]
    public sealed class GridModelTests
    {
        [Test]
        public void Constructor_ValidDimensions_CreatesGrid()
        {
            var grid = new GridModel(4, 5);

            Assert.AreEqual(4, grid.Rows);
            Assert.AreEqual(5, grid.Columns);
        }

        [Test]
        public void Constructor_ZeroRows_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridModel(0, 5));
        }

        [Test]
        public void Constructor_ZeroColumns_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridModel(4, 0));
        }

        [Test]
        public void Constructor_NegativeDimensions_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridModel(-1, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridModel(4, -3));
        }

        [Test]
        public void NewGrid_AllCellsAreEmpty()
        {
            var grid = new GridModel(3, 3);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.IsTrue(grid.IsEmpty(r, c), $"Cell [{r},{c}] should be empty");
        }

        [Test]
        public void Indexer_SetAndGet_ReturnsCorrectValue()
        {
            var grid = new GridModel(3, 3);

            grid[1, 2] = 5;

            Assert.AreEqual(5, grid[1, 2]);
        }

        [Test]
        public void IsInBounds_ValidCoordinates_ReturnsTrue()
        {
            var grid = new GridModel(4, 5);

            Assert.IsTrue(grid.IsInBounds(0, 0));
            Assert.IsTrue(grid.IsInBounds(3, 4));
            Assert.IsTrue(grid.IsInBounds(2, 2));
        }

        [Test]
        public void IsInBounds_OutOfRange_ReturnsFalse()
        {
            var grid = new GridModel(4, 5);

            Assert.IsFalse(grid.IsInBounds(-1, 0));
            Assert.IsFalse(grid.IsInBounds(0, -1));
            Assert.IsFalse(grid.IsInBounds(4, 0));
            Assert.IsFalse(grid.IsInBounds(0, 5));
        }

        [Test]
        public void IsEmpty_EmptyCell_ReturnsTrue()
        {
            var grid = new GridModel(3, 3);

            Assert.IsTrue(grid.IsEmpty(0, 0));
        }

        [Test]
        public void IsEmpty_FilledCell_ReturnsFalse()
        {
            var grid = new GridModel(3, 3);
            grid[0, 0] = 2;

            Assert.IsFalse(grid.IsEmpty(0, 0));
        }

        [Test]
        public void Clear_ResetsAllCellsToEmpty()
        {
            var grid = new GridModel(2, 2);
            grid[0, 0] = 1;
            grid[0, 1] = 2;
            grid[1, 0] = 3;
            grid[1, 1] = 4;

            grid.Clear();

            for (int r = 0; r < 2; r++)
                for (int c = 0; c < 2; c++)
                    Assert.IsTrue(grid.IsEmpty(r, c));
        }
    }
}
