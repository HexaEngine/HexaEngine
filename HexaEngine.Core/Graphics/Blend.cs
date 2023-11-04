namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies different blending options for use in a graphics pipeline.
    /// </summary>
    public enum Blend : int
    {
        /// <summary>
        /// The source and destination factors are both set to zero, resulting in a transparent effect.
        /// </summary>
        Zero = unchecked(1),

        /// <summary>
        /// Both the source and destination factors are set to one, resulting in full opacity.
        /// </summary>
        One = unchecked(2),

        /// <summary>
        /// The result is the source color.
        /// </summary>
        SourceColor = unchecked(3),

        /// <summary>
        /// The result is the inverse of the source color.
        /// </summary>
        InverseSourceColor = unchecked(4),

        /// <summary>
        /// The result is the source alpha component.
        /// </summary>
        SourceAlpha = unchecked(5),

        /// <summary>
        /// The result is the inverse of the source alpha component.
        /// </summary>
        InverseSourceAlpha = unchecked(6),

        /// <summary>
        /// The result is the destination alpha component.
        /// </summary>
        DestinationAlpha = unchecked(7),

        /// <summary>
        /// The result is the inverse of the destination alpha component.
        /// </summary>
        InverseDestinationAlpha = unchecked(8),

        /// <summary>
        /// The result is the destination color.
        /// </summary>
        DestinationColor = unchecked(9),

        /// <summary>
        /// The result is the inverse of the destination color.
        /// </summary>
        InverseDestinationColor = unchecked(10),

        /// <summary>
        /// The result is the source alpha component clamped to a maximum of 1.0.
        /// </summary>
        SourceAlphaSaturate = unchecked(11),

        /// <summary>
        /// The result is a blend factor specified separately.
        /// </summary>
        BlendFactor = unchecked(14),

        /// <summary>
        /// The result is the inverse of a blend factor specified separately.
        /// </summary>
        InverseBlendFactor = unchecked(15),

        /// <summary>
        /// The result is the source1 color.
        /// </summary>
        Source1Color = unchecked(16),

        /// <summary>
        /// The result is the inverse of the source1 color.
        /// </summary>
        InverseSource1Color = unchecked(17),

        /// <summary>
        /// The result is the source1 alpha component.
        /// </summary>
        Source1Alpha = unchecked(18),

        /// <summary>
        /// The result is the inverse of the source1 alpha component.
        /// </summary>
        InverseSource1Alpha = unchecked(19)
    }
}