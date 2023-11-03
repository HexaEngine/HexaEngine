namespace HexaEngine.Core.Unsafes
{
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Represents an unsafe UTF-8 string.
    /// </summary>
    [Obsolete("Use StdString instead, will be removed next version")]
    public unsafe struct UnsafeString : IFreeable
    {
        private const int NullTerminatorSize = 1;

        /// <summary>
        /// A pointer to the UTF-8 string.
        /// </summary>
        public byte* Ptr;

        /// <summary>
        /// The length of the UTF-8 string.
        /// </summary>
        public nint Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeString"/> struct with the specified string.
        /// </summary>
        /// <param name="str">The string to initialize the UnsafeUTF8String with.</param>
        public UnsafeString(string str)
        {
            int stringLength = Encoding.UTF8.GetByteCount(str);
            int sizeInBytes = (stringLength + NullTerminatorSize) * sizeof(byte);
            Ptr = (byte*)Marshal.AllocHGlobal(sizeInBytes);
            fixed (char* strPtr = str)
            {
                Encoding.UTF8.GetBytes(strPtr, stringLength, Ptr, sizeInBytes);
            }
            Ptr[stringLength] = (byte)'\0';
            Length = stringLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeString"/> struct with the specified pointer and length.
        /// </summary>
        /// <param name="ptr">A pointer to the UTF-8 string.</param>
        /// <param name="length">The length of the UTF-8 string.</param>
        public UnsafeString(byte* ptr, nint length)
        {
            Ptr = ptr;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeString"/> struct with the specified length.
        /// The contents of the string will be initialized to null bytes.
        /// </summary>
        /// <param name="length">The length of the UTF-8 string.</param>
        public UnsafeString(nint length)
        {
            nint sizeInBytes = (length + NullTerminatorSize) * sizeof(byte);
            Ptr = (byte*)Marshal.AllocHGlobal(sizeInBytes);
            Length = length;
            ZeroMemory(Ptr, sizeInBytes);
        }

        /// <summary>
        /// Gets or sets the byte at the specified index.
        /// </summary>
        /// <param name="index">The index of the byte.</param>
        /// <returns>The byte at the specified index.</returns>
        public byte this[int index] { get => Ptr[index]; set => Ptr[index] = value; }

        /// <summary>
        /// Compares the UnsafeUTF8String with another UnsafeUTF8String.
        /// </summary>
        /// <param name="other">The UnsafeUTF8String to compare with.</param>
        /// <returns><c>true</c> if the strings are equal; otherwise, <c>false</c>.</returns>
        public bool Compare(UnsafeString* other)
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
        /// Resizes the UnsafeUTF8String to the specified length.
        /// </summary>
        /// <param name="newSize">The new length of the UnsafeUTF8String.</param>
        public void Resize(int newSize)
        {
            int sizeInBytes = newSize * sizeof(byte);
            var newPtr = (byte*)Marshal.AllocHGlobal(sizeInBytes);
            MemcpyT(Ptr, newPtr, sizeInBytes, sizeInBytes);
            Marshal.FreeHGlobal((nint)Ptr);
            Ptr = newPtr;
        }

        /// <summary>
        /// Releases the memory associated with the UnsafeUTF8String.
        /// </summary>
        public void Release()
        {
            Utils.Free(Ptr);
        }

        /// <summary>
        /// Implicitly converts an UnsafeUTF8String to a string.
        /// </summary>
        /// <param name="ptr">The UnsafeUTF8String to convert.</param>
        public static implicit operator string(UnsafeString ptr)
        {
            return Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr.Ptr));
        }

        /// <summary>
        /// Implicitly converts a string to an UnsafeUTF8String.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        public static implicit operator UnsafeString(string str)
        {
            return new(str);
        }

        /// <summary>
        /// Implicitly converts an UnsafeUTF8String to a pointer.
        /// </summary>
        /// <param name="str">The UnsafeUTF8String to convert.</param>
        public static implicit operator byte*(UnsafeString str) => str.Ptr;

        /// <summary>
        /// Returns a string that represents the UnsafeUTF8String.
        /// </summary>
        /// <returns>The string representation of the UnsafeUTF8String.</returns>
        public override string ToString()
        {
            return this;
        }
    }
}