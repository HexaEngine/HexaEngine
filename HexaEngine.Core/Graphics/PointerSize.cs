namespace HexaEngine.Core.Graphics
{
    using System.Globalization;

    /// <summary>
    /// Represents a size value that can be used with pointer arithmetic.
    /// </summary>
    public readonly struct PointerSize : IEquatable<PointerSize>, IFormattable
    {
        private readonly IntPtr _size;

        /// <summary>
        /// Gets a <see cref="PointerSize"/> with a value of 0.
        /// </summary>
        public static readonly PointerSize Zero = new(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerSize"/> struct with a specified <see cref="IntPtr"/> size.
        /// </summary>
        /// <param name="size">The size as an <see cref="IntPtr"/>.</param>
        public PointerSize(IntPtr size)
        {
            _size = size;
        }

        private unsafe PointerSize(void* size)
        {
            _size = new IntPtr(size);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerSize"/> struct with a specified <see cref="int"/> size.
        /// </summary>
        /// <param name="size">The size as an <see cref="int"/>.</param>
        public PointerSize(int size)
        {
            _size = new IntPtr(size);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerSize"/> struct with a specified <see cref="long"/> size.
        /// </summary>
        /// <param name="size">The size as a <see cref="long"/>.</param>
        public PointerSize(long size)
        {
            _size = new IntPtr(size);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <inheritdoc/>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(formatProvider ?? CultureInfo.CurrentCulture, string.IsNullOrEmpty(format) ? "{0}" : ("{0:" + format + "}"), _size);
        }

        /// <inheritdoc/>
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _size.GetHashCode();
        }

        /// <inheritdoc/>
        public bool Equals(PointerSize other)
        {
            return _size.Equals(other._size);
        }

        /// <inheritdoc/>
        public override bool Equals(object? value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is PointerSize)
            {
                PointerSize other = (PointerSize)value;
                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Adds two <see cref="PointerSize"/> values.
        /// </summary>
        public static PointerSize operator +(PointerSize left, PointerSize right)
        {
            return new PointerSize(left._size.ToInt64() + right._size.ToInt64());
        }

        /// <summary>
        /// Returns the current <see cref="PointerSize"/>.
        /// </summary>
        public static PointerSize operator +(PointerSize value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two <see cref="PointerSize"/> values.
        /// </summary>
        public static PointerSize operator -(PointerSize left, PointerSize right)
        {
            return new PointerSize(left._size.ToInt64() - right._size.ToInt64());
        }

        /// <summary>
        /// Negates a <see cref="PointerSize"/> value.
        /// </summary>
        public static PointerSize operator -(PointerSize value)
        {
            return new PointerSize(-value._size.ToInt64());
        }

        /// <summary>
        /// Multiplies a <see cref="PointerSize"/> value by an <see cref="int"/> scale.
        /// </summary>
        public static PointerSize operator *(int scale, PointerSize value)
        {
            return new PointerSize(scale * value._size.ToInt64());
        }

        /// <summary>
        /// Multiplies a <see cref="PointerSize"/> value by an <see cref="int"/> scale.
        /// </summary>
        public static PointerSize operator *(PointerSize value, int scale)
        {
            return new PointerSize(scale * value._size.ToInt64());
        }

        /// <summary>
        /// Divides a <see cref="PointerSize"/> value by an <see cref="int"/> scale.
        /// </summary>
        public static PointerSize operator /(PointerSize value, int scale)
        {
            return new PointerSize(value._size.ToInt64() / scale);
        }

        /// <summary>
        /// Checks if two <see cref="PointerSize"/> values are equal.
        /// </summary>
        public static bool operator ==(PointerSize left, PointerSize right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks if two <see cref="PointerSize"/> values are not equal.
        /// </summary>
        public static bool operator !=(PointerSize left, PointerSize right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Implicitly converts a <see cref="PointerSize"/> value to an <see cref="int"/>.
        /// </summary>
        public static implicit operator int(PointerSize value)
        {
            return value._size.ToInt32();
        }

        /// <summary>
        /// Implicitly converts a <see cref="PointerSize"/> value to a <see cref="long"/>.
        /// </summary>
        public static implicit operator long(PointerSize value)
        {
            return value._size.ToInt64();
        }

        /// <summary>
        /// Implicitly converts an <see cref="int"/> value to a <see cref="PointerSize"/>.
        /// </summary>
        public static implicit operator PointerSize(int value)
        {
            return new PointerSize(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="long"/> value to a <see cref="PointerSize"/>.
        /// </summary>
        public static implicit operator PointerSize(long value)
        {
            return new PointerSize(value);
        }

        /// <summary>
        /// Implicitly converts an <see cref="IntPtr"/> value to a <see cref="PointerSize"/>.
        /// </summary>
        public static implicit operator PointerSize(IntPtr value)
        {
            return new PointerSize(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="PointerSize"/> value to an <see cref="IntPtr"/>.
        /// </summary>
        public static implicit operator IntPtr(PointerSize value)
        {
            return value._size;
        }

        /// <summary>
        /// Implicitly converts a <see cref="PointerSize"/> value to an <see cref="nuint"/>.
        /// </summary>
        public static implicit operator nuint(PointerSize value)
        {
            return (nuint)value._size;
        }

        /// <summary>
        /// Implicitly converts a <see cref="void"/> value to a <see cref="PointerSize"/>.
        /// </summary>
        public static unsafe implicit operator PointerSize(void* value)
        {
            return new PointerSize(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="PointerSize"/> value to a <see cref="void"/>.
        /// </summary>
        public static unsafe implicit operator void*(PointerSize value)
        {
            return (void*)value._size;
        }
    }
}