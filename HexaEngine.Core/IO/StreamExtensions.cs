namespace HexaEngine.Core.IO
{
    using HexaEngine.Mathematics;
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.IO;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Provides extension methods for the <see cref="Stream"/> class.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Writes a string to the stream using the specified encoder and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="str">The string to write to the stream.</param>
        /// <param name="encoder">The encoding to use for the string.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteString(this Stream stream, string? str, Encoding encoder, Endianness endianness)
        {
            if (str == null)
            {
                stream.WriteInt32(0, endianness);
                return;
            }
            var count = encoder.GetByteCount(str);
            var bytes = count + 4;
            Span<byte> dst = bytes < 2048 ? stackalloc byte[bytes] : new byte[bytes];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dst, count);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(dst, count);
            }

            encoder.GetBytes(str, dst[4..]);
            stream.Write(dst);
        }

        /// <summary>
        /// Reads a string from the stream using the specified decoder and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="decoder">The encoding to use for reading the string.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The string read from the stream, or null if the string length is 0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string? ReadString(this Stream stream, Encoding decoder, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[4];
            stream.Read(buf);
            int len = 0;
            if (endianness == Endianness.LittleEndian)
            {
                len = BinaryPrimitives.ReadInt32LittleEndian(buf);
            }
            else
            {
                len = BinaryPrimitives.ReadInt32BigEndian(buf);
            }

            if (len == 0)
            {
                return null;
            }

            Span<byte> src = len < 2048 ? stackalloc byte[len] : new byte[len];
            stream.Read(src);

            int charCount = decoder.GetCharCount(src);
            Span<char> chars = charCount < 2048 ? stackalloc char[charCount] : new char[charCount];
            decoder.GetChars(src, chars);
            return new(chars);
        }

        /// <summary>
        /// Reads a GUID from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The GUID read from the stream.</returns>
        public static Guid ReadGuid(this Stream stream, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[16];
            stream.Read(buf);
            Guid guid = new(buf, endianness == Endianness.BigEndian);
            return guid;
        }

        /// <summary>
        /// Writes a GUID to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="guid">The value to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        public static void WriteGuid(this Stream stream, Guid guid, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[16];
            guid.TryWriteBytes(buf, endianness == Endianness.BigEndian, out _);
            stream.Write(buf);
        }

        /// <summary>
        /// Writes a 16-bit signed integer to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="val">The value to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteInt16(this Stream stream, short val, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[2];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt16LittleEndian(buf, val);
            }
            else
            {
                BinaryPrimitives.WriteInt16BigEndian(buf, val);
            }

            stream.Write(buf);
        }

        /// <summary>
        /// Reads a 16-bit signed integer from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The 16-bit signed integer read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static short ReadInt16(this Stream stream, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[2];
            stream.Read(buf);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadInt16LittleEndian(buf);
            }
            else
            {
                return BinaryPrimitives.ReadInt16BigEndian(buf);
            }
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The 16-bit unsigned integer read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ushort ReadUInt16(this Stream stream, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[2];
            stream.Read(buf);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadUInt16LittleEndian(buf);
            }
            else
            {
                return BinaryPrimitives.ReadUInt16BigEndian(buf);
            }
        }

        /// <summary>
        /// Writes a 16-bit unsigned integer to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="val">The value to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteUInt16(this Stream stream, ushort val, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[2];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(buf, val);
            }
            else
            {
                BinaryPrimitives.WriteUInt16BigEndian(buf, val);
            }

            stream.Write(buf);
        }

        /// <summary>
        /// Writes a 32-bit signed integer to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="val">The value to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteInt32(this Stream stream, int val, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[4];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(buf, val);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(buf, val);
            }

            stream.Write(buf);
        }

        /// <summary>
        /// Reads a 32-bit signed integer from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The 32-bit signed integer read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ReadInt32(this Stream stream, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[4];
            stream.Read(buf);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadInt32LittleEndian(buf);
            }
            else
            {
                return BinaryPrimitives.ReadInt32BigEndian(buf);
            }
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The 32-bit unsigned integer read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint ReadUInt32(this Stream stream, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[4];
            stream.Read(buf);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadUInt32LittleEndian(buf);
            }
            else
            {
                return BinaryPrimitives.ReadUInt32BigEndian(buf);
            }
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="val">The value to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteUInt32(this Stream stream, uint val, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[4];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(buf, val);
            }
            else
            {
                BinaryPrimitives.WriteUInt32BigEndian(buf, val);
            }

            stream.Write(buf);
        }

        /// <summary>
        /// Writes a 64-bit unsigned integer to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="val">The value to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteUInt64(this Stream stream, ulong val, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[8];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(buf, val);
            }
            else
            {
                BinaryPrimitives.WriteUInt64BigEndian(buf, val);
            }

            stream.Write(buf);
        }

        /// <summary>
        /// Writes a 64-bit signed integer to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="val">The value to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteInt64(this Stream stream, long val, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[8];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt64LittleEndian(buf, val);
            }
            else
            {
                BinaryPrimitives.WriteInt64BigEndian(buf, val);
            }

            stream.Write(buf);
        }

        /// <summary>
        /// Reads a 64-bit signed integer from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The 64-bit signed integer read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static long ReadInt64(this Stream stream, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[8];
            stream.Read(buf);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadInt64LittleEndian(buf);
            }
            else
            {
                return BinaryPrimitives.ReadInt64BigEndian(buf);
            }
        }

        /// <summary>
        /// Reads a 64-bit unsigned integer from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The 64-bit unsigned integer read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ulong ReadUInt64(this Stream stream, Endianness endianness)
        {
            Span<byte> buf = stackalloc byte[8];
            stream.Read(buf);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadUInt64LittleEndian(buf);
            }
            else
            {
                return BinaryPrimitives.ReadUInt64BigEndian(buf);
            }
        }

        /// <summary>
        /// Reads a byte array of the specified length from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length of the byte array to read.</param>
        /// <returns>The byte array read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static byte[] Read(this Stream stream, long length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }

        /// <summary>
        /// Compares the data in the stream with the provided byte array.
        /// </summary>
        /// <param name="stream">The stream to compare with.</param>
        /// <param name="compare">The byte array to compare with the stream data.</param>
        /// <returns>True if the stream data matches the provided byte array; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool Compare(this Stream stream, byte[] compare)
        {
#nullable disable
            bool pool = compare.Length > 2048;
            byte[] array = null;
            Span<byte> buffer = pool ? (Span<byte>)(array = ArrayPool<byte>.Shared.Rent(compare.Length)) : (stackalloc byte[compare.Length]);
            stream.Read(buffer);
            var result = buffer.SequenceEqual(compare);
            if (pool)
            {
                ArrayPool<byte>.Shared.Return(array);
            }

            return result;
#nullable enable
        }

        /// <summary>
        /// Compares the version read from the stream with the specified minimum and latest versions.
        /// </summary>
        /// <param name="stream">The stream to read the version from.</param>
        /// <param name="min">The minimum version to compare against.</param>
        /// <param name="latest">The latest version to compare against.</param>
        /// <param name="endianness">The endianness to use for reading the version.</param>
        /// <param name="version">The version read from the stream.</param>
        /// <returns>True if the version is within the specified range; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool CompareVersion(this Stream stream, ulong min, ulong latest, Endianness endianness, out ulong version)
        {
            version = stream.ReadUInt64(endianness);
            return version >= min && version <= latest;
        }

        /// <summary>
        /// Reads a byte array of the specified length from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length of the byte array to read.</param>
        /// <returns>The byte array read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static byte[] ReadBytes(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        /// <summary>
        /// Reads a byte array of the specified length from the stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length of the byte array to read.</param>
        /// <returns>The byte array read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static byte[] ReadBytes(this Stream stream, uint length)
        {
            byte[] bytes = new byte[length];
            stream.Read(bytes);
            return bytes;
        }

        /// <summary>
        /// Writes a Vector4 to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="vector">The Vector4 to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteVector4(this Stream stream, Vector4 vector, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[16];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dst, vector.X);
                BinaryPrimitives.WriteSingleLittleEndian(dst[4..], vector.Y);
                BinaryPrimitives.WriteSingleLittleEndian(dst[8..], vector.Z);
                BinaryPrimitives.WriteSingleLittleEndian(dst[12..], vector.W);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dst, vector.X);
                BinaryPrimitives.WriteSingleBigEndian(dst[4..], vector.Y);
                BinaryPrimitives.WriteSingleBigEndian(dst[8..], vector.Z);
                BinaryPrimitives.WriteSingleBigEndian(dst[12..], vector.W);
            }

            stream.Write(dst);
        }

        /// <summary>
        /// Reads a Vector4 from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The Vector4 read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector4 ReadVector4(this Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[16];
            stream.Read(src);
            Vector4 vector;
            if (endianness == Endianness.LittleEndian)
            {
                vector.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                vector.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                vector.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
                vector.W = BinaryPrimitives.ReadSingleLittleEndian(src[12..]);
            }
            else
            {
                vector.X = BinaryPrimitives.ReadSingleBigEndian(src);
                vector.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                vector.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
                vector.W = BinaryPrimitives.ReadSingleBigEndian(src[12..]);
            }

            return vector;
        }

        /// <summary>
        /// Writes a Vector3 to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="vector">The Vector3 to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteVector3(this Stream stream, Vector3 vector, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[12];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dst, vector.X);
                BinaryPrimitives.WriteSingleLittleEndian(dst[4..], vector.Y);
                BinaryPrimitives.WriteSingleLittleEndian(dst[8..], vector.Z);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dst, vector.X);
                BinaryPrimitives.WriteSingleBigEndian(dst[4..], vector.Y);
                BinaryPrimitives.WriteSingleBigEndian(dst[8..], vector.Z);
            }

            stream.Write(dst);
        }

        /// <summary>
        /// Reads a Vector3 from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The Vector3 read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector3 ReadVector3(this Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[12];
            stream.Read(src);
            Vector3 vector;
            if (endianness == Endianness.LittleEndian)
            {
                vector.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                vector.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
                vector.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
            }
            else
            {
                vector.X = BinaryPrimitives.ReadSingleBigEndian(src);
                vector.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
                vector.Z = BinaryPrimitives.ReadSingleBigEndian(src[8..]);
            }

            return vector;
        }

        /// <summary>
        /// Writes a Vector2 to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="vector">The Vector2 to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteVector2(this Stream stream, Vector2 vector, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[8];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dst, vector.X);
                BinaryPrimitives.WriteSingleLittleEndian(dst[4..], vector.Y);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dst, vector.X);
                BinaryPrimitives.WriteSingleBigEndian(dst[4..], vector.Y);
            }

            stream.Write(dst);
        }

        /// <summary>
        /// Reads a Vector2 from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The Vector2 read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector2 ReadVector2(this Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[8];
            stream.Read(src);
            Vector2 vector;
            if (endianness == Endianness.LittleEndian)
            {
                vector.X = BinaryPrimitives.ReadSingleLittleEndian(src);
                vector.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
            }
            else
            {
                vector.X = BinaryPrimitives.ReadSingleBigEndian(src);
                vector.Y = BinaryPrimitives.ReadSingleBigEndian(src[4..]);
            }

            return vector;
        }

        /// <summary>
        /// Writes a single-precision floating-point value to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The single-precision floating-point value to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteFloat(this Stream stream, float value, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[4];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dst, value);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dst, value);
            }

            stream.Write(dst);
        }

        /// <summary>
        /// Reads a single-precision floating-point value from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The single-precision floating-point value read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float ReadFloat(this Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[4];
            stream.Read(src);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadSingleLittleEndian(src);
            }
            else
            {
                return BinaryPrimitives.ReadSingleBigEndian(src);
            }
        }

        /// <summary>
        /// Writes a double-precision floating-point value to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="value">The double-precision floating-point value to write.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteDouble(this Stream stream, double value, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[8];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteDoubleLittleEndian(dst, value);
            }
            else
            {
                BinaryPrimitives.WriteDoubleBigEndian(dst, value);
            }

            stream.Write(dst);
        }

        /// <summary>
        /// Reads a double-precision floating-point value from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The double-precision floating-point value read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static double ReadDouble(this Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[8];
            stream.Read(src);
            if (endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadDoubleLittleEndian(src);
            }
            else
            {
                return BinaryPrimitives.ReadDoubleBigEndian(src);
            }
        }

        /// <summary>
        /// Writes a 4x4 matrix to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="matrix">The 4x4 matrix to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteMatrix4x4(this Stream stream, Matrix4x4 matrix, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[64];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dst[0..], matrix.M11);
                BinaryPrimitives.WriteSingleLittleEndian(dst[4..], matrix.M12);
                BinaryPrimitives.WriteSingleLittleEndian(dst[8..], matrix.M13);
                BinaryPrimitives.WriteSingleLittleEndian(dst[12..], matrix.M14);
                BinaryPrimitives.WriteSingleLittleEndian(dst[16..], matrix.M21);
                BinaryPrimitives.WriteSingleLittleEndian(dst[20..], matrix.M22);
                BinaryPrimitives.WriteSingleLittleEndian(dst[24..], matrix.M23);
                BinaryPrimitives.WriteSingleLittleEndian(dst[28..], matrix.M24);
                BinaryPrimitives.WriteSingleLittleEndian(dst[32..], matrix.M31);
                BinaryPrimitives.WriteSingleLittleEndian(dst[36..], matrix.M32);
                BinaryPrimitives.WriteSingleLittleEndian(dst[40..], matrix.M33);
                BinaryPrimitives.WriteSingleLittleEndian(dst[44..], matrix.M34);
                BinaryPrimitives.WriteSingleLittleEndian(dst[48..], matrix.M41);
                BinaryPrimitives.WriteSingleLittleEndian(dst[52..], matrix.M42);
                BinaryPrimitives.WriteSingleLittleEndian(dst[56..], matrix.M43);
                BinaryPrimitives.WriteSingleLittleEndian(dst[60..], matrix.M44);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dst[0..], matrix.M11);
                BinaryPrimitives.WriteSingleBigEndian(dst[4..], matrix.M12);
                BinaryPrimitives.WriteSingleBigEndian(dst[8..], matrix.M13);
                BinaryPrimitives.WriteSingleBigEndian(dst[12..], matrix.M14);
                BinaryPrimitives.WriteSingleBigEndian(dst[16..], matrix.M21);
                BinaryPrimitives.WriteSingleBigEndian(dst[20..], matrix.M22);
                BinaryPrimitives.WriteSingleBigEndian(dst[24..], matrix.M23);
                BinaryPrimitives.WriteSingleBigEndian(dst[28..], matrix.M24);
                BinaryPrimitives.WriteSingleBigEndian(dst[32..], matrix.M31);
                BinaryPrimitives.WriteSingleBigEndian(dst[36..], matrix.M32);
                BinaryPrimitives.WriteSingleBigEndian(dst[40..], matrix.M33);
                BinaryPrimitives.WriteSingleBigEndian(dst[44..], matrix.M34);
                BinaryPrimitives.WriteSingleBigEndian(dst[48..], matrix.M41);
                BinaryPrimitives.WriteSingleBigEndian(dst[52..], matrix.M42);
                BinaryPrimitives.WriteSingleBigEndian(dst[56..], matrix.M43);
                BinaryPrimitives.WriteSingleBigEndian(dst[60..], matrix.M44);
            }

            stream.Write(dst);
        }

        /// <summary>
        /// Reads a 4x4 matrix from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The 4x4 matrix read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Matrix4x4 ReadMatrix4x4(this Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[64];
            stream.Read(src);
            Matrix4x4 matrix;
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

            return matrix;
        }

        /// <summary>
        /// Writes a Quaternion to the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="quaternion">The Quaternion to write to the stream.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteQuaternion(this Stream stream, Quaternion quaternion, Endianness endianness)
        {
            Span<byte> dst = stackalloc byte[16];
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(dst, quaternion.X);
                BinaryPrimitives.WriteSingleLittleEndian(dst[4..], quaternion.Y);
                BinaryPrimitives.WriteSingleLittleEndian(dst[8..], quaternion.Z);
                BinaryPrimitives.WriteSingleLittleEndian(dst[12..], quaternion.W);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(dst, quaternion.X);
                BinaryPrimitives.WriteSingleBigEndian(dst[4..], quaternion.Y);
                BinaryPrimitives.WriteSingleBigEndian(dst[8..], quaternion.Z);
                BinaryPrimitives.WriteSingleBigEndian(dst[12..], quaternion.W);
            }

            stream.Write(dst);
        }

        /// <summary>
        /// Reads a Quaternion from the stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading.</param>
        /// <returns>The Quaternion read from the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Quaternion ReadQuaternion(this Stream stream, Endianness endianness)
        {
            Span<byte> src = stackalloc byte[16];
            stream.Read(src);
            Quaternion quaternion;
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

            return quaternion;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayFloat(this Stream stream, float[] array, Endianness endianness)
        {
            int stride = sizeof(float);
            fixed (float* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayFloat(this Stream stream, float[] array, Endianness endianness)
        {
            int stride = sizeof(float);
            fixed (float* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayVector2(this Stream stream, Vector2[] array, Endianness endianness)
        {
            int stride = sizeof(Vector2);
            fixed (Vector2* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayVector2(this Stream stream, Vector2[] array, Endianness endianness)
        {
            int stride = sizeof(Vector2);
            fixed (Vector2* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayVector3(this Stream stream, Vector3[] array, Endianness endianness)
        {
            int stride = sizeof(Vector3);
            fixed (Vector3* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayVector3(this Stream stream, Vector3[] array, Endianness endianness)
        {
            int stride = sizeof(Vector3);
            fixed (Vector3* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayVector4(this Stream stream, Vector4[] array, Endianness endianness)
        {
            int stride = sizeof(Vector4);
            fixed (Vector4* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayVector4(this Stream stream, Vector4[] array, Endianness endianness)
        {
            int stride = sizeof(Vector4);
            fixed (Vector4* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayDouble(this Stream stream, double[] array, Endianness endianness)
        {
            int stride = sizeof(double);
            fixed (double* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayDouble(this Stream stream, double[] array, Endianness endianness)
        {
            int stride = sizeof(double);
            fixed (double* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayUInt16(this Stream stream, ushort[] array, Endianness endianness)
        {
            int stride = sizeof(ushort);
            fixed (ushort* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayUInt16(this Stream stream, ushort[] array, Endianness endianness)
        {
            int stride = sizeof(ushort);
            fixed (ushort* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayInt16(this Stream stream, short[] array, Endianness endianness)
        {
            int stride = sizeof(short);
            fixed (short* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayInt16(this Stream stream, Span<short> array, Endianness endianness)
        {
            int stride = sizeof(short);
            fixed (short* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayUInt32(this Stream stream, uint[] array, Endianness endianness)
        {
            int stride = sizeof(uint);
            fixed (uint* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayUInt32(this Stream stream, uint[] array, Endianness endianness)
        {
            int stride = sizeof(uint);
            fixed (uint* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayInt32(this Stream stream, int[] array, Endianness endianness)
        {
            int stride = sizeof(int);
            fixed (int* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayInt32(this Stream stream, int[] array, Endianness endianness)
        {
            int stride = sizeof(int);
            fixed (int* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayUInt64(this Stream stream, ulong[] array, Endianness endianness)
        {
            int stride = sizeof(ulong);
            fixed (ulong* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayUInt64(this Stream stream, ulong[] array, Endianness endianness)
        {
            int stride = sizeof(ulong);
            fixed (ulong* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadArrayInt64(this Stream stream, long[] array, Endianness endianness)
        {
            int stride = sizeof(long);
            fixed (long* p = array)
            {
                ReadArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteArrayInt64(this Stream stream, long[] array, Endianness endianness)
        {
            int stride = sizeof(long);
            fixed (long* p = array)
            {
                WriteArray(stream, array, endianness, stride, p, &ReverseEndianness);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteArray<T>(Stream stream, Span<T> array, Endianness endianness, int stride, T* p, delegate*<T, T*, void> reverseEndianness) where T : unmanaged
        {
            bool reverse = BitConverter.IsLittleEndian && endianness == Endianness.BigEndian || !BitConverter.IsLittleEndian && endianness == Endianness.LittleEndian;
            if (reverse)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    reverseEndianness(p[i], &p[i]);
                }
            }

            Span<byte> bytes = new(p, array.Length * stride);
            stream.Write(bytes);

            if (reverse)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    reverseEndianness(p[i], &p[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ReadArray<T>(Stream stream, T[] array, Endianness endianness, int stride, T* p, delegate*<T, T*, void> reverseEndianness) where T : unmanaged
        {
            Span<byte> bytes = new(p, array.Length * stride);
            stream.Read(bytes);

            if (BitConverter.IsLittleEndian && endianness == Endianness.BigEndian || !BitConverter.IsLittleEndian && endianness == Endianness.LittleEndian)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    reverseEndianness(p[i], &p[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(ushort target, ushort* output)
        {
            byte byte0 = (byte)(target >> 0);
            byte byte1 = (byte)(target >> 8);
            *output = (ushort)((byte0 << 8) | byte1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(short target, short* output)
        {
            byte byte0 = (byte)(target >> 0);
            byte byte1 = (byte)(target >> 8);
            *output = (short)((byte0 << 8) | byte1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(uint target, uint* output)
        {
            byte byte0 = (byte)(target >> 0);
            byte byte1 = (byte)(target >> 8);
            byte byte2 = (byte)(target >> 16);
            byte byte3 = (byte)(target >> 24);
            *output = (uint)((byte0 << 24) | (byte1 << 16) | (byte2 << 8) | byte3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(int target, int* output)
        {
            byte byte0 = (byte)(target >> 0);
            byte byte1 = (byte)(target >> 8);
            byte byte2 = (byte)(target >> 16);
            byte byte3 = (byte)(target >> 24);
            *output = (byte0 << 24) | (byte1 << 16) | (byte2 << 8) | byte3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(ulong target, ulong* output)
        {
            byte byte0 = (byte)(target >> 0);
            byte byte1 = (byte)(target >> 8);
            byte byte2 = (byte)(target >> 16);
            byte byte3 = (byte)(target >> 24);
            byte byte4 = (byte)(target >> 32);
            byte byte5 = (byte)(target >> 40);
            byte byte6 = (byte)(target >> 48);
            byte byte7 = (byte)(target >> 56);
            *output = (ulong)((byte0 << 56) | (byte1 << 48) | (byte2 << 40) | (byte3 << 32) | (byte4 << 24) | (byte5 << 16) | (byte6 << 8) | byte7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(long target, long* output)
        {
            byte byte0 = (byte)(target >> 0);
            byte byte1 = (byte)(target >> 8);
            byte byte2 = (byte)(target >> 16);
            byte byte3 = (byte)(target >> 24);
            byte byte4 = (byte)(target >> 32);
            byte byte5 = (byte)(target >> 40);
            byte byte6 = (byte)(target >> 48);
            byte byte7 = (byte)(target >> 56);
            *output = (byte0 << 56) | (byte1 << 48) | (byte2 << 40) | (byte3 << 32) | (byte4 << 24) | (byte5 << 16) | (byte6 << 8) | byte7;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(float target, float* output)
        {
            uint uTarget = *(uint*)&target;
            uint reversedValue;
            ReverseEndianness(uTarget, &reversedValue);
            *output = *(float*)&reversedValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(double target, double* output)
        {
            ulong uTarget = *(ulong*)&target;
            ulong reversedValue;
            ReverseEndianness(uTarget, &reversedValue);
            *output = *(double*)&reversedValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(Vector2 target, Vector2* output)
        {
            ReverseEndianness(target.X, &target.X);
            ReverseEndianness(target.Y, &target.Y);
            *output = target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(Vector3 target, Vector3* output)
        {
            ReverseEndianness(target.X, &target.X);
            ReverseEndianness(target.Y, &target.Y);
            ReverseEndianness(target.Z, &target.Z);
            *output = target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReverseEndianness(Vector4 target, Vector4* output)
        {
            ReverseEndianness(target.X, &target.X);
            ReverseEndianness(target.Y, &target.Y);
            ReverseEndianness(target.Z, &target.Z);
            ReverseEndianness(target.W, &target.W);
            *output = target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveBlock(this Stream stream, long from, long to, long size)
        {
            if (from == stream.Length)
            {
                return;
            }

            const int BufferSize = 8192;
            Span<byte> buffer = stackalloc byte[BufferSize];

            long positionFrom = from;
            long positionTo = to;
            while (size > 0)
            {
                int bytesToRead = (int)Math.Min(size, BufferSize);

                stream.Position = positionFrom;
                int read = stream.Read(buffer[..bytesToRead]);
                positionFrom += read;

                stream.Position = to;
                stream.Write(buffer[..read]);
                positionTo += read;

                size -= read;
            }
        }
    }
}