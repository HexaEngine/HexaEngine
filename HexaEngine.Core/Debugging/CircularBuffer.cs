namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] buffer;
        private int head;
        private int tail;
        public readonly int Mask;

        public CircularBuffer(int capacity)
        {
            buffer = new T[capacity];
            head = 0;
            tail = 0;
            Mask = capacity - 1;
            // check power of two
            if (!BitOperations.IsPow2(capacity))
            {
                throw new InvalidOperationException("Capacity must be power of two.");
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return head >= tail ? head - tail : buffer.Length - tail + head;
            }
        }

        public int Head => head;

        public int Tail => tail;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(in T item)
        {
            buffer[head] = item;
            head = (head + 1) & Mask;
            if (head == tail)
            {
                tail = (tail + 1) & Mask;
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            head = 0;
            tail = 0;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return buffer[(tail + index) & Mask];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
                buffer[(tail + index) & Mask] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetDirect(int index)
        {
            return ref buffer[index];
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly CircularBuffer<T> circularBuffer;
            private int index;

            public Enumerator(CircularBuffer<T> circularBuffer)
            {
                this.circularBuffer = circularBuffer;
                index = circularBuffer.tail - 1;
            }

            public readonly T Current => circularBuffer.GetDirect(index);

            readonly object IEnumerator.Current => circularBuffer.GetDirect(index)!;

            public readonly int LogicalIndex => index < circularBuffer.tail ? circularBuffer.buffer.Length - circularBuffer.tail + index : index - circularBuffer.tail;

            public readonly int AbsoluteIndex => index;

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                index = (index + 1) & circularBuffer.Mask;
                return index != circularBuffer.head;
            }

            public void Reset()
            {
                index = circularBuffer.tail - 1;
            }

            public void Skip(int count)
            {
                index = (index + count) & circularBuffer.Mask;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}