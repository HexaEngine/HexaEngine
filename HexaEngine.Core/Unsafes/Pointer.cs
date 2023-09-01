namespace HexaEngine.Core.Unsafes
{
    using System;

    /// <summary>
    /// Represents an unsafe pointer to a block of memory.
    /// </summary>
    public unsafe struct Pointer : IEquatable<Pointer>
    {
        /// <summary>
        /// The underlying data pointer.
        /// </summary>
        public void* Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pointer"/> struct.
        /// </summary>
        /// <param name="pointer">The void pointer.</param>
        public Pointer(void* pointer)
        {
            Data = pointer;
        }

        /// <summary>
        /// Casts the <see cref="Pointer"/> to a <see cref="Pointer{T}"/> of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <returns>A new <see cref="Pointer{T}"/> instance.</returns>
        public Pointer<T> Cast<T>() where T : unmanaged
        {
            return new Pointer<T>((T*)Data);
        }

        /// <summary>
        /// Determines whether the current <see cref="Pointer"/> is equal to another <see cref="Pointer"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Pointer"/> to compare with the current object.</param>
        /// <returns><c>true</c> if the current <see cref="Pointer"/> is equal to the other <see cref="Pointer"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            return obj is Pointer pointer && Equals(pointer);
        }

        /// <summary>
        /// Determines whether the current <see cref="Pointer"/> is equal to another <see cref="Pointer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Pointer"/> to compare with the current object.</param>
        /// <returns><c>true</c> if the current <see cref="Pointer"/> is equal to the other <see cref="Pointer"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Pointer other)
        {
            return Data == other.Data;
        }

        /// <summary>
        /// Determines whether two <see cref="Pointer"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Pointer"/> to compare.</param>
        /// <param name="right">The second <see cref="Pointer"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Pointer"/> objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Pointer left, Pointer right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Pointer"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Pointer"/> to compare.</param>
        /// <param name="right">The second <see cref="Pointer"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Pointer"/> objects are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Pointer left, Pointer right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Pointer"/> to a <c>void*</c>.
        /// </summary>
        /// <param name="pointer">The <see cref="Pointer"/> to convert.</param>
        /// <returns>The converted <c>void*</c>.</returns>
        public static implicit operator void*(Pointer pointer) => pointer.Data;

        /// <summary>
        /// Implicitly converts a <c>void*</c> to a <see cref="Pointer"/>.
        /// </summary>
        /// <param name="pointer">The <c>void*</c> to convert.</param>
        /// <returns>The converted <see cref="Pointer"/>.</returns>
        public static implicit operator Pointer(void* pointer) => new Pointer(pointer);

        /// <summary>
        /// Implicitly converts a <see cref="Pointer"/> to an <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="pointer">The <see cref="Pointer"/> to convert.</param>
        /// <returns>The converted <see cref="IntPtr"/>.</returns>
        public static implicit operator IntPtr(Pointer pointer) => (IntPtr)pointer.Data;

        /// <summary>
        /// Implicitly converts an <see cref="IntPtr"/> to a <see cref="Pointer"/>.
        /// </summary>
        /// <param name="pointer">The <see cref="IntPtr"/> to convert.</param>
        /// <returns>The converted <see cref="Pointer"/>.</returns>
        public static implicit operator Pointer(IntPtr pointer) => new Pointer((void*)pointer);

        /// <summary>
        /// Returns the hash code for the <see cref="Pointer"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="Pointer"/>.</returns>
        public override int GetHashCode()
        {
            return ((IntPtr)Data).GetHashCode();
        }
    }

    /// <summary>
    /// Represents an unsafe pointer to a block of memory of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the memory block.</typeparam>
    public unsafe struct Pointer<T> : IEquatable<Pointer<T>> where T : unmanaged
    {
        /// <summary>
        /// The underlying data pointer.
        /// </summary>
        public T* Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pointer{T}"/> struct.
        /// </summary>
        public Pointer()
        {
            Data = AllocT<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pointer{T}"/> struct.
        /// </summary>
        /// <param name="pointer">The typed pointer.</param>
        public Pointer(T* pointer)
        {
            Data = pointer;
        }

        /// <summary>
        /// Gets a _value indicating whether the <see cref="Pointer{T}"/> is null.
        /// </summary>
        public bool IsNull => Data == null;

        /// <summary>
        /// Frees the memory allocated by the <see cref="Pointer{T}"/>.
        /// </summary>
        public void Free()
        {
            Utils.Free(Data);
            Data = null;
        }

        /// <summary>
        /// Casts the <see cref="Pointer{T}"/> to a <see cref="Pointer"/>.
        /// </summary>
        /// <returns>A new <see cref="Pointer"/> instance.</returns>
        public Pointer Cast()
        {
            return new Pointer(Data);
        }

        /// <summary>
        /// Casts the <see cref="Pointer{T}"/> to a <see cref="Pointer{CastTo}"/> of the specified type.
        /// </summary>
        /// <typeparam name="CastTo">The type to cast to.</typeparam>
        /// <returns>A new <see cref="Pointer{CastTo}"/> instance.</returns>
        public Pointer<CastTo> Cast<CastTo>() where CastTo : unmanaged
        {
            return new Pointer<CastTo>((CastTo*)Data);
        }

        /// <summary>
        /// Determines whether the current <see cref="Pointer{T}"/> is equal to another <see cref="Pointer{T}"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Pointer{T}"/> to compare with the current object.</param>
        /// <returns><c>true</c> if the current <see cref="Pointer{T}"/> is equal to the other <see cref="Pointer{T}"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            return obj is Pointer<T> pointer && Equals(pointer);
        }

        /// <summary>
        /// Determines whether the current <see cref="Pointer{T}"/> is equal to another <see cref="Pointer{T}"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Pointer{T}"/> to compare with the current object.</param>
        /// <returns><c>true</c> if the current <see cref="Pointer{T}"/> is equal to the other <see cref="Pointer{T}"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Pointer<T> other)
        {
            return Data == other.Data;
        }

        /// <summary>
        /// Determines whether two <see cref="Pointer{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Pointer{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="Pointer{T}"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Pointer{T}"/> objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Pointer<T> left, Pointer<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Pointer{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Pointer{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="Pointer{T}"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Pointer{T}"/> objects are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Pointer<T> left, Pointer<T> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Pointer{T}"/> to a <c>T*</c>.
        /// </summary>
        /// <param name="pointer">The <see cref="Pointer{T}"/> to convert.</param>
        /// <returns>The converted <c>T*</c>.</returns>
        public static implicit operator T*(Pointer<T> pointer) => pointer.Data;

        /// <summary>
        /// Implicitly converts a <c>T*</c> to a <see cref="Pointer{T}"/>.
        /// </summary>
        /// <param name="pointer">The <c>T*</c> to convert.</param>
        /// <returns>The converted <see cref="Pointer{T}"/>.</returns>
        public static implicit operator Pointer<T>(T* pointer) => new Pointer<T>(pointer);

        /// <summary>
        /// Implicitly converts an <see cref="IntPtr"/> to a <see cref="Pointer{T}"/>.
        /// </summary>
        /// <param name="pointer">The <see cref="IntPtr"/> to convert.</param>
        /// <returns>The converted <see cref="Pointer{T}"/>.</returns>
        public static implicit operator Pointer<T>(IntPtr pointer) => new Pointer<T>((T*)pointer);

        /// <summary>
        /// Implicitly converts a <see cref="Pointer"/> to a <see cref="Pointer{T}"/>.
        /// </summary>
        /// <param name="pointer">The <see cref="Pointer"/> to convert.</param>
        /// <returns>The converted <see cref="Pointer{T}"/>.</returns>
        public static implicit operator Pointer<T>(Pointer pointer) => new Pointer<T>((T*)pointer.Data);

        /// <summary>
        /// Implicitly converts a <see cref="Pointer{T}"/> to a <see cref="Pointer"/>.
        /// </summary>
        /// <param name="pointer">The <see cref="Pointer{T}"/> to convert.</param>
        /// <returns>The converted <see cref="Pointer"/>.</returns>
        public static implicit operator Pointer(Pointer<T> pointer) => pointer.Data;

        /// <summary>
        /// Returns the hash code for the <see cref="Pointer{T}"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="Pointer{T}"/>.</returns>
        public override int GetHashCode()
        {
            return ((IntPtr)Data).GetHashCode();
        }
    }

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