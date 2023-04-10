namespace HexaEngine.Core.IO.Meshes
{
    [Flags]
    public enum VertexFlags
    {
        None = 0,
        Colors = 2,
        Positions = 4,
        UVs = 8,
        Normals = 16,
        Tangents = 32,
        Bitangents = 64,
        Skinned = 128,
    }
}