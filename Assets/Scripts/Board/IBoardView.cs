using System.Threading;
using System.Threading.Tasks;
using TapMatch.GameLogic;
using UnityEngine;

namespace TapMatch.Runtime.Board
{
    /// <summary>
    /// Abstraction over the visual board representation.
    /// Allows the presenter to drive animations without knowing about GameObjects.
    /// </summary>
    public interface IBoardView
    {
        /// <summary>
        /// Spawns a matchable at the given grid position with the specified color.
        /// </summary>
        void SpawnMatchable(int row, int col, int colorId);

        /// <summary>
        /// Removes the matched tiles with a brief animation.
        /// </summary>
        Task AnimateRemovalAsync(TapResult result, CancellationToken ct);

        /// <summary>
        /// Animates existing matchables falling to new positions.
        /// </summary>
        Task AnimateFallsAsync(TapResult result, CancellationToken ct);

        /// <summary>
        /// Spawns new matchables at the top and animates them falling in.
        /// </summary>
        Task AnimateSpawnsAsync(TapResult result, CancellationToken ct);

        /// <summary>
        /// Clears all matchables from the board.
        /// </summary>
        void ClearBoard();
    }
}
