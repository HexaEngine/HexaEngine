namespace HexaEngine.ShadingLang
{
    public enum HXSLFieldFlags
    {
        None,
        Static = 1,
        Nointerpolation = 2,
        Shared = 4,
        GroupShared = 8,
        Uniform = 16,
        Volatile = 32,
        Public = 64,
        Private = 128,
    }
}