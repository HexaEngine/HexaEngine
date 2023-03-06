namespace HexaEngine.Core.IO.Meshes
{
    [Flags]
    public enum MeshFlags
    {
        None = 0,
        Indexed = 1,
        Colors = 2,
        Positions = 4,
        UVs = 8,
        Normals = 16,
        Tangents = 32,
        Weights = 64,
    }
}