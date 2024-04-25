namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    [Flags]
    public enum TypeFlags
    {
        Undefined = 0,
        Void = 1,
        Bool = 2,
        Int = 4,
        Float = 8,
        Vector = 0x100,
        Matrix = 0x200,
        ExternalImage = 0x10000,
        ExternalSampler = 0x20000,
        ExternalSampledImage = 0x40000,
        ExternalBlock = 0x80000,
        ExternalAccelerationStructure = 0x100000,
        ExternalMask = 0xFF0000,
        Struct = 0x10000000,
        Array = 0x20000000,
        Ref = 0x40000000
    }
}