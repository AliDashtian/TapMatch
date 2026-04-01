using System.Collections.Generic;


namespace TapMatch.GameLogic.Tests
{
    /// <summary>
    /// Deterministic random provider for unit tests.
    /// Returns values from a pre-defined sequence, cycling if exhausted.
    /// </summary>
    public sealed class FakeRandomProvider : IRandomProvider
    {
        private readonly List<int> _values;
        private int _index;

        public FakeRandomProvider(params int[] values)
        {
            _values = new List<int>(values);
            _index = 0;
        }

        public int Next(int maxExclusive)
        {
            if (_values.Count == 0) return 0;

            int value = _values[_index % _values.Count];
            _index++;
            return value % maxExclusive;
        }
    }
}
