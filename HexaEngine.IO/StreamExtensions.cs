namespace HexaEngine.IO
{
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;

    public static class StreamExtensions
    {
        public static void WriteString(this Stream stream, string str)
        {
            stream.WriteInt(str.Length);
            stream.Write(Encoding.UTF8.GetBytes(str));
        }

        public static void WriteString(this Stream stream, string str, Encoding encoding)
        {
            stream.WriteInt(str.Length);
            stream.Write(encoding.GetBytes(str));
        }

        public static string ReadString(this Stream stream)
        {
            var length = stream.ReadInt();
            var buffer = new byte[length];
            stream.Read(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        public static string ReadString(this Stream stream, Encoding encoding)
        {
            var length = stream.ReadInt();
            var buffer = new byte[length];
            stream.Read(buffer);
            return encoding.GetString(buffer);
        }

        public static void WriteInt(this Stream stream, int val)
        {
            stream.Write(BitConverter.GetBytes(val));
        }

        public static int ReadInt(this Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(4);
            stream.Read(buffer, 0, 4);
            var val = BitConverter.ToInt32(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return val;
        }

        public static void WriteUInt64(this Stream stream, ulong val)
        {
            stream.Write(BitConverter.GetBytes(val));
        }

        public static void WriteInt64(this Stream stream, long val)
        {
            stream.Write(BitConverter.GetBytes(val));
        }

        public static long ReadInt64(this Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(8);
            stream.Read(buffer, 0, 8);
            var val = BitConverter.ToInt64(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return val;
        }

        public static ulong ReadUInt64(this Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(8);
            stream.Read(buffer, 0, 8);
            var val = BitConverter.ToUInt64(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return val;
        }

        public static byte[] Read(this Stream stream, long length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }

        public static bool Compare(this Stream stream, byte[] compare)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(compare.Length);
            stream.Read(buffer, 0, compare.Length);
            for (int i = 0; i < compare.Length; i++)
            {
                if (buffer[i] != compare[i])
                    return false;
            }
            ArrayPool<byte>.Shared.Return(buffer);
            return true;
        }

        public static bool Compare(this Stream stream, ulong value)
        {
            var other = stream.ReadUInt64();
            return other == value;
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
    }
}