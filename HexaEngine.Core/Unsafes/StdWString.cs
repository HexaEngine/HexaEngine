namespace HexaEngine.Core.Unsafes
{
    using System.Collections;
    using System.Text;

    /// <summary>
    /// Represents a C++-style std::wstring implemented in C#.
    /// </summary>
    public unsafe struct StdWString : IEnumerable<char>
    {
        private const int DefaultCapacity = 4;

        private char* data;
        private int size;
        private int capacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="StdWString"/> struct with default capacity.
        /// </summary>
        public StdWString()
        {
            data = AllocT<char>(DefaultCapacity + 1);
            capacity = DefaultCapacity;
            ZeroMemoryT(data, capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StdWString"/> struct with a specified capacity.
        /// </summary>
        public StdWString(int capacity)
        {
            data = AllocT<char>(capacity);
            this.capacity = capacity;
            ZeroMemoryT(data, capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StdWString"/> struct from a C# string.
        /// </summary>
        public StdWString(string s)
        {
            var byteCount = Encoding.Unicode.GetByteCount(s) / sizeof(char);
            data = AllocT<char>(byteCount + 1);
            capacity = size = s.Length;
            Encoding.Unicode.GetBytes(s, new Span<byte>(data, byteCount));
            data[size] = '\0';
        }

        /// <summary>
        /// Gets a pointer to the internal data.
        /// </summary>
        public readonly char* Data => data;

        /// <summary>
        /// Gets the size (length) of the string.
        /// </summary>
        public readonly int Size => size;

        /// <summary>
        /// Gets a pointer to the first byte of the string.
        /// </summary>
        public readonly char* Front => data;

        /// <summary>
        /// Gets a pointer to the last byte of the string.
        /// </summary>
        public readonly char* Back => data + size - 1;

        /// <summary>
        /// Gets or sets the capacity of the string. Adjusting the capacity can change the memory allocated for the string.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the character at the specified index.
        /// </summary>
        /// <param name="index">The index of the character to get or set.</param>
        /// <returns>The character at the specified index.</returns>
        public char this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        /// <summary>
        /// Gets a C-style null-terminated string (char*) from the data.
        /// </summary>
        /// <returns>A pointer to the null-terminated string.</returns>
        public readonly char* CStr()
        {
            return data;
        }

        /// <summary>
        /// Retrieves the char at the specified index, throwing exceptions for invalid index values.
        /// </summary>
        /// <param name="index">The index of the char to retrieve.</param>
        /// <returns>The char at the specified index.</returns>
        public char At(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, size);
            return this[index];
        }

        /// <summary>
        /// Increases the capacity of the string to a specified value.
        /// </summary>
        /// <param name="capacity">The desired capacity.</param>
        public void Grow(int capacity)
        {
            if (this.capacity < capacity)
            {
                Capacity = capacity;
            }
        }

        /// <summary>
        /// Ensures that the capacity of the string is at least a specified value.
        /// </summary>
        /// <param name="capacity">The desired capacity.</param>
        public void EnsureCapacity(int capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity * 2);
            }
        }

        /// <summary>
        /// Reduces the capacity of the string to match the current size.
        /// </summary>
        public void ShrinkToFit()
        {
            Capacity = size;
        }

        /// <summary>
        /// Resizes the string to the specified size, padding with null bytes if necessary.
        /// </summary>
        /// <param name="size">The new size of the string.</param>
        public void Resize(int size)
        {
            EnsureCapacity(size);
            this.size = size;
            for (int i = size; i < capacity + 1; i++)
            {
                data[i] = '\0';
            }
        }

        /// <summary>
        /// Inserts a char at the specified index in the current <see cref="StdWString"/>.
        /// </summary>
        /// <param name="index">The index where the char is inserted.</param>
        /// <param name="item">The char to insert.</param>
        public void Insert(int index, char item)
        {
            Grow(size + 1);
            MemcpyT(&data[index], &data[index + 1], size - index);
            data[index] = item;
            size++;
        }

        /// <summary>
        /// Inserts the contents of another <see cref="StdWString"/> at the specified index in the current <see cref="StdWString"/>.
        /// </summary>
        /// <param name="index">The index where the insertion starts.</param>
        /// <param name="item">The <see cref="StdWString"/> to insert.</param>
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

        /// <summary>
        /// Inserts a string at the specified index in the current <see cref="StdWString"/>.
        /// </summary>
        /// <param name="index">The index where the insertion starts.</param>
        /// <param name="item">The string to insert.</param>
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

        /// <summary>
        /// Appends a char to the end of the current <see cref="StdWString"/>.
        /// </summary>
        /// <param name="c">The char to append.</param>
        public void Append(char c)
        {
            EnsureCapacity(size + 1);
            data[size] = c;
            size++;
        }

        /// <summary>
        /// Appends the contents of another <see cref="StdWString"/> to the end of the current <see cref="StdWString"/>.
        /// </summary>
        /// <param name="c">The <see cref="StdWString"/> to append.</param>
        public void Append(StdWString c)
        {
            EnsureCapacity(size + c.size);
            MemcpyT(c.data, data + size, c.size);
            size += c.size;
        }

        /// <summary>
        /// Clears the contents of the current <see cref="StdWString"/>.
        /// </summary>
        public void Clear()
        {
            ZeroMemoryT(data, size);
            size = 0;
        }

        /// <summary>
        /// Clears the contents of the current <see cref="StdWString"/> without changing its size.
        /// </summary>
        public readonly void Erase()
        {
            ZeroMemoryT(data, size);
        }

        /// <summary>
        /// Compares the current <see cref="StdWString"/> with a specified char sequence.
        /// </summary>
        /// <param name="other">The char sequence to compare against.</param>
        /// <returns><c>true</c> if the contents are equal; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Compares the current <see cref="StdWString"/> with a specified character sequence.
        /// </summary>
        /// <param name="other">The character sequence to compare against.</param>
        /// <returns><c>true</c> if the contents are equal; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Checks if the current <see cref="StdString"/> starts with a specified character sequence.
        /// </summary>
        /// <param name="str">The character sequence to check for at the beginning.</param>
        /// <returns><c>true</c> if the string starts with the specified sequence; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Checks if the current <see cref="StdString"/> ends with a specified character sequence.
        /// </summary>
        /// <param name="str">The character sequence to check for at the end.</param>
        /// <returns><c>true</c> if the string ends with the specified sequence; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Checks if the current <see cref="StdString"/> starts with a specified character sequence.
        /// </summary>
        /// <param name="str">The character sequence to check for at the beginning.</param>
        /// <returns><c>true</c> if the string starts with the specified sequence; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Checks if the current <see cref="StdString"/> ends with a specified character sequence.
        /// </summary>
        /// <param name="str">The character sequence to check for at the end.</param>
        /// <returns><c>true</c> if the string ends with the specified sequence; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Checks if the current <see cref="StdString"/> contains a specified character sequence.
        /// </summary>
        /// <param name="str">The character sequence to check for.</param>
        /// <returns><c>true</c> if the string contains the specified sequence; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Checks if the current <see cref="StdString"/> contains a specified character sequence.
        /// </summary>
        /// <param name="str">The character sequence to check for.</param>
        /// <returns><c>true</c> if the string contains the specified sequence; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Replaces all occurrences of a target character with a replacement character in the current <see cref="StdString"/>.
        /// </summary>
        /// <param name="target">The character to find and replace.</param>
        /// <param name="replacement">The character to replace the target with.</param>
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

        /// <summary>
        /// Replaces all occurrences of a specific character with another character.
        /// </summary>
        /// <param name="target">The character to be replaced.</param>
        /// <param name="replacement">The character to replace with.</param>
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

        /// <summary>
        /// Replaces all occurrences of a specific character with another character.
        /// </summary>
        /// <param name="target">The character to be replaced.</param>
        /// <param name="replacement">The character to replace with.</param>
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

        /// <summary>
        /// Creates a new <see cref="StdWString"/> that is a substring of the current string, starting at the specified index and having the specified length.
        /// </summary>
        /// <param name="index">The index in the current string where the substring starts.</param>
        /// <param name="length">The length of the substring to create.</param>
        /// <returns>A new <see cref="StdWString"/> representing the substring.</returns>
        public readonly StdWString SubString(int index, int length)
        {
            StdWString @string = new(length);
            @string.Resize(length);
            MemcpyT(data + index, @string.data, length);
            return @string;
        }

        /// <summary>
        /// Creates a new <see cref="StdWString"/> that is a substring of the current string, starting at the specified index and extending to the end of the string.
        /// </summary>
        /// <param name="index">The index in the current string where the substring starts.</param>
        /// <returns>A new <see cref="StdWString"/> representing the substring.</returns>
        public readonly StdWString SubString(int index)
        {
            var length = size - index;
            StdWString @string = new(length);
            @string.Resize(length);
            MemcpyT(data + index, @string.data, length);
            return @string;
        }

        /// <summary>
        /// Searches for the first occurrence of a specified character sequence within the current string, starting at the specified position.
        /// </summary>
        /// <param name="str">The character sequence to search for.</param>
        /// <param name="pos">The starting position for the search.</param>
        /// <returns>The index of the first occurrence of the character sequence, or -1 if it is not found.</returns>
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

        /// <summary>
        /// Searches for the last occurrence of a specified character sequence within the current string, starting at the specified position.
        /// </summary>
        /// <param name="str">The character sequence to search for.</param>
        /// <param name="pos">The starting position for the search.</param>
        /// <returns>The index of the last occurrence of the character sequence, or -1 if it is not found.</returns>
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

        /// <summary>
        /// Searches for the first occurrence of a specified character sequence within the current string, starting at the specified position.
        /// </summary>
        /// <param name="str">The character sequence to search for.</param>
        /// <param name="pos">The starting position for the search.</param>
        /// <returns>The index of the first occurrence of the character sequence, or -1 if it is not found.</returns>
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

        /// <summary>
        /// Searches for the last occurrence of a specified character sequence within the current string, starting at the specified position.
        /// </summary>
        /// <param name="str">The character sequence to search for.</param>
        /// <param name="pos">The starting position for the search.</param>
        /// <returns>The index of the last occurrence of the character sequence, or -1 if it is not found.</returns>
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

        /// <summary>
        /// Swaps the contents of this string with another string.
        /// </summary>
        /// <param name="other">The other string to swap with.</param>
        public void Swap(ref StdWString other)
        {
            var tmpData = data;
            data = other.data;
            other.data = tmpData;
            (other.size, size) = (size, other.size);
            (other.capacity, capacity) = (capacity, other.capacity);
        }

        /// <summary>
        /// Creates a new string with the same content as the current string.
        /// </summary>
        /// <returns>A new <see cref="StdString"/> containing a copy of the current string's data.</returns>
        public readonly StdWString Clone()
        {
            StdWString @string = new(size);
            @string.Resize(size);
            MemcpyT(data, @string.data, size);
            return @string;
        }

        /// <summary>
        /// Returns a <see cref="Span{T}"/> that represents the data in the current <see cref="StdWString"/>.
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> that represents the data.</returns>
        public readonly Span<char> AsSpan()
        {
            return new Span<char>(data, size);
        }

        /// <summary>
        /// Returns a <see cref="ReadOnlySpan{T}"/> that represents the data in the current <see cref="StdWString"/>.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> that represents the data.</returns>
        public readonly ReadOnlySpan<char> AsReadOnlySpan()
        {
            return new ReadOnlySpan<char>(data, size);
        }

        /// <summary>
        /// Concatenates a character to the end of the current <see cref="StdWString"/>.
        /// </summary>
        /// <param name="str">The <see cref="StdWString"/> to concatenate to.</param>
        /// <param name="c">The byte to concatenate.</param>
        /// <returns>The concatenated <see cref="StdWString"/>.</returns>
        public static StdWString operator +(StdWString str, char c)
        {
            str.Append(c);
            return str;
        }

        /// <summary>
        /// Compares two <see cref="StdWString"/> objects for equality.
        /// </summary>
        /// <param name="str1">The first <see cref="StdWString"/> to compare.</param>
        /// <param name="str2">The second <see cref="StdWString"/> to compare.</param>
        /// <returns><c>true</c> if the two strings are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(StdWString str1, StdWString str2)
        {
            return str1.Compare(str2);
        }

        /// <summary>
        /// Compares two <see cref="StdWString"/> objects for inequality.
        /// </summary>
        /// <param name="str1">The first <see cref="StdWString"/> to compare.</param>
        /// <param name="str2">The second <see cref="StdWString"/> to compare.</param>
        /// <returns><c>true</c> if the two strings are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(StdWString str1, StdWString str2)
        {
            return !str1.Compare(str2);
        }

        /// <summary>
        /// Compares a <see cref="StdWString"/> object with a <see cref="string"/> for equality.
        /// </summary>
        /// <param name="str1">The <see cref="StdWString"/> to compare.</param>
        /// <param name="str2">The <see cref="string"/> to compare.</param>
        /// <returns><c>true</c> if the two strings are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(StdWString str1, string str2)
        {
            return str1.Compare(str2);
        }

        /// <summary>
        /// Compares a <see cref="StdWString"/> object with a <see cref="string"/> for inequality.
        /// </summary>
        /// <param name="str1">The <see cref="StdWString"/> to compare.</param>
        /// <param name="str2">The <see cref="string"/> to compare.</param>
        /// <returns><c>true</c> if the two strings are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(StdWString str1, string str2)
        {
            return !str1.Compare(str2);
        }

        /// <summary>
        /// Implicitly converts a <see cref="StdWString"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="StdWString"/> to convert.</param>
        public static implicit operator StdWString(string str)
        {
            return new StdWString(str);
        }

        /// <summary>
        /// Implicitly converts a <see cref="StdWString"/> to a <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="str">The <see cref="StdWString"/> to convert.</param>
        public static implicit operator Span<char>(StdWString str)
        {
            return str.AsSpan();
        }

        /// <summary>
        /// Implicitly converts a <see cref="StdWString"/> to a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="str">The <see cref="StdWString"/> to convert.</param>
        public static implicit operator ReadOnlySpan<char>(StdWString str)
        {
            return str.AsReadOnlySpan();
        }

        /// <summary>
        /// Releases the memory associated with the string.
        /// </summary>
        public void Release()
        {
            Free(data);
            data = null;
            capacity = 0;
            size = 0;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current string.
        /// </summary>
        /// <param name="obj">The object to compare with the current string.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
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

        /// <summary>
        /// Returns a hash code for the string.
        /// </summary>
        /// <returns>The hash code for the string.</returns>
        public override int GetHashCode()
        {
            HashCode hashCode = new();
            for (int i = 0; i < size; i++)
            {
                hashCode.Add(data[i]);
            }
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Converts the string to a C# string using Unicode encoding.
        /// </summary>
        /// <returns>A C# string representing the current string's data.</returns>
        public override readonly string ToString()
        {
            return new(data, 0, size);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the characters of the string.
        /// </summary>
        /// <returns>An enumerator for the characters of the string.</returns>
        public readonly IEnumerator<char> GetEnumerator()
        {
            return new Enumerator(this, false);
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the characters of the string.
        /// </summary>
        /// <returns>An enumerator for the characters of the string.</returns>
        public readonly IEnumerator<char> Begin()
        {
            return new Enumerator(this, false);
        }

        /// <summary>
        /// Returns a reverse enumerator that iterates through the characters of the string.
        /// </summary>
        /// <returns>A reverse enumerator for the characters of the string.</returns>
        public readonly IEnumerator<char> RBegin()
        {
            return new Enumerator(this, true);
        }

        /// <summary>
        /// Enumerator for iterating through the characters of the string.
        /// </summary>
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

            /// <inheritdoc/>
            public char Current => pointer[currentIndex];

            object IEnumerator.Current => Current;

            /// <inheritdoc/>
            public readonly void Dispose()
            {
                // Enumerator does not own resources, so nothing to dispose.
            }

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            public void Reset()
            {
                currentIndex = 0;
            }
        }
    }
}