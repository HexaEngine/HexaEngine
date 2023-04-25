#nullable disable

namespace HexaEngine.Core.Resources
{
    public enum MaterialShaderFlags
    {
        None = 0,
        Forward = 1,
        Deferred = 2,
        Custom = 4,
        Depth = 8,
        Shadow = 16,
    }
}