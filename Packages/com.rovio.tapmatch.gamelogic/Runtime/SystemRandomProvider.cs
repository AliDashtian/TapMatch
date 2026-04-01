
namespace TapMatch.GameLogic
{
    /// <summary>
    /// Default implementation using System.Random.
    /// For deterministic testing, use FakeRandomProvider instead.
    /// </summary>
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private readonly System.Random _random = new();

        public int Next(int maxExclusive) => _random.Next(maxExclusive);
    }
}
