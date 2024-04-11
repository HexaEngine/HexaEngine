namespace HexaEngine.Core.Threading
{
    using HexaEngine.Core.Debugging;
    using System;
    using System.Collections;

    public readonly struct ReadWriteLockEnumerator<T>(ReadWriteLock readWriteLock, IEnumerator<T> enumerator) : IEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator = enumerator;
        private readonly IDisposable readHandle = readWriteLock.BeginReadBlock();

        public readonly T Current => enumerator.Current;

        object IEnumerator.Current => ((IEnumerator)enumerator).Current;

        public readonly void Dispose()
        {
            enumerator.Dispose();
            readHandle.Dispose();
        }

        public readonly bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public readonly void Reset()
        {
            enumerator.Reset();
        }
    }

    public readonly struct ReadWriteLockEnumerable<T>(ReadWriteLock readWriteLock, IEnumerable<T> enumerable) : IEnumerable<T>
    {
        private readonly ReadWriteLock readWriteLock = readWriteLock;
        private readonly IEnumerable<T> enumerable = enumerable;

        public IEnumerator<T> GetEnumerator()
        {
            return new ReadWriteLockEnumerator<T>(readWriteLock, enumerable.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}