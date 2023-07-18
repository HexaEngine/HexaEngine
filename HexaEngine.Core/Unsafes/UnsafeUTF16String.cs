namespace HexaEngine.Core.Unsafes
{
    using System.Runtime.InteropServices;

    public unsafe struct UnsafeUTF16String : IFreeable
    {
        public char* Ptr;
        public nint Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeUTF16String"/> struct with the specified string.
        /// </summary>
        /// <param name="str">The string to initialize the UnsafeUTF16String with.</param>
        public UnsafeUTF16String(string str)
        {
            int sizeInBytes = (str.Length + 1) * sizeof(char);
            Ptr = (char*)Marshal.AllocHGlobal(sizeInBytes);
            fixed (char* strPtr = str)
            {
                MemoryCopy(strPtr, Ptr, sizeInBytes, sizeInBytes);
            }
            Length = str.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeUTF16String"/> struct with the specified pointer and length.
        /// </summary>
        /// <param name="ptr">A pointer to the UTF-16 string.</param>
        /// <param name="length">The length of the UTF-16 string.</param>
        public UnsafeUTF16String(char* ptr, nint length)
        {
            Ptr = ptr;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeUTF16String"/> struct with the specified length.
        /// The contents of the string will be initialized to null characters.
        /// </summary>
        /// <param name="length">The length of the UTF-16 string.</param>
        public UnsafeUTF16String(nint length)
        {
            nint sizeInBytes = length * sizeof(char);
            Ptr = (char*)Marshal.AllocHGlobal(sizeInBytes);
            Length = length;
            ZeroMemory(Ptr, sizeInBytes);
        }

        /// <summary>
        /// Gets or sets the character at the specified index.
        /// </summary>
        /// <param name="index">The index of the character.</param>
        /// <returns>The character at the specified index.</returns>
        public char this[int index] { get => Ptr[index]; set => Ptr[index] = value; }

        /// <summary>
        /// Compares the UnsafeUTF16String with another UnsafeUTF16String.
        /// </summary>
        /// <param name="other">The UnsafeUTF16String to compare with.</param>
        /// <returns><c>true</c> if the strings are equal; otherwise, <c>false</c>.</returns>
        public bool Compare(UnsafeUTF16String* other)
        {
            if (Length != other->Length)
            {
                return false;
            }

            for (uint i = 0; i < Length; i++)
            {
                if (Ptr[i] != other->Ptr[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Resizes the UnsafeUTF16String to the specified length.
        /// </summary>
        /// <param name="newSize">The new length of the UnsafeUTF16String.</param>
        public void Resize(int newSize)
        {
            int sizeInBytes = newSize * sizeof(char);
            var newPtr = (char*)Marshal.AllocHGlobal(sizeInBytes);
            MemoryCopy(Ptr, newPtr, sizeInBytes, sizeInBytes);
            Marshal.FreeHGlobal((nint)Ptr);
            Ptr = newPtr;
        }

        /// <summary>
        /// Releases the memory associated with the UnsafeUTF16String.
        /// </summary>
        public void Release()
        {
            Utils.Free(Ptr);
        }

        /// <summary>
        /// Implicitly converts an UnsafeUTF16String to a string.
        /// </summary>
        /// <param name="ptr">The UnsafeUTF16String to convert.</param>
        public static implicit operator string(UnsafeUTF16String ptr)
        {
            return new(ptr.Ptr);
        }

        /// <summary>
        /// Implicitly converts a string to an UnsafeUTF16String.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        public static implicit operator UnsafeUTF16String(string str)
        {
            return new(str);
        }

        /// <summary>
        /// Returns a string that represents the UnsafeUTF16String.
        /// </summary>
        /// <returns>The string representation of the UnsafeUTF16String.</returns>
        public override string ToString()
        {
            return this;
        }
    }
}