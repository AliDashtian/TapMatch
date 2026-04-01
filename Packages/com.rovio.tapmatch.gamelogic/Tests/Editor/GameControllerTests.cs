using NUnit.Framework;
using System;

namespace TapMatch.GameLogic.Tests
{
    [TestFixture]
    public sealed class GameControllerTests
    {
        private GameController CreateController(int rows, int cols, int colors, params int[] randomValues)
        {
            var grid = new GridModel(rows, cols);
            var random = new FakeRandomProvider(randomValues);
            var filler = new GridFiller(random);
            var controller = new GameController(
                grid,
                colors,
                new MatchFinder(),
                new GridCollapser(),
                filler);

            return controller;
        }

        [Test]
        public void Constructor_NullGrid_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GameController(null, 3, new MatchFinder(), new GridCollapser(),
                    new GridFiller(new FakeRandomProvider(0))));
        }

        [Test]
        public void Constructor_ZeroColors_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameController(new GridModel(3, 3), 0, new MatchFinder(),
                    new GridCollapser(), new GridFiller(new FakeRandomProvider(0))));
        }

        [Test]
        public void Initialize_FillsEntireGrid()
        {
            var controller = CreateController(3, 3, 4, 0, 1, 2, 3, 0, 1, 2, 3, 0);

            controller.Initialize();

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.IsFalse(controller.Grid.IsEmpty(r, c), $"Cell [{r},{c}] should be filled");
        }

        [Test]
        public void Initialize_SetsStateToIdle()
        {
            var controller = CreateController(3, 3, 4, 0, 1, 2, 3, 0, 1, 2, 3, 0);

            controller.Initialize();

            Assert.AreEqual(GameController.State.Idle, controller.CurrentState);
        }

        [Test]
        public void TryTap_OutOfBounds_ReturnsNull()
        {
            var controller = CreateController(3, 3, 3, 0, 1, 2, 0, 1, 2, 0, 1, 2);
            controller.Initialize();

            var result = controller.TryTap(-1, 0);

            Assert.IsNull(result);
        }

        [Test]
        public void TryTap_SingleIsolatedCell_ReturnsNull()
        {
            // Grid where no cell has a same-color neighbor
            // 0 1 2
            // 1 2 0
            // 2 0 1
            var controller = CreateController(3, 3, 3, 0, 1, 2, 1, 2, 0, 2, 0, 1);
            controller.Initialize();

            var result = controller.TryTap(0, 0);

            Assert.IsNull(result, "Single isolated matchable (group size < 2) should not be tappable");
        }

        [Test]
        public void TryTap_ConnectedGroup_RemovesAndFills()
        {
            // Grid:
            // 0 0
            // 1 1
            var controller = CreateController(2, 2, 2, 0, 0, 1, 1, 2, 2);
            // After init: random gives 0,0,1,1 → grid is [0,0 / 1,1]
            // After tap on (0,0): removes (0,0),(0,1), collapse (nothing to collapse for row 0),
            // then fill with next random values: 2%2=0, 2%2=0 → refills top row
            controller.Initialize();

            var result = controller.TryTap(0, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Removed.Count);
            Assert.IsTrue(result.Spawned.Count > 0);
        }

        [Test]
        public void TryTap_ValidTap_GridIsFullAfterward()
        {
            // Ensure the grid has no empty cells after a valid tap
            var controller = CreateController(3, 3, 2,
                0, 0, 0, 1, 1, 1, 0, 0, 0,  // initial fill
                1, 1, 1, 1, 1, 1, 1, 1, 1); // refill values
            controller.Initialize();

            controller.TryTap(0, 0);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.IsFalse(controller.Grid.IsEmpty(r, c),
                        $"Cell [{r},{c}] should be filled after tap");
        }

        [Test]
        public void TryTap_ReturnsCorrectFallMoves()
        {
            // Grid:
            // 0 1
            // 0 1
            // Tap (0,0) removes both 0s from column 0
            var controller = CreateController(2, 2, 2, 0, 1, 0, 1, 2, 2, 2, 2);
            controller.Initialize();

            var result = controller.TryTap(0, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Removed.Count);
            // After removing column 0, both cells are empty, so spawns fill them
            Assert.AreEqual(2, result.Spawned.Count);
        }

        [Test]
        public void TryTap_StateReturnsToIdle_AfterProcessing()
        {
            var controller = CreateController(2, 2, 2, 0, 0, 1, 1, 0, 0);
            controller.Initialize();

            controller.TryTap(0, 0);

            Assert.AreEqual(GameController.State.Idle, controller.CurrentState);
        }
    }
}
