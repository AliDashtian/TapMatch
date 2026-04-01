
namespace TapMatch.GameLogic
{
    /// <summary>
    /// Abstraction over random number generation.
    /// Enables deterministic testing by injecting a controlled implementation.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>
        /// Returns a random integer in [0, maxExclusive).
        /// </summary>
        int Next(int maxExclusive);
    }
}
