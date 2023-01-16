namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.Json;
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

        public MeshData()
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