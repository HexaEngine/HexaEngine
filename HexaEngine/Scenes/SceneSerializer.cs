namespace HexaEngine.Scenes
{
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public unsafe class SceneSerializer
    {
        public static void Serialize(Scene scene)
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
            options.IncludeFields = true;
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;

            File.WriteAllText("scene.json", JsonSerializer.Serialize(scene, typeof(Scene), options));
        }

        public void Deserialize(Scene scene)
        {
        }

        public void MeasureSizeScene(Scene scene)
        {
            int size;
        }

        public void MeasureSizeResources(Scene scene)
        {
        }

        private static int WriteString(string s, Span<byte> dest)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, s.Length * 2);
            return Encoding.Unicode.GetBytes(s, dest[4..]) + 4;
        }

        private static int ReadString(ref string s, Span<byte> src)
        {
            int length = BinaryPrimitives.ReadInt32LittleEndian(src);
            s = Encoding.Unicode.GetString(src.Slice(4, length));
            return length + 4;
        }

        private static int WriteInt16(short v, Span<byte> dest)
        {
            BinaryPrimitives.WriteInt16LittleEndian(dest, v);
            return 2;
        }

        private static int ReadInt16(ref short v, Span<byte> src)
        {
            v = BinaryPrimitives.ReadInt16LittleEndian(src);
            return 2;
        }

        private static int WriteInt32(int v, Span<byte> dest)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, v);
            return 4;
        }

        private static int ReadInt32(ref int v, Span<byte> src)
        {
            v = BinaryPrimitives.ReadInt32LittleEndian(src);
            return 4;
        }

        private static int WriteInt64(long v, Span<byte> dest)
        {
            BinaryPrimitives.WriteInt64LittleEndian(dest, v);
            return 8;
        }

        private static int ReadInt64(ref long v, Span<byte> src)
        {
            v = BinaryPrimitives.ReadInt64LittleEndian(src);
            return 8;
        }

        private static int WriteUInt16(ushort v, Span<byte> dest)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(dest, v);
            return 2;
        }

        private static int ReadUInt16(ref ushort v, Span<byte> src)
        {
            v = BinaryPrimitives.ReadUInt16LittleEndian(src);
            return 2;
        }

        private static int WriteUInt32(uint v, Span<byte> dest)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(dest, v);
            return 4;
        }

        private static int ReadUInt32(ref uint v, Span<byte> src)
        {
            v = BinaryPrimitives.ReadUInt32LittleEndian(src);
            return 4;
        }

        private static int WriteUInt64(ulong v, Span<byte> dest)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(dest, v);
            return 8;
        }

        private static int ReadUInt64(ref ulong v, Span<byte> src)
        {
            v = BinaryPrimitives.ReadUInt64LittleEndian(src);
            return 8;
        }

        private static int MeasureStruct<T>(T v) where T : unmanaged
        {
            return sizeof(T) + 4;
        }

        private static int WriteStruct<T>(T v, Span<byte> dest) where T : unmanaged
        {
            Span<byte> bytes = MemoryMarshal.AsBytes((Span<T>)new T[] { v });
            BinaryPrimitives.WriteInt32LittleEndian(dest, bytes.Length);
            bytes.CopyTo(dest[4..]);
            return bytes.Length + 4;
        }

        private static int ReadStruct<T>(ref T v, Span<byte> src) where T : unmanaged
        {
            var length = BinaryPrimitives.ReadInt32LittleEndian(src);
            v = MemoryMarshal.Cast<byte, T>(src.Slice(4, length))[0];
            return length + 4;
        }
    }
}