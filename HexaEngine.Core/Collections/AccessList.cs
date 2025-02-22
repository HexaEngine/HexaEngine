using System.Collections;

namespace HexaEngine.Core.Collections
{
    public class AccessibleList<T> : IList<T>
    {
        private T[] values;
        private int count;
        private int capacity;

        private const int DefaultCapacity = 4;

        public AccessibleList() : this(DefaultCapacity)
        {
        }

        public AccessibleList(int capacity)
        {
            values = new T[capacity];
            this.capacity = capacity;
        }

        public AccessibleList(IEnumerable<T> values) : this(DefaultCapacity)
        {
            AddRange(values);
        }

        public int Capacity
        {
            get => capacity;
            set
            {
                Array.Resize(ref values, value);
                capacity = value;
                count = Math.Min(count, value);
            }
        }

        public T[] Values => values;

        public int Count => count;

        public bool IsReadOnly { get; }

        public T this[int index]
        {
            get => values[index];
            set => values[index] = value;
        }

        public void Add(T item)
        {
            int newSize = count + 1;
            if (newSize > capacity)
            {
                AddAndGrow(item);
                return;
            }
            values[count] = item;
            count = newSize;
        }

        public void AddRange(IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                Add(item);
            }
        }

        private void AddAndGrow(T item)
        {
            int newCapacity = capacity * 2;
            Array.Resize(ref values, newCapacity);
            capacity = newCapacity;
            values[count] = item;
            count++;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(values, item);
        }

        public void RemoveAt(int index)
        {
            if (index == count - 1)
            {
                count--;
                return;
            }

            Array.Copy(values, index + 1, values, index, count - index - 1);
            count--;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        public void Insert(int index, T item)
        {
            if (index == count)
            {
                Add(item);
            }
            else
            {
                int newSize = count + 1;
                if (newSize > capacity)
                {
                    int newCapacity = capacity * 2;
                    Array.Resize(ref values, newCapacity);
                    capacity = newCapacity;
                }

                Array.Copy(values, index, values, index + 1, count - index);

                values[index] = item;

                count++;
            }
        }

        public void Clear()
        {
            if (!typeof(T).IsValueType)
            {
                Array.Clear(values, 0, count);
            }
            count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(values, 0, array, arrayIndex, count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(values, count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(values, count);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] values;
            private readonly int count;
            private int currentIndex = -1;

            public Enumerator(T[] values, int count)
            {
                this.values = values;
                this.count = count;
            }

            public readonly T Current
            {
                get
                {
                    return values[currentIndex];
                }
            }

            readonly object? IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (currentIndex < count - 1)
                {
                    currentIndex++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public readonly void Dispose()
            {
            }
        }
    }
}