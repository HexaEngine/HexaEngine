namespace HexaEngine.Coroutines
{
    using System;
    using System.Collections;

    public readonly struct WaitUntil(Func<bool> condition) : IEnumerator
    {
        private readonly Func<bool> condition = condition;

        public object? Current { get; } = null;

        public readonly bool MoveNext()
        {
            return condition();
        }

        public readonly void Reset()
        {
        }
    }
}