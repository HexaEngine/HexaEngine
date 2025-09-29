namespace HexaEngine.ShadingLang
{
    [Flags]
    public enum HXSLModifierFlags
    {
        None,
        Inline = 1,
        Public = 2,
        Private = 4,
        Static = 8,
        Nointerpolation = 16,
        Shared = 32,
        GroupShared = 64,
        Uniform = 128,
        Volatile = 256,
    }
}