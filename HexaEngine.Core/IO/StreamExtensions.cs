namespace HexaEngine.Core.IO
{
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class StreamExtensions
    {
        public static void WriteString(this Stream stream, string str, Encoding encoder)
        {
            var count = encoder.GetByteCount(str);
            Span<byte> dst = count < 2048 ? stackalloc byte[count] : new byte[count];
            BinaryPrimitives.WriteInt32LittleEndian(dst, count);
            encoder.GetBytes(str, dst[4..]);
            stream.Write(dst);
        }

        public static string ReadString(this Stream stream, Encoding decoder)
        {
            Span<byte> buf = stackalloc byte[4];
            stream.Read(buf);
            int len = BinaryPrimitives.ReadInt32LittleEndian(buf);

            Span<byte> src = len < 2048 ? stackalloc byte[len] : new byte[len];
            stream.Read(src);

            int charCount = decoder.GetCharCount(src);
            Span<char> chars = charCount < 2048 ? stackalloc char[charCount] : new char[charCount];
            decoder.GetChars(src, chars);
            return new(chars);
        }

        public static void WriteInt(this Stream stream, int val)
        {
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(buf, val);
            stream.Write(buf);
        }

        public static int ReadInt(this Stream stream)
        {
            Span<byte> buf = stackalloc byte[4];
            stream.Read(buf);
            return BinaryPrimitives.ReadInt32LittleEndian(buf);
        }

        public static uint ReadUInt(this Stream stream)
        {
            Span<byte> buf = stackalloc byte[4];
            stream.Read(buf);
            return BinaryPrimitives.ReadUInt32LittleEndian(buf);
        }

        public static void WriteUInt(this Stream stream, uint val)
        {
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buf, val);
            stream.Write(buf);
        }

        public static void WriteUInt64(this Stream stream, ulong val)
        {
            Span<byte> buf = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(buf, val);
            stream.Write(buf);
        }

        public static void WriteInt64(this Stream stream, long val)
        {
            Span<byte> buf = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(buf, val);
            stream.Write(buf);
        }

        public static long ReadInt64(this Stream stream)
        {
            Span<byte> buf = stackalloc byte[8];
            stream.Read(buf);
            return BinaryPrimitives.ReadInt64LittleEndian(buf);
        }

        public static ulong ReadUInt64(this Stream stream)
        {
            Span<byte> buf = stackalloc byte[8];
            stream.Read(buf);
            return BinaryPrimitives.ReadUInt64LittleEndian(buf);
        }

        public static byte[] Read(this Stream stream, long length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }

        public static bool Compare(this Stream stream, byte[] compare)
        {
#nullable disable
            bool pool = compare.Length > 2048;
            byte[] array = null;
            Span<byte> buffer = pool ? (Span<byte>)(array = ArrayPool<byte>.Shared.Rent(compare.Length)) : (stackalloc byte[compare.Length]);
            stream.Read(buffer);
            var result = buffer.SequenceEqual(compare);
            if (pool)
                ArrayPool<byte>.Shared.Return(array);
            return result;
#nullable enable
        }

        public static bool Compare(this Stream stream, ulong value)
        {
            return stream.ReadUInt64() == value;
        }

        public static bool Compare(this ReadOnlySpan<byte> src, ulong value)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(src) == value;
        }

        public static string ReadString(this ReadOnlySpan<byte> src, Encoding encoding, out int read)
        {
            var len = BinaryPrimitives.ReadInt32LittleEndian(src);
            read = len + 4;
            return encoding.GetString(src.Slice(4, len));
        }

        public static int WriteString(this Span<byte> dest, ReadOnlySpan<char> src, Encoding encoding)
        {
            var len = encoding.GetByteCount(src);
            BinaryPrimitives.WriteInt32LittleEndian(dest, len);
            encoding.GetBytes(src, dest[4..]);
            return 4 + len;
        }

        public static byte[] ReadBytes(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        public static unsafe T ReadStruct<T>(this Stream stream) where T : unmanaged
        {
#nullable disable
            var byteLength = Marshal.SizeOf(typeof(T));
            var bytes = stream.ReadBytes(byteLength);
            var pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var stt = (T)Marshal.PtrToStructure(
                pinned.AddrOfPinnedObject(),
                typeof(T));
            pinned.Free();
            return stt;
#nullable enable
        }

        public static unsafe void WriteStruct<T>(this Stream stream, T t) where T : unmanaged
        {
#nullable disable
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(sizeOfT);
            Marshal.StructureToPtr(t, ptr, false);
            var bytes = new byte[sizeOfT];
            Marshal.Copy(ptr, bytes, 0, bytes.Length);
            Marshal.FreeHGlobal(ptr);
            stream.Write(bytes);
#nullable enable
        }

        public static int WriteVector3(Span<byte> dst, Vector3 vector)
        {
            BinaryPrimitives.WriteSingleLittleEndian(dst, vector.X);
            BinaryPrimitives.WriteSingleLittleEndian(dst[4..], vector.Y);
            BinaryPrimitives.WriteSingleLittleEndian(dst[8..], vector.Z);
            return 12;
        }

        public static int ReadVector3(ReadOnlySpan<byte> src, out Vector3 vector)
        {
            vector.X = BinaryPrimitives.ReadSingleLittleEndian(src);
            vector.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
            vector.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
            return 12;
        }

        public static void WriteVector3(this Stream stream, Vector3 vector)
        {
            Span<byte> dst = stackalloc byte[12];
            BinaryPrimitives.WriteSingleLittleEndian(dst, vector.X);
            BinaryPrimitives.WriteSingleLittleEndian(dst[4..], vector.Y);
            BinaryPrimitives.WriteSingleLittleEndian(dst[8..], vector.Z);
            stream.Write(dst);
        }

        public static Vector3 ReadVector3(this Stream stream)
        {
            Span<byte> src = stackalloc byte[12];
            stream.Read(src);
            Vector3 vector;
            vector.X = BinaryPrimitives.ReadSingleLittleEndian(src);
            vector.Y = BinaryPrimitives.ReadSingleLittleEndian(src[4..]);
            vector.Z = BinaryPrimitives.ReadSingleLittleEndian(src[8..]);
            return vector;
        }

        public static void WriteFloat(this Stream stream, float value)
        {
            Span<byte> dst = stackalloc byte[4];
            BinaryPrimitives.WriteSingleLittleEndian(dst, value);
            stream.Write(dst);
        }

        public static float ReadFloat(this Stream stream)
        {
            Span<byte> src = stackalloc byte[4];
            stream.Read(src);
            return BinaryPrimitives.ReadSingleLittleEndian(src);
        }
    }
}