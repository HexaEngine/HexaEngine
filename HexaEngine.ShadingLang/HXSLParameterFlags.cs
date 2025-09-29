namespace HexaEngine.ShadingLang
{
    public enum HXSLParameterFlags
    {
        None = 0,
        Out = 1,
        In = 2,
        InOut = In | Out,
        Uniform = 4
    }
}