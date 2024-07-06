namespace HexaEngine.Core.Unsafes
{
    using System;

    /// <summary>
    /// Represents an unsafe pointer to an array of pointers.
    /// </summary>
    /// <typeparam name="T">The type of the pointers.</typeparam>
    public unsafe struct PointerPointer<T> where T : unmanaged
    {
        /// <summary>
        /// The underlying data pointer.
        /// </summary>
        public T** Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerPointer{T}"/> struct.
        /// </summary>
        /// <param name="pointer">The pointer to the array of pointers.</param>
        public PointerPointer(T** pointer)
        {
            Data = pointer;
        }

        /// <summary>
        /// Implicitly converts a <see cref="PointerPointer{T}"/> to a <c>T**</c>.
        /// </summary>
        /// <param name="pointer">The <see cref="PointerPointer{T}"/> to convert.</param>
        /// <returns>The converted <c>T**</c>.</returns>
        public static implicit operator T**(PointerPointer<T> pointer) => pointer.Data;

        /// <summary>
        /// Implicitly converts a <c>T**</c> to a <see cref="PointerPointer{T}"/>.
        /// </summary>
        /// <param name="pointer">The <c>T**</c> to convert.</param>
        /// <returns>The converted <see cref="PointerPointer{T}"/>.</returns>
        public static implicit operator PointerPointer<T>(T** pointer) => new PointerPointer<T>(pointer);

        /// <summary>
        /// Implicitly converts an <see cref="IntPtr"/> to a <see cref="PointerPointer{T}"/>.
        /// </summary>
        /// <param name="pointer">The <see cref="IntPtr"/> to convert.</param>
        /// <returns>The converted <see cref="PointerPointer{T}"/>.</returns>
        public static implicit operator PointerPointer<T>(IntPtr pointer) => new PointerPointer<T>((T**)pointer);
    }
}