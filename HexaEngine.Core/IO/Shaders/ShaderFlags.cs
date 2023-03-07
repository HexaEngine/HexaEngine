namespace HexaEngine.Core.IO.Shaders
{
    [Flags]
    public enum ShaderFlags
    {
        None = 0,
        Debug = 1,
        StripDebug = 2,
        Optimization1 = 4,
        Optimization2 = 8,
    }
}