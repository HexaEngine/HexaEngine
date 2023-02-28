namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.IO.Json;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;

    public unsafe struct MeshData
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

#pragma warning disable CS8618 // Non-nullable field 'Vertices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'Indices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public MeshData()
#pragma warning restore CS8618 // Non-nullable field 'Indices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'Vertices' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
        }

        public void Write(Stream stream)
        {
        }

        public void Read(Stream stream)
        {
        }
    }
}