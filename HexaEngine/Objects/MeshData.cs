namespace HexaEngine.Objects
{
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Buffers;
    using System.Runtime.InteropServices;

    public unsafe class MeshVertexArrayConverter : JsonConverter<MeshVertex[]>
    {
        public override MeshVertex[] ReadJson(JsonReader reader, Type objectType, MeshVertex[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            if (token.Type != JTokenType.Bytes)
                throw new InvalidDataException();
            byte[] value = token.Value<byte[]>();

            return MemoryMarshal.Cast<byte, MeshVertex>(value.AsSpan()).ToArray();
        }

        public override void WriteJson(JsonWriter writer, MeshVertex[] value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(MemoryMarshal.AsBytes(value.AsSpan()).ToArray());
            t.WriteTo(writer);
        }
    }

    public unsafe class UnmanagedArrayConverter<T> : JsonConverter<T[]> where T : unmanaged
    {
        public override T[] ReadJson(JsonReader reader, Type objectType, T[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            if (token.Type != JTokenType.Bytes)
                throw new InvalidDataException();
            byte[] value = token.Value<byte[]>();

            return MemoryMarshal.Cast<byte, T>(value.AsSpan()).ToArray();
        }

        public override void WriteJson(JsonWriter writer, T[] value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(MemoryMarshal.AsBytes(value.AsSpan()).ToArray());
            t.WriteTo(writer);
        }
    }

    public unsafe class UnmanagedConverter<T> : JsonConverter<T> where T : unmanaged
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            if (token.Type != JTokenType.Bytes)
                throw new InvalidDataException();
            byte[] value = token.Value<byte[]>();

            return MemoryMarshal.Cast<byte, T>(value.AsSpan())[0];
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(MemoryMarshal.AsBytes(new T[] { value }.AsSpan()).ToArray());
            t.WriteTo(writer);
        }
    }

    public unsafe class MeshData
    {
        public string Name = string.Empty;

        [JsonConverter(typeof(UnmanagedArrayConverter<MeshVertex>))]
        public MeshVertex[] Vertices;

        [JsonConverter(typeof(UnmanagedArrayConverter<int>))]
        public int[] Indices;

        [JsonConverter(typeof(UnmanagedConverter<BoundingBox>))]
        public BoundingBox BoundingBox;

        [JsonConverter(typeof(UnmanagedConverter<BoundingSphere>))]
        public BoundingSphere BoundingSphere;

        public MeshBone[]? Bones;
        public Animature? Animature;

        public MeshData()
        {
        }
    }
}