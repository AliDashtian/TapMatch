

using NUnit.Framework;

namespace TapMatch.GameLogic.Tests
{
    [TestFixture]
    public sealed class MatchFinderTests
    {
        private MatchFinder _finder;

        [SetUp]
        public void SetUp()
        {
            _finder = new MatchFinder();
        }

        [Test]
        public void FindConnected_SingleColor_ReturnsAllConnected()
        {
            // Grid:
            // 0 1 0
            // 0 0 1
            // 1 0 0
            var grid = new GridModel(3, 3);
            grid[0, 0] = 0; grid[0, 1] = 1; grid[0, 2] = 0;
            grid[1, 0] = 0; grid[1, 1] = 0; grid[1, 2] = 1;
            grid[2, 0] = 1; grid[2, 1] = 0; grid[2, 2] = 0;

            var result = _finder.FindConnected(grid, 0, 0);

            // Connected 0s from (0,0): (0,0), (1,0), (1,1), (2,1), (2,2)
            Assert.AreEqual(5, result.Count);
            Assert.IsTrue(result.Contains((0, 0)));
            Assert.IsTrue(result.Contains((1, 0)));
            Assert.IsTrue(result.Contains((1, 1)));
            Assert.IsTrue(result.Contains((2, 1)));
            Assert.IsTrue(result.Contains((2, 2)));
        }

        [Test]
        public void FindConnected_IsolatedCell_ReturnsSingleCell()
        {
            // Grid:
            // 0 1 0
            // 1 1 1
            // 0 1 0
            var grid = new GridModel(3, 3);
            grid[0, 0] = 0; grid[0, 1] = 1; grid[0, 2] = 0;
            grid[1, 0] = 1; grid[1, 1] = 1; grid[1, 2] = 1;
            grid[2, 0] = 0; grid[2, 1] = 1; grid[2, 2] = 0;

            var result = _finder.FindConnected(grid, 0, 0);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains((0, 0)));
        }

        [Test]
        public void FindConnected_EntireGridSameColor_ReturnsAll()
        {
            var grid = new GridModel(3, 3);
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    grid[r, c] = 2;

            var result = _finder.FindConnected(grid, 1, 1);

            Assert.AreEqual(9, result.Count);
        }

        [Test]
        public void FindConnected_DoesNotIncludeDiagonals()
        {
            // Grid:
            // 0 1
            // 1 0
            var grid = new GridModel(2, 2);
            grid[0, 0] = 0; grid[0, 1] = 1;
            grid[1, 0] = 1; grid[1, 1] = 0;

            var result = _finder.FindConnected(grid, 0, 0);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains((0, 0)));
        }

        [Test]
        public void FindConnected_EmptyCell_ReturnsEmpty()
        {
            var grid = new GridModel(3, 3);

            var result = _finder.FindConnected(grid, 1, 1);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindConnected_OutOfBounds_ReturnsEmpty()
        {
            var grid = new GridModel(3, 3);

            var result = _finder.FindConnected(grid, -1, 0);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindConnected_LShapedGroup_FindsAll()
        {
            // Grid:
            // 1 0 0
            // 1 0 0
            // 1 1 1
            var grid = new GridModel(3, 3);
            grid[0, 0] = 1; grid[0, 1] = 0; grid[0, 2] = 0;
            grid[1, 0] = 1; grid[1, 1] = 0; grid[1, 2] = 0;
            grid[2, 0] = 1; grid[2, 1] = 1; grid[2, 2] = 1;

            var result = _finder.FindConnected(grid, 0, 0);

            Assert.AreEqual(5, result.Count);
        }

        [Test]
        public void FindConnected_AssignmentExample_MatchesSpec()
        {
            // From the assignment PDF:
            // 0 4 2 4 1     (row 0, top)
            // 3 X x x 0     (row 1, X=tapped)
            // 2 0 x 0 3     (row 2)
            // 0 x x 1 2     (row 3, bottom)
            // Using color 5 for 'x' (the matched color)
            var grid = new GridModel(4, 5);
            grid[0, 0] = 0; grid[0, 1] = 4; grid[0, 2] = 2; grid[0, 3] = 4; grid[0, 4] = 1;
            grid[1, 0] = 3; grid[1, 1] = 5; grid[1, 2] = 5; grid[1, 3] = 5; grid[1, 4] = 0;
            grid[2, 0] = 2; grid[2, 1] = 0; grid[2, 2] = 5; grid[2, 3] = 0; grid[2, 4] = 3;
            grid[3, 0] = 0; grid[3, 1] = 5; grid[3, 2] = 5; grid[3, 3] = 1; grid[3, 4] = 2;

            // Tap on (1,1) — the 'X'
            var result = _finder.FindConnected(grid, 1, 1);

            // Should find: (1,1), (1,2), (1,3), (2,2), (3,1), (3,2)
            Assert.AreEqual(6, result.Count);
            Assert.IsTrue(result.Contains((1, 1)));
            Assert.IsTrue(result.Contains((1, 2)));
            Assert.IsTrue(result.Contains((1, 3)));
            Assert.IsTrue(result.Contains((2, 2)));
            Assert.IsTrue(result.Contains((3, 1)));
            Assert.IsTrue(result.Contains((3, 2)));
        }
    }
}
