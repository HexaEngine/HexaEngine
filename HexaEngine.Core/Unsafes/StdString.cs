namespace HexaEngine.Core.Unsafes
{
    using System;
    using System.Collections;
    using System.Text;

    public unsafe struct StdString : IEnumerable<byte>
    {
        private const int DefaultCapacity = 4;

        private byte* data;
        private int size;
        private int capacity;

        public StdString()
        {
            data = AllocT<byte>(DefaultCapacity + 1);
            capacity = DefaultCapacity;
            ZeroMemoryT(data, capacity);
        }

        public StdString(int capacity)
        {
            data = AllocT<byte>(capacity);
            this.capacity = capacity;
            ZeroMemoryT(data, capacity);
        }

        public StdString(string s)
        {
            var byteCount = Encoding.UTF8.GetByteCount(s);
            data = AllocT<byte>(byteCount + 1);
            capacity = size = s.Length;
            Encoding.UTF8.GetBytes(s, new Span<byte>(data, byteCount));
            data[size] = (byte)'\0';
        }

        public readonly byte* Data => data;

        public readonly int Size => size;

        public readonly byte* Front => data;

        public readonly byte* Back => data + size - 1;

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
                    data[i] = (byte)'\0';
                }
            }
        }

        public byte this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public readonly byte* CStr()
        {
            return data;
        }

        public byte At(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, size);
            return this[index];
        }

        public void Grow(int capacity)
        {
            if (this.capacity < capacity || data == null)
            {
                Capacity = capacity;
            }
        }

        public void EnsureCapacity(int capacity)
        {
            if (this.capacity < capacity || data == null)
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
                data[i] = (byte)'\0';
            }
        }

        public void Insert(int index, byte item)
        {
            Grow(size + 1);
            MemcpyT(&data[index], &data[index + 1], size - index);
            data[index] = item;
            size++;
        }

        public void InsertRange(int index, StdString item)
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
                data[index + i] = (byte)item[i];
            }

            size += item.Length;
        }

        public void Append(byte c)
        {
            EnsureCapacity(size + 1);
            data[size] = c;
            size++;
        }

        public void Append(StdString c)
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

        public readonly void Replace(byte target, byte replacement)
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

                        for (int j = 0; j < replacement.Length; j++)
                        {
                            data[idx + j] = replacement[j];
                        }

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

                        for (int j = 0; j < replacement.Length; j++)
                        {
                            data[idx + j] = (byte)replacement[j];
                        }

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

        public readonly StdString SubString(int index, int length)
        {
            StdString @string = new(length);
            @string.Resize(length);
            MemcpyT(data + index, @string.data, length);
            return @string;
        }

        public readonly StdString SubString(int index)
        {
            var length = size - index;
            StdString @string = new(length);
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

        public void Swap(ref StdString other)
        {
            var tmpData = data;
            data = other.data;
            other.data = tmpData;
            (other.size, size) = (size, other.size);
            (other.capacity, capacity) = (capacity, other.capacity);
        }

        public readonly StdString Clone()
        {
            StdString @string = new(size);
            @string.Resize(size);
            MemcpyT(data, @string.data, size);
            return @string;
        }

        public StdWString ToWString()
        {
            StdWString str = new();
            str.Resize(size);
            for (int i = 0; i < size; i++)
            {
                str[i] = (char)this[i];
            }
            return str;
        }

        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(data, size);
        }

        public readonly ReadOnlySpan<byte> AsReadOnlySpan()
        {
            return new ReadOnlySpan<byte>(data, size);
        }

        public static StdString operator +(StdString str, byte c)
        {
            str.Append(c);
            return str;
        }

        public static bool operator ==(StdString str1, StdString str2)
        {
            return str1.Compare(str2);
        }

        public static bool operator !=(StdString str1, StdString str2)
        {
            return !str1.Compare(str2);
        }

        public static bool operator ==(StdString str1, string str2)
        {
            return str1.Compare(str2);
        }

        public static bool operator !=(StdString str1, string str2)
        {
            return !str1.Compare(str2);
        }

        public static implicit operator StdString(string str)
        {
            return new StdString(str);
        }

        public static implicit operator Span<byte>(StdString str)
        {
            return str.AsSpan();
        }

        public static implicit operator ReadOnlySpan<byte>(StdString str)
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
            if (obj is StdString stdString)
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
            return Encoding.UTF8.GetString(data, size);
        }

        public readonly IEnumerator<byte> GetEnumerator()
        {
            return new Enumerator(this, false);
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public readonly IEnumerator<byte> Begin()
        {
            return new Enumerator(this, false);
        }

        public readonly IEnumerator<byte> RBegin()
        {
            return new Enumerator(this, true);
        }

        public struct Enumerator : IEnumerator<byte>
        {
            private readonly byte* pointer;
            private readonly int size;
            private readonly bool reverse;
            private int currentIndex;

            internal Enumerator(StdString str, bool reverse)
            {
                pointer = str.data;
                size = str.size;
                currentIndex = reverse ? size : -1;
                this.reverse = reverse;
            }

            public byte Current => pointer[currentIndex];

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
                else if (currentIndex < size - 1)
                {
                    currentIndex++;
                    return true;
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