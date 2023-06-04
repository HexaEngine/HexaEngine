namespace HexaEngine.Core.IO.Terrains
{
    using System;

    [Flags]
    public enum TerrainFlags
    {
        None = 0,
        Positions = 2,
        UVs = 4,
        Normals = 8,
        Tangents = 16,
        Bitangents = 32,
    }
}