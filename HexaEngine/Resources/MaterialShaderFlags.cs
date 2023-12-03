#nullable disable

using HexaEngine;

namespace HexaEngine.Resources
{
    public enum MaterialShaderFlags
    {
        None = 0,
        Transparent = 1,
        Custom = 2,
        Depth = 4,
        Shadow = 8,
        AlphaTest = 16,
        TwoSided = 32,
        Tessellation = 64,
    }
}