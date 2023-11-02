namespace HexaEngine.Core.Unsafes
{
    using System.Collections;
    using System.Text;

    public unsafe struct StdWString : IEnumerable<char>
    {
        private const int DefaultCapacity = 4;

        private char* data;
        private int size;
        private int capacity;

        public StdWString()
        {
            data = AllocT<char>(DefaultCapacity + 1);
            capacity = DefaultCapacity;
            ZeroMemoryT(data, capacity);
        }

        public StdWString(int capacity)
        {
            data = AllocT<char>(capacity);
            this.capacity = capacity;
            ZeroMemoryT(data, capacity);
        }

        public StdWString(string s)
        {
            var byteCount = Encoding.Unicode.GetByteCount(s) / sizeof(char);
            data = AllocT<char>(byteCount + 1);
            capacity = size = s.Length;
            Encoding.Unicode.GetBytes(s, new Span<byte>(data, byteCount));
            data[size] = '\0';
        }

        public readonly char* Data => data;

        public readonly int Size => size;

        public readonly char* Front => data;

        public readonly char* Back => data + size - 1;

        public int Capacity
        {
            readonly get => capacity;
            set
            {
                capacity = value;
                size = size > value ? value : size;
                data = ReAllocT(data, value + 1);
                for (int i = size; i < value + 1; i++)
                {
                    data[i] = '\0';
                }
            }
        }

        public char this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public readonly char* CStr()
        {
            return data;
        }

        public char At(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, size);
            return this[index];
        }

        public void Grow(int capacity)
        {
            if (this.capacity < capacity)
            {
                Capacity = capacity;
            }
        }

        public void EnsureCapacity(int capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity * 2);
            }
        }

        public void ShrinkToFit()
        {
            Capacity = size;
        }

        public void Resize(int size)
        {
            EnsureCapacity(size);
            this.size = size;
            for (int i = size; i < capacity + 1; i++)
            {
                data[i] = '\0';
            }
        }

        public void Insert(int index, char item)
        {
            Grow(size + 1);
            MemcpyT(&data[index], &data[index + 1], size - index);
            data[index] = item;
            size++;
        }

        public void InsertRange(int index, StdWString item)
        {
            Grow(size + item.size);
            MemcpyT(&data[index], &data[index + item.size], size - index);
            for (int i = 0; i < item.size; i++)
            {
                data[index + i] = item[i];
            }
            size += item.size;
        }

        public void InsertRange(int index, string item)
        {
            Grow(size + item.Length);
            MemcpyT(&data[index], &data[index + item.Length], size - index);
            for (int i = 0; i < item.Length; i++)
            {
                data[index + i] = item[i];
            }
            size += item.Length;
        }

        public void Append(char c)
        {
            EnsureCapacity(size + 1);
            data[size] = c;
            size++;
        }

        public void Append(StdWString c)
        {
            EnsureCapacity(size + c.size);
            MemcpyT(c.data, data + size, c.size);
            size += c.size;
        }

        public void Clear()
        {
            ZeroMemoryT(data, size);
            size = 0;
        }

        public readonly void Erase()
        {
            ZeroMemoryT(data, size);
        }

        public bool Compare(ReadOnlySpan<char> other)
        {
            if (size != other.Length)
                return false;
            for (int i = 0; i < size; i++)
            {
                if (data[i] != other[i])
                    return false;
            }
            return true;
        }

        public bool Compare(ReadOnlySpan<byte> other)
        {
            if (size != other.Length)
                return false;
            for (int i = 0; i < size; i++)
            {
                if (data[i] != other[i])
                    return false;
            }
            return true;
        }

        public bool StartsWith(ReadOnlySpan<char> str)
        {
            if (size < str.Length)
                return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (data[i] != str[i])
                    return false;
            }
            return true;
        }

        public bool EndsWith(ReadOnlySpan<char> str)
        {
            if (size < str.Length)
                return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (data[size - 1 - i] != str[i])
                    return false;
            }
            return true;
        }

        public bool StartsWith(ReadOnlySpan<byte> str)
        {
            if (size < str.Length)
                return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (data[i] != str[i])
                    return false;
            }
            return true;
        }

        public bool EndsWith(ReadOnlySpan<byte> str)
        {
            if (size < str.Length)
                return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (data[size - 1 - i] != str[i])
                    return false;
            }
            return true;
        }

        public bool Contains(ReadOnlySpan<char> str)
        {
            if (str.Length > size)
                return false;

            int cmp = 0;
            for (int i = 0; i < size; i++)
            {
                if (data[i] == str[cmp])
                {
                    cmp++;
                    if (cmp == str.Length)
                    {
                        return true;
                    }
                }
                else
                {
                    cmp = 0;
                }
            }

            return cmp == str.Length;
        }

        public bool Contains(ReadOnlySpan<byte> str)
        {
            if (str.Length > size)
                return false;

            int cmp = 0;
            for (int i = 0; i < size; i++)
            {
                if (data[i] == str[cmp])
                {
                    cmp++;
                    if (cmp == str.Length)
                    {
                        return true;
                    }
                }
                else
                {
                    cmp = 0;
                }
            }

            return cmp == str.Length;
        }

        public readonly void Replace(char target, char replacement)
        {
            for (int i = 0; i < size; i++)
            {
                if (data[i] == target)
                {
                    data[i] = replacement;
                }
            }
        }

        public void Replace(ReadOnlySpan<byte> target, ReadOnlySpan<byte> replacement)
        {
            if (size < target.Length)
                return;

            int cmp = 0;

            for (int i = 0; i < size; i++)
            {
                if (data[i] == target[cmp])
                {
                    cmp++;

                    if (cmp == target.Length)
                    {
                        var idx = i - cmp + 1;
                        var newSize = size - cmp + replacement.Length;

                        Grow(newSize);

                        if (i + 1 != size)
                        {
                            // copy data forward/backwards to ensure no data is lost or is additionally there where no data should be.
                            int delta = replacement.Length - target.Length;
                            MemcpyT(data + idx + target.Length, data + idx + replacement.Length, size - delta);
                        }

                        // copy the replacement into the gap.
                        for (int j = 0; j < replacement.Length; j++)
                        {
                            data[idx + j] = (char)replacement[j];
                        }

                        // update state.
                        size = newSize;
                        i = idx + replacement.Length;
                        cmp = 0;
                    }
                }
                else
                {
                    cmp = 0;
                }
            }
        }

        public void Replace(ReadOnlySpan<char> target, ReadOnlySpan<char> replacement)
        {
            if (size < target.Length)
                return;

            int cmp = 0;

            for (int i = 0; i < size; i++)
            {
                if (data[i] == target[cmp])
                {
                    cmp++;

                    if (cmp == target.Length)
                    {
                        var idx = i - cmp + 1;
                        var newSize = size - cmp + replacement.Length;

                        Grow(newSize);

                        if (i + 1 != size)
                        {
                            // copy data forward/backwards to ensure no data is lost or is additionally there where no data should be.
                            int delta = replacement.Length - target.Length;
                            MemcpyT(data + idx + target.Length, data + idx + replacement.Length, size - delta);
                        }

                        // copy the replacement into the gap.
                        for (int j = 0; j < replacement.Length; j++)
                        {
                            data[idx + j] = replacement[j];
                        }

                        // update state.
                        size = newSize;
                        i = idx + replacement.Length;
                        cmp = 0;
                    }
                }
                else
                {
                    cmp = 0;
                }
            }
        }

        public readonly StdWString SubString(int index, int length)
        {
            StdWString @string = new(length);
            @string.Resize(length);
            MemcpyT(data + index, @string.data, length);
            return @string;
        }

        public readonly StdWString SubString(int index)
        {
            var length = size - index;
            StdWString @string = new(length);
            @string.Resize(length);
            MemcpyT(data + index, @string.data, length);
            return @string;
        }

        public int Find(ReadOnlySpan<char> str, int pos)
        {
            if (str.Length > size - pos)
                return -1;

            int cmp = 0;
            for (int i = pos; i < size; i++)
            {
                if (data[i] == str[cmp])
                {
                    cmp++;
                    if (cmp == str.Length)
                    {
                        return i - cmp + 1;
                    }
                }
                else
                {
                    cmp = 0;
                }
            }

            return -1;
        }

        public int FindLast(ReadOnlySpan<char> str, int pos)
        {
            if (str.Length > size - pos)
                return -1;

            int cmp = str.Length - 1;
            for (int i = pos; i >= 0; i--)
            {
                if (data[i] == str[cmp])
                {
                    cmp--;
                    if (cmp == 0)
                    {
                        return i;
                    }
                }
                else
                {
                    cmp = str.Length - 1;
                }
            }

            return -1;
        }

        public int Find(ReadOnlySpan<byte> str, int pos)
        {
            if (str.Length > size - pos)
                return -1;

            int cmp = 0;
            for (int i = pos; i < size; i++)
            {
                if (data[i] == str[cmp])
                {
                    cmp++;
                    if (cmp == str.Length)
                    {
                        return i - cmp + 1;
                    }
                }
                else
                {
                    cmp = 0;
                }
            }

            return -1;
        }

        public int FindLast(ReadOnlySpan<byte> str, int pos)
        {
            if (str.Length > size - pos)
                return -1;

            int cmp = str.Length - 1;
            for (int i = pos; i >= 0; i--)
            {
                if (data[i] == str[cmp])
                {
                    cmp--;
                    if (cmp == 0)
                    {
                        return i;
                    }
                }
                else
                {
                    cmp = str.Length - 1;
                }
            }

            return -1;
        }

        public void Swap(ref StdWString other)
        {
            var tmpData = data;
            data = other.data;
            other.data = tmpData;
            (other.size, size) = (size, other.size);
            (other.capacity, capacity) = (capacity, other.capacity);
        }

        public readonly StdWString Clone()
        {
            StdWString @string = new(size);
            @string.Resize(size);
            MemcpyT(data, @string.data, size);
            return @string;
        }

        public readonly Span<char> AsSpan()
        {
            return new Span<char>(data, size);
        }

        public readonly ReadOnlySpan<char> AsReadOnlySpan()
        {
            return new ReadOnlySpan<char>(data, size);
        }

        public static StdWString operator +(StdWString str, char c)
        {
            str.Append(c);
            return str;
        }

        public static bool operator ==(StdWString str1, StdWString str2)
        {
            return str1.Compare(str2);
        }

        public static bool operator !=(StdWString str1, StdWString str2)
        {
            return !str1.Compare(str2);
        }

        public static bool operator ==(StdWString str1, string str2)
        {
            return str1.Compare(str2);
        }

        public static bool operator !=(StdWString str1, string str2)
        {
            return !str1.Compare(str2);
        }

        public static implicit operator StdWString(string str)
        {
            return new StdWString(str);
        }

        public static implicit operator Span<char>(StdWString str)
        {
            return str.AsSpan();
        }

        public static implicit operator ReadOnlySpan<char>(StdWString str)
        {
            return str.AsReadOnlySpan();
        }

        public void Release()
        {
            Free(data);
            data = null;
            capacity = 0;
            size = 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StdWString stdString)
            {
                return Compare(stdString);
            }
            if (obj is string str)
            {
                return Compare(str);
            }
            return false;
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            for (int i = 0; i < size; i++)
            {
                hashCode.Add(data[i]);
            }
            return hashCode.ToHashCode();
        }

        public override readonly string ToString()
        {
            return new(data, 0, size);
        }

        public readonly IEnumerator<char> GetEnumerator()
        {
            return new Enumerator(this, false);
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public readonly IEnumerator<char> Begin()
        {
            return new Enumerator(this, false);
        }

        public readonly IEnumerator<char> RBegin()
        {
            return new Enumerator(this, true);
        }

        public struct Enumerator : IEnumerator<char>
        {
            private readonly char* pointer;
            private readonly int size;
            private readonly bool reverse;
            private int currentIndex;

            internal Enumerator(StdWString str, bool reverse)
            {
                pointer = str.data;
                size = str.size;
                currentIndex = reverse ? size : -1;
                this.reverse = reverse;
            }

            public char Current => pointer[currentIndex];

            object IEnumerator.Current => Current;

            public readonly void Dispose()
            {
                // Enumerator does not own resources, so nothing to dispose.
            }

            public bool MoveNext()
            {
                if (reverse)
                {
                    if (currentIndex > 0)
                    {
                        currentIndex--;
                        return true;
                    }
                }
                else
                {
                    if (currentIndex < size - 1)
                    {
                        currentIndex++;
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                currentIndex = 0;
            }
        }
    }
}