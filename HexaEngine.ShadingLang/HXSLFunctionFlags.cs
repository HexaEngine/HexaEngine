namespace HexaEngine.ShadingLang
{
    [Flags]
    public enum HXSLFunctionFlags
    {
        None,
        Inline = 2,
        Public = 4,
        Private = 8,
    }
}