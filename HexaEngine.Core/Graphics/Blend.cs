namespace HexaEngine.Core.Graphics
{
    public enum Blend : int
    {
        Zero = unchecked((int)1),
        One = unchecked((int)2),
        SourceColor = unchecked((int)3),
        InverseSourceColor = unchecked((int)4),
        SourceAlpha = unchecked((int)5),
        InverseSourceAlpha = unchecked((int)6),
        DestinationAlpha = unchecked((int)7),
        InverseDestinationAlpha = unchecked((int)8),
        DestinationColor = unchecked((int)9),
        InverseDestinationColor = unchecked((int)10),
        SourceAlphaSaturate = unchecked((int)11),
        BlendFactor = unchecked((int)14),
        InverseBlendFactor = unchecked((int)15),
        Source1Color = unchecked((int)16),
        InverseSource1Color = unchecked((int)17),
        Source1Alpha = unchecked((int)18),
        InverseSource1Alpha = unchecked((int)19)
    }
}