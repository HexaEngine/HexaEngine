namespace HexaEngine.Core.IO
{
    using Hexa.NET.Mathematics;
    using System;
    using System.Buffers.Binary;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Provides extension methods for working with <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Reads a string from the specified <see cref="ReadOnlySpan{T}"/> using the given encoding and endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="encoding">The encoding to use for string decoding.</param>
        /// <param name="str">The output string containing the decoded value.</param>
        /// <param name="endianness">The endianness to use for reading the string length.</param>
        /// <returns>The total number of bytes read, including the string length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadString(this ReadOnlySpan<byte> src, Encoding encoding, out string str, Endianness endianness = Endianness.LittleEndian)
        {
            int len;
            if (endianness == Endianness.LittleEndian)
            {
                len = BinaryPrimitives.ReadInt32LittleEndian(src);
            }
            else
            {
                len = BinaryPrimitives.ReadInt32BigEndian(src);
            }
            str = encoding.GetString(src.Slice(4, len));
            return 4 + len;
        }

        /// <summary>
        /// Reads a string from the specified <see cref="Span{T}"/> using the given encoding and endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="encoding">The encoding to use for string decoding.</param>
        /// <param name="str">The output string containing the decoded value.</param>
        /// <param name="endianness">The endianness to use for reading the string length.</param>
        /// <returns>The total number of bytes read, including the string length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadString(this Span<byte> src, Encoding encoding, out string str, Endianness endianness = Endianness.LittleEndian)
        {
            int len;
            if (endianness == Endianness.LittleEndian)
            {
                len = BinaryPrimitives.ReadInt32LittleEndian(src);
            }
            else
            {
                len = BinaryPrimitives.ReadInt32BigEndian(src);
            }
            str = encoding.GetString(src.Slice(4, len));
            return 4 + len;
        }

        /// <summary>
        /// Writes a string to the specified <see cref="Span{T}"/> using the given encoding and endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> containing the string to write.</param>
        /// <param name="encoding">The encoding to use for string encoding.</param>
        /// <param name="endianness">The endianness to use for writing the string length.</param>
        /// <returns>The total number of bytes written, including the string length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteString(this Span<byte> dest, ReadOnlySpan<char> src, Encoding encoding, Endianness endianness = Endianness.LittleEndian)
        {
            var len = encoding.GetByteCount(src);
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest, len);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(dest, len);
            }
            encoding.GetBytes(src, dest[4..]);
            return 4 + len;
        }

        /// <summary>
        /// Gets the size, in bytes, of a string when encoded with the specified encoder.
        /// </summary>
        /// <param name="str">The string to calculate the size of.</param>
        /// <param name="encoder">The encoder to use for size calculation.</param>
        /// <returns>The size of the string when encoded with the given encoder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SizeOf(this string str, Encoder encoder)
        {
            return 4 + encoder.GetByteCount(str, true);
        }

        /// <summary>
        /// Writes a 16-bit signed integer to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 16-bit signed integer to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 2).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteInt16(this Span<byte> dest, short value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt16LittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteInt16BigEndian(dest, value);
            }
            return 2;
        }

        /// <summary>
        /// Writes a 16-bit unsigned integer to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 16-bit unsigned integer to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 2).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteUInt16(this Span<byte> dest, ushort value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteUInt16BigEndian(dest, value);
            }
            return 2;
        }

        /// <summary>
        /// Writes a 32-bit signed integer to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 32-bit signed integer to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteInt32(this Span<byte> dest, int value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(dest, value);
            }
            return 4;
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 32-bit unsigned integer to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteUInt32(this Span<byte> dest, uint value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteUInt32BigEndian(dest, value);
            }
            return 4;
        }

        /// <summary>
        /// Writes a 64-bit signed integer to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 64-bit signed integer to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteInt64(this Span<byte> dest, long value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt64LittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteInt64BigEndian(dest, value);
            }
            return 8;
        }

        /// <summary>
        /// Writes a 64-bit unsigned integer to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 64-bit unsigned integer to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteUInt64(this Span<byte> dest, ulong value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteUInt64BigEndian(dest, value);
            }
            return 8;
        }

        /// <summary>
        /// Writes a single-precision floating-point value to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The single-precision floating-point value to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteSingle(this Span<byte> dest, float value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dest, value);
            }
            return 4;
        }

        /// <summary>
        /// Writes a double-precision floating-point value to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The double-precision floating-point value to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteDouble(this Span<byte> dest, double value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteDoubleLittleEndian(dest, value);
            }
            else
            {
                BinaryPrimitives.WriteDoubleBigEndian(dest, value);
            }
            return 8;
        }

        /// <summary>
        /// Writes a 2D vector to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 2D vector to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteVector2(this Span<byte> dest, Vector2 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dest, value.X);
                BinaryPrimitives.WriteSingleLittleEndian(dest[4..], value.Y);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dest, value.X);
                BinaryPrimitives.WriteSingleBigEndian(dest[4..], value.Y);
            }
            return 8;
        }

        /// <summary>
        /// Writes a 3D vector to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 3D vector to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 12).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteVector3(this Span<byte> dest, Vector3 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dest, value.X);
                BinaryPrimitives.WriteSingleLittleEndian(dest[4..], value.Y);
                BinaryPrimitives.WriteSingleLittleEndian(dest[8..], value.Z);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dest, value.X);
                BinaryPrimitives.WriteSingleBigEndian(dest[4..], value.Y);
                BinaryPrimitives.WriteSingleBigEndian(dest[8..], value.Z);
            }
            return 12;
        }

        /// <summary>
        /// Writes a 4D vector to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="value">The 4D vector to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 16).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteVector4(this Span<byte> dest, Vector4 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dest, value.X);
                BinaryPrimitives.WriteSingleLittleEndian(dest[4..], value.Y);
                BinaryPrimitives.WriteSingleLittleEndian(dest[8..], value.Z);
                BinaryPrimitives.WriteSingleLittleEndian(dest[12..], value.W);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dest, value.X);
                BinaryPrimitives.WriteSingleBigEndian(dest[4..], value.Y);
                BinaryPrimitives.WriteSingleBigEndian(dest[8..], value.Z);
                BinaryPrimitives.WriteSingleBigEndian(dest[12..], value.W);
            }
            return 16;
        }

        /// <summary>
        /// Writes a 4x4 matrix to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="matrix">The 4x4 matrix to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 64).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteMatrix4x4(this Span<byte> dest, Matrix4x4 matrix, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dest[0..], matrix.M11);
                BinaryPrimitives.WriteSingleLittleEndian(dest[4..], matrix.M12);
                BinaryPrimitives.WriteSingleLittleEndian(dest[8..], matrix.M13);
                BinaryPrimitives.WriteSingleLittleEndian(dest[12..], matrix.M14);
                BinaryPrimitives.WriteSingleLittleEndian(dest[16..], matrix.M21);
                BinaryPrimitives.WriteSingleLittleEndian(dest[20..], matrix.M22);
                BinaryPrimitives.WriteSingleLittleEndian(dest[24..], matrix.M23);
                BinaryPrimitives.WriteSingleLittleEndian(dest[28..], matrix.M24);
                BinaryPrimitives.WriteSingleLittleEndian(dest[32..], matrix.M31);
                BinaryPrimitives.WriteSingleLittleEndian(dest[36..], matrix.M32);
                BinaryPrimitives.WriteSingleLittleEndian(dest[40..], matrix.M33);
                BinaryPrimitives.WriteSingleLittleEndian(dest[44..], matrix.M34);
                BinaryPrimitives.WriteSingleLittleEndian(dest[48..], matrix.M41);
                BinaryPrimitives.WriteSingleLittleEndian(dest[52..], matrix.M42);
                BinaryPrimitives.WriteSingleLittleEndian(dest[56..], matrix.M43);
                BinaryPrimitives.WriteSingleLittleEndian(dest[60..], matrix.M44);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dest[0..], matrix.M11);
                BinaryPrimitives.WriteSingleBigEndian(dest[4..], matrix.M12);
                BinaryPrimitives.WriteSingleBigEndian(dest[8..], matrix.M13);
                BinaryPrimitives.WriteSingleBigEndian(dest[12..], matrix.M14);
                BinaryPrimitives.WriteSingleBigEndian(dest[16..], matrix.M21);
                BinaryPrimitives.WriteSingleBigEndian(dest[20..], matrix.M22);
                BinaryPrimitives.WriteSingleBigEndian(dest[24..], matrix.M23);
                BinaryPrimitives.WriteSingleBigEndian(dest[28..], matrix.M24);
                BinaryPrimitives.WriteSingleBigEndian(dest[32..], matrix.M31);
                BinaryPrimitives.WriteSingleBigEndian(dest[36..], matrix.M32);
                BinaryPrimitives.WriteSingleBigEndian(dest[40..], matrix.M33);
                BinaryPrimitives.WriteSingleBigEndian(dest[44..], matrix.M34);
                BinaryPrimitives.WriteSingleBigEndian(dest[48..], matrix.M41);
                BinaryPrimitives.WriteSingleBigEndian(dest[52..], matrix.M42);
                BinaryPrimitives.WriteSingleBigEndian(dest[56..], matrix.M43);
                BinaryPrimitives.WriteSingleBigEndian(dest[60..], matrix.M44);
            }

            return 64;
        }

        /// <summary>
        /// Writes a quaternion to the specified <see cref="Span{T}"/> using the specified endianness.
        /// </summary>
        /// <param name="dest">The destination <see cref="Span{T}"/> to write to.</param>
        /// <param name="quaternion">The quaternion to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        /// <returns>The size of the data written in bytes (always 16).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int WriteQuaternion(this Span<byte> dest, Quaternion quaternion, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dest, quaternion.X);
                BinaryPrimitives.WriteSingleLittleEndian(dest[4..], quaternion.Y);
                BinaryPrimitives.WriteSingleLittleEndian(dest[8..], quaternion.Z);
                BinaryPrimitives.WriteSingleLittleEndian(dest[12..], quaternion.W);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dest, quaternion.X);
                BinaryPrimitives.WriteSingleBigEndian(dest[4..], quaternion.Y);
                BinaryPrimitives.WriteSingleBigEndian(dest[8..], quaternion.Z);
                BinaryPrimitives.WriteSingleBigEndian(dest[12..], quaternion.W);
            }

            return 16;
        }

        /// <summary>
        /// Reads a 16-bit signed integer from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 16-bit signed integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 2).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadInt16(this ReadOnlySpan<byte> src, out short value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadInt16LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadInt16BigEndian(src);
            }
            return 2;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 16-bit unsigned integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 2).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadUInt16(this ReadOnlySpan<byte> src, out ushort value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadUInt16LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadUInt16BigEndian(src);
            }
            return 2;
        }

        /// <summary>
        /// Reads a 32-bit signed integer from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 32-bit signed integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadInt32(this ReadOnlySpan<byte> src, out int value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadInt32LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadInt32BigEndian(src);
            }
            return 4;
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 32-bit unsigned integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadUInt32(this ReadOnlySpan<byte> src, out uint value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadUInt32LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadUInt32BigEndian(src);
            }
            return 4;
        }

        /// <summary>
        /// Reads a 64-bit signed integer from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 64-bit signed integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadInt64(this ReadOnlySpan<byte> src, out long value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadInt64LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadInt64BigEndian(src);
            }
            return 8;
        }

        /// <summary>
        /// Reads a 64-bit unsigned integer from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 64-bit unsigned integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadUInt64(this ReadOnlySpan<byte> src, out ulong value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadUInt64LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadUInt64BigEndian(src);
            }
            return 8;
        }

        /// <summary>
        /// Reads a single-precision floating-point value from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output single-precision floating-point value.</param>
        /// <param name="endianness">The endianness to use for reading the floating-point value.</param>
        /// <returns>The total number of bytes read (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadSingle(this ReadOnlySpan<byte> src, out float value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadSingleLittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadSingleBigEndian(src);
            }
            return 4;
        }

        /// <summary>
        /// Reads a double-precision floating-point value from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output double-precision floating-point value.</param>
        /// <param name="endianness">The endianness to use for reading the floating-point value.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadDouble(this ReadOnlySpan<byte> src, out double value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadDoubleLittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadDoubleBigEndian(src);
            }
            return 8;
        }

        /// <summary>
        /// Reads a 2D vector from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 2D vector value.</param>
        /// <param name="endianness">The endianness to use for reading the vector.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        public static int ReadVector2(this ReadOnlySpan<byte> src, out Vector2 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                value.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
            }
            else
            {
                value.X = BinaryPrimitives.ReadSingleBigEndian(src);
                value.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
            }
            return 8;
        }

        /// <summary>
        /// Reads a 3D vector from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 3D vector value.</param>
        /// <param name="endianness">The endianness to use for reading the vector.</param>
        /// <returns>The total number of bytes read (always 12).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadVector3(this ReadOnlySpan<byte> src, out Vector3 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                value.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
            }
            else
            {
                value.X = BinaryPrimitives.ReadSingleBigEndian(src);
                value.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
            }
            return 12;
        }

        /// <summary>
        /// Reads a 4D vector from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="value">The output 4D vector value.</param>
        /// <param name="endianness">The endianness to use for reading the vector.</param>
        /// <returns>The total number of bytes read (always 16).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadVector4(this ReadOnlySpan<byte> src, out Vector4 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                value.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
                value.W = BinaryPrimitives.ReadSingleLittleEndian(src[12..]);
            }
            else
            {
                value.X = BinaryPrimitives.ReadSingleBigEndian(src);
                value.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
                value.W = BinaryPrimitives.ReadSingleBigEndian(src[12..]);
            }
            return 16;
        }

        /// <summary>
        /// Reads a 4x4 matrix from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="matrix">The output 4x4 matrix value.</param>
        /// <param name="endianness">The endianness to use for reading the matrix.</param>
        /// <returns>The total number of bytes read (always 64).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadMatrix4x4(this ReadOnlySpan<byte> src, out Matrix4x4 matrix, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                matrix.M11 = BinaryPrimitives.ReadSingleLittleEndian(src[0..]);
                matrix.M12 = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                matrix.M13 = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
                matrix.M14 = BinaryPrimitives.ReadSingleLittleEndian(src[12..]);
                matrix.M21 = BinaryPrimitives.ReadSingleLittleEndian(src[16..]);
                matrix.M22 = BinaryPrimitives.ReadSingleLittleEndian(src[20..]);
                matrix.M23 = BinaryPrimitives.ReadSingleLittleEndian(src[24..]);
                matrix.M24 = BinaryPrimitives.ReadSingleLittleEndian(src[28..]);
                matrix.M31 = BinaryPrimitives.ReadSingleLittleEndian(src[32..]);
                matrix.M32 = BinaryPrimitives.ReadSingleLittleEndian(src[36..]);
                matrix.M33 = BinaryPrimitives.ReadSingleLittleEndian(src[40..]);
                matrix.M34 = BinaryPrimitives.ReadSingleLittleEndian(src[44..]);
                matrix.M41 = BinaryPrimitives.ReadSingleLittleEndian(src[48..]);
                matrix.M42 = BinaryPrimitives.ReadSingleLittleEndian(src[52..]);
                matrix.M43 = BinaryPrimitives.ReadSingleLittleEndian(src[56..]);
                matrix.M44 = BinaryPrimitives.ReadSingleLittleEndian(src[60..]);
            }
            else
            {
                matrix.M11 = BinaryPrimitives.ReadSingleBigEndian(src[0..]);
                matrix.M12 = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                matrix.M13 = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
                matrix.M14 = BinaryPrimitives.ReadSingleBigEndian(src[12..]);
                matrix.M21 = BinaryPrimitives.ReadSingleBigEndian(src[16..]);
                matrix.M22 = BinaryPrimitives.ReadSingleBigEndian(src[20..]);
                matrix.M23 = BinaryPrimitives.ReadSingleBigEndian(src[24..]);
                matrix.M24 = BinaryPrimitives.ReadSingleBigEndian(src[28..]);
                matrix.M31 = BinaryPrimitives.ReadSingleBigEndian(src[32..]);
                matrix.M32 = BinaryPrimitives.ReadSingleBigEndian(src[36..]);
                matrix.M33 = BinaryPrimitives.ReadSingleBigEndian(src[40..]);
                matrix.M34 = BinaryPrimitives.ReadSingleBigEndian(src[44..]);
                matrix.M41 = BinaryPrimitives.ReadSingleBigEndian(src[48..]);
                matrix.M42 = BinaryPrimitives.ReadSingleBigEndian(src[52..]);
                matrix.M43 = BinaryPrimitives.ReadSingleBigEndian(src[56..]);
                matrix.M44 = BinaryPrimitives.ReadSingleBigEndian(src[60..]);
            }

            return 64;
        }

        /// <summary>
        /// Reads a quaternion from the specified <see cref="ReadOnlySpan{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="ReadOnlySpan{T}"/> to read from.</param>
        /// <param name="quaternion">The output quaternion value.</param>
        /// <param name="endianness">The endianness to use for reading the quaternion.</param>
        /// <returns>The total number of bytes read (always 16).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadQuaternion(this ReadOnlySpan<byte> src, out Quaternion quaternion, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                quaternion.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                quaternion.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                quaternion.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
                quaternion.W = BinaryPrimitives.ReadSingleLittleEndian(src[12..]);
            }
            else
            {
                quaternion.X = BinaryPrimitives.ReadSingleBigEndian(src);
                quaternion.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                quaternion.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
                quaternion.W = BinaryPrimitives.ReadSingleBigEndian(src[12..]);
            }

            return 16;
        }

        /// <summary>
        /// Reads a 16-bit signed integer from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 16-bit signed integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 2).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadInt16(this Span<byte> src, out short value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadInt16LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadInt16BigEndian(src);
            }
            return 2;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 16-bit unsigned integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 2).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadUInt16(this Span<byte> src, out ushort value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadUInt16LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadUInt16BigEndian(src);
            }
            return 2;
        }

        /// <summary>
        /// Reads a 32-bit signed integer from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 32-bit signed integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadInt32(this Span<byte> src, out int value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadInt32LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadInt32BigEndian(src);
            }
            return 4;
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 32-bit unsigned integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadUInt32(this Span<byte> src, out uint value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadUInt32LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadUInt32BigEndian(src);
            }
            return 4;
        }

        /// <summary>
        /// Reads a 64-bit signed integer from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 64-bit signed integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadInt64(this Span<byte> src, out long value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadInt64LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadInt64BigEndian(src);
            }
            return 8;
        }

        /// <summary>
        /// Reads a 64-bit unsigned integer from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 64-bit unsigned integer value.</param>
        /// <param name="endianness">The endianness to use for reading the integer.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadUInt64(this Span<byte> src, out ulong value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadUInt64LittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadUInt64BigEndian(src);
            }
            return 8;
        }

        /// <summary>
        /// Reads a single-precision floating-point value from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output single-precision floating-point value.</param>
        /// <param name="endianness">The endianness to use for reading the floating-point value.</param>
        /// <returns>The total number of bytes read (always 4).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadSingle(this Span<byte> src, out float value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadSingleLittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadSingleBigEndian(src);
            }
            return 4;
        }

        /// <summary>
        /// Reads a double-precision floating-point value from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output double-precision floating-point value.</param>
        /// <param name="endianness">The endianness to use for reading the floating-point value.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadDouble(this Span<byte> src, out double value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value = BinaryPrimitives.ReadDoubleLittleEndian(src);
            }
            else
            {
                value = BinaryPrimitives.ReadDoubleBigEndian(src);
            }
            return 8;
        }

        /// <summary>
        /// Reads a 2D vector from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 2D vector value.</param>
        /// <param name="endianness">The endianness to use for reading the vector.</param>
        /// <returns>The total number of bytes read (always 8).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadVector2(this Span<byte> src, out Vector2 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                value.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
            }
            else
            {
                value.X = BinaryPrimitives.ReadSingleBigEndian(src);
                value.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
            }
            return 8;
        }

        /// <summary>
        /// Reads a 3D vector from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 3D vector value.</param>
        /// <param name="endianness">The endianness to use for reading the vector.</param>
        /// <returns>The total number of bytes read (always 12).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadVector3(this Span<byte> src, out Vector3 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                value.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
            }
            else
            {
                value.X = BinaryPrimitives.ReadSingleBigEndian(src);
                value.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
            }
            return 12;
        }

        /// <summary>
        /// Reads a 4D vector from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="value">The output 4D vector value.</param>
        /// <param name="endianness">The endianness to use for reading the vector.</param>
        /// <returns>The total number of bytes read (always 16).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadVector4(this Span<byte> src, out Vector4 value, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                value.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                value.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
                value.W = BinaryPrimitives.ReadSingleLittleEndian(src[12..]);
            }
            else
            {
                value.X = BinaryPrimitives.ReadSingleBigEndian(src);
                value.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                value.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
                value.W = BinaryPrimitives.ReadSingleBigEndian(src[12..]);
            }
            return 16;
        }

        /// <summary>
        /// Reads a 4x4 matrix from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="matrix">The output 4x4 matrix value.</param>
        /// <param name="endianness">The endianness to use for reading the matrix.</param>
        /// <returns>The total number of bytes read (always 64).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadMatrix4x4(this Span<byte> src, out Matrix4x4 matrix, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                matrix.M11 = BinaryPrimitives.ReadSingleLittleEndian(src[0..]);
                matrix.M12 = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                matrix.M13 = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
                matrix.M14 = BinaryPrimitives.ReadSingleLittleEndian(src[12..]);
                matrix.M21 = BinaryPrimitives.ReadSingleLittleEndian(src[16..]);
                matrix.M22 = BinaryPrimitives.ReadSingleLittleEndian(src[20..]);
                matrix.M23 = BinaryPrimitives.ReadSingleLittleEndian(src[24..]);
                matrix.M24 = BinaryPrimitives.ReadSingleLittleEndian(src[28..]);
                matrix.M31 = BinaryPrimitives.ReadSingleLittleEndian(src[32..]);
                matrix.M32 = BinaryPrimitives.ReadSingleLittleEndian(src[36..]);
                matrix.M33 = BinaryPrimitives.ReadSingleLittleEndian(src[40..]);
                matrix.M34 = BinaryPrimitives.ReadSingleLittleEndian(src[44..]);
                matrix.M41 = BinaryPrimitives.ReadSingleLittleEndian(src[48..]);
                matrix.M42 = BinaryPrimitives.ReadSingleLittleEndian(src[52..]);
                matrix.M43 = BinaryPrimitives.ReadSingleLittleEndian(src[56..]);
                matrix.M44 = BinaryPrimitives.ReadSingleLittleEndian(src[60..]);
            }
            else
            {
                matrix.M11 = BinaryPrimitives.ReadSingleBigEndian(src[0..]);
                matrix.M12 = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                matrix.M13 = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
                matrix.M14 = BinaryPrimitives.ReadSingleBigEndian(src[12..]);
                matrix.M21 = BinaryPrimitives.ReadSingleBigEndian(src[16..]);
                matrix.M22 = BinaryPrimitives.ReadSingleBigEndian(src[20..]);
                matrix.M23 = BinaryPrimitives.ReadSingleBigEndian(src[24..]);
                matrix.M24 = BinaryPrimitives.ReadSingleBigEndian(src[28..]);
                matrix.M31 = BinaryPrimitives.ReadSingleBigEndian(src[32..]);
                matrix.M32 = BinaryPrimitives.ReadSingleBigEndian(src[36..]);
                matrix.M33 = BinaryPrimitives.ReadSingleBigEndian(src[40..]);
                matrix.M34 = BinaryPrimitives.ReadSingleBigEndian(src[44..]);
                matrix.M41 = BinaryPrimitives.ReadSingleBigEndian(src[48..]);
                matrix.M42 = BinaryPrimitives.ReadSingleBigEndian(src[52..]);
                matrix.M43 = BinaryPrimitives.ReadSingleBigEndian(src[56..]);
                matrix.M44 = BinaryPrimitives.ReadSingleBigEndian(src[60..]);
            }

            return 64;
        }

        /// <summary>
        /// Reads a quaternion from the specified <see cref="Span{T}"/> using the given endianness.
        /// </summary>
        /// <param name="src">The source <see cref="Span{T}"/> to read from.</param>
        /// <param name="quaternion">The output quaternion value.</param>
        /// <param name="endianness">The endianness to use for reading the quaternion.</param>
        /// <returns>The total number of bytes read (always 16).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadQuaternion(this Span<byte> src, out Quaternion quaternion, Endianness endianness = Endianness.LittleEndian)
        {
            if (endianness == Endianness.LittleEndian)
            {
                quaternion.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                quaternion.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                quaternion.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
                quaternion.W = BinaryPrimitives.ReadSingleLittleEndian(src[12..]);
            }
            else
            {
                quaternion.X = BinaryPrimitives.ReadSingleBigEndian(src);
                quaternion.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                quaternion.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
                quaternion.W = BinaryPrimitives.ReadSingleBigEndian(src[12..]);
            }

            return 16;
        }
    }
}