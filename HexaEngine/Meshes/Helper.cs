namespace HexaEngine.Meshes
{
    using System;
    using System.Buffers.Binary;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class BinaryHelper
    {
        private static readonly Encoder encoder = Encoding.Unicode.GetEncoder();
        private static readonly Decoder decoder = Encoding.Unicode.GetDecoder();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadString(ReadOnlySpan<byte> source, out string result)
        {
            int len = BinaryPrimitives.ReadInt32LittleEndian(source);
            Span<char> en = new char[len / 2];
            decoder.GetChars(source.Slice(4, len), en, true);
            result = new(en);
            return len + 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteString(Span<byte> destination, ReadOnlySpan<char> target)
        {
            int len = encoder.GetByteCount(target, true);
            BinaryPrimitives.WriteInt32LittleEndian(destination, len);
            encoder.GetBytes(target, destination.Slice(4, len), true);
            return len + 4;
        }

        public static int SizeOfString(ReadOnlySpan<char> target)
        {
            return encoder.GetByteCount(target, true) + 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadEnum16<T>(ReadOnlySpan<byte> source, out T t) where T : Enum
        {
            t = (T)(object)BinaryPrimitives.ReadInt16LittleEndian(source);
            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteEnum16<T>(Span<byte> destination, T t) where T : Enum
        {
            BinaryPrimitives.WriteInt16LittleEndian(destination, (short)(object)t);
            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadEnumU16<T>(ReadOnlySpan<byte> source, out T t) where T : Enum
        {
            t = (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(source);
            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteEnumU16<T>(Span<byte> destination, T t) where T : Enum
        {
            BinaryPrimitives.WriteUInt16LittleEndian(destination, (ushort)(object)t);
            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadEnum32<T>(ReadOnlySpan<byte> source, out T t) where T : Enum
        {
            t = (T)(object)BinaryPrimitives.ReadInt32LittleEndian(source);
            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteEnum32<T>(Span<byte> destination, T t) where T : Enum
        {
            BinaryPrimitives.WriteInt32LittleEndian(destination, (int)(object)t);
            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadEnumU32<T>(ReadOnlySpan<byte> source, out T t) where T : Enum
        {
            t = (T)(object)BinaryPrimitives.ReadUInt32LittleEndian(source);
            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteEnumU32<T>(Span<byte> destination, T t) where T : Enum
        {
            BinaryPrimitives.WriteUInt32LittleEndian(destination, (uint)(object)t);
            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadVector3(ReadOnlySpan<byte> source, out Vector3 vector)
        {
            float x = BinaryPrimitives.ReadSingleLittleEndian(source);
            float y = BinaryPrimitives.ReadSingleLittleEndian(source[4..]);
            float z = BinaryPrimitives.ReadSingleLittleEndian(source[8..]);
            vector = new Vector3(x, y, z);
            return sizeof(Vector3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteVector3(Span<byte> source, Vector3 vector)
        {
            BinaryPrimitives.WriteSingleLittleEndian(source, vector.X);
            BinaryPrimitives.WriteSingleLittleEndian(source[4..], vector.Y);
            BinaryPrimitives.WriteSingleLittleEndian(source[8..], vector.Z);
            return sizeof(Vector3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadVector2(ReadOnlySpan<byte> source, out Vector2 vector)
        {
            float x = BinaryPrimitives.ReadSingleLittleEndian(source);
            float y = BinaryPrimitives.ReadSingleLittleEndian(source[4..]);
            vector = new Vector2(x, y);
            return sizeof(Vector2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteVector2(Span<byte> source, Vector2 vector)
        {
            BinaryPrimitives.WriteSingleLittleEndian(source, vector.X);
            BinaryPrimitives.WriteSingleLittleEndian(source[4..], vector.Y);
            return sizeof(Vector2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteVertex(Span<byte> source, MeshVertex vertex)
        {
            int idx = WriteVector3(source, vertex.Position);
            idx += WriteVector2(source[idx..], vertex.Texture);
            idx += WriteVector3(source[idx..], vertex.Normal);
            idx += WriteVector3(source[idx..], vertex.Tangent);
            return idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadVertex(Span<byte> source, out MeshVertex vertex)
        {
            int idx = ReadVector3(source, out vertex.Position);
            idx += ReadVector2(source[idx..], out vertex.Texture);
            idx += ReadVector3(source[idx..], out vertex.Normal);
            idx += ReadVector3(source[idx..], out vertex.Tangent);
            return idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadFloat(ReadOnlySpan<byte> source, out float value)
        {
            value = BinaryPrimitives.ReadSingleLittleEndian(source);
            return sizeof(float);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteFloat(Span<byte> dest, float value)
        {
            BinaryPrimitives.WriteSingleLittleEndian(dest, value);
            return sizeof(float);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(ReadOnlySpan<byte> source, out int value)
        {
            value = BinaryPrimitives.ReadInt32LittleEndian(source);
            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteInt32(Span<byte> dest, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, value);
            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadStructArray<T>(Span<byte> src, int size, out T[] values) where T : struct
        {
            int len = BinaryPrimitives.ReadInt32LittleEndian(src) * size;
            values = MemoryMarshal.Cast<byte, T>(src.Slice(4, len)).ToArray();
            return len + 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteStructArray<T>(Span<byte> dest, int size, T[] values) where T : struct
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, values.Length);
            MemoryMarshal.AsBytes(values.AsSpan()).CopyTo(dest[4..]);
            return size * values.Length + 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOfStructArray<T>(T[] values, int size) where T : struct
        {
            return values.Length * size + 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadBinaryArray<T>(Span<byte> src, out T[] values) where T : struct, IBinarySerializable
        {
            int count = BinaryPrimitives.ReadInt32LittleEndian(src);
            int idx = 4;
            values = new T[count];
            for (int i = 0; i < count; i++)
            {
                idx += values[i].Read(src[idx..]);
            }
            return idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteBinaryArray<T>(Span<byte> dest, T[] values) where T : struct, IBinarySerializable
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, values.Length);
            int idx = 4;
            for (int i = 0; i < values.Length; i++)
            {
                idx += values[i].Write(dest[idx..]);
            }
            return idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOfBinaryArray<T>(T[] values) where T : struct, IBinarySerializable
        {
            return 4 + values.Sum(x => x.SizeOf());
        }
    }
}