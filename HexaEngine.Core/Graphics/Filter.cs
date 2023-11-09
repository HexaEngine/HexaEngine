namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies texture filtering modes for sampling textures.
    /// </summary>
    public enum Filter : int
    {
        /// <summary>
        /// Use point sampling for minification, magnification, and mip-level sampling.
        /// </summary>
        MinMagMipPoint = unchecked(0),

        /// <summary>
        /// Use point sampling for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinMagPointMipLinear = unchecked(1),

        /// <summary>
        /// Use point sampling for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinPointMagLinearMipPoint = unchecked(4),

        /// <summary>
        /// Use point sampling for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        MinPointMagMipLinear = unchecked(5),

        /// <summary>
        /// Use linear interpolation for minification, point sampling for magnification, and point sampling for mip-levels.
        /// </summary>
        MinLinearMagMipPoint = unchecked(16),

        /// <summary>
        /// Use linear interpolation for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinLinearMagPointMipLinear = unchecked(17),

        /// <summary>
        /// Use linear interpolation for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinMagLinearMipPoint = unchecked(20),

        /// <summary>
        /// Use linear interpolation for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        MinMagMipLinear = unchecked(21),

        /// <summary>
        /// Use anisotropic filtering for minification, magnification, and mip-level sampling.
        /// </summary>
        Anisotropic = unchecked(85),

        /// <summary>
        /// Use comparison filtering with point sampling for minification, magnification, and mip-level sampling.
        /// </summary>
        ComparisonMinMagMipPoint = unchecked(128),

        /// <summary>
        /// Use comparison filtering with point sampling for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        ComparisonMinMagPointMipLinear = unchecked(129),

        /// <summary>
        /// Use comparison filtering with point sampling for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        ComparisonMinPointMagLinearMipPoint = unchecked(132),

        /// <summary>
        /// Use comparison filtering with point sampling for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        ComparisonMinPointMagMipLinear = unchecked(133),

        /// <summary>
        /// Use comparison filtering with linear interpolation for minification, point sampling for magnification, and point sampling for mip-levels.
        /// </summary>
        ComparisonMinLinearMagMipPoint = unchecked(144),

        /// <summary>
        /// Use comparison filtering with linear interpolation for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        ComparisonMinLinearMagPointMipLinear = unchecked(145),

        /// <summary>
        /// Use comparison filtering with linear interpolation for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        ComparisonMinMagLinearMipPoint = unchecked(148),

        /// <summary>
        /// Use comparison filtering with linear interpolation for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        ComparisonMinMagMipLinear = unchecked(149),

        /// <summary>
        /// Use comparison filtering with anisotropic filtering for minification, magnification, and mip-level sampling.
        /// </summary>
        ComparisonAnisotropic = unchecked(213),

        /// <summary>
        /// Use minimum filtering with point sampling for minification, magnification, and mip-level sampling.
        /// </summary>
        MinimumMinMagMipPoint = unchecked(256),

        /// <summary>
        /// Use minimum filtering with point sampling for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinimumMinMagPointMipLinear = unchecked(257),

        /// <summary>
        /// Use minimum filtering with point sampling for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinimumMinPointMagLinearMipPoint = unchecked(260),

        /// <summary>
        /// Use minimum filtering with point sampling for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        MinimumMinPointMagMipLinear = unchecked(261),

        /// <summary>
        /// Use minimum filtering with linear interpolation for minification, point sampling for magnification, and point sampling for mip-levels.
        /// </summary>
        MinimumMinLinearMagMipPoint = unchecked(272),

        /// <summary>
        /// Use minimum filtering with linear interpolation for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinimumMinLinearMagPointMipLinear = unchecked(273),

        /// <summary>
        /// Use minimum filtering with linear interpolation for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MinimumMinMagLinearMipPoint = unchecked(276),

        /// <summary>
        /// Use minimum filtering with linear interpolation for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        MinimumMinMagMipLinear = unchecked(277),

        /// <summary>
        /// Use minimum filtering with anisotropic filtering for minification, magnification, and mip-level sampling.
        /// </summary>
        MinimumAnisotropic = unchecked(341),

        /// <summary>
        /// Use maximum filtering with point sampling for minification, magnification, and mip-level sampling.
        /// </summary>
        MaximumMinMagMipPoint = unchecked(384),

        /// <summary>
        /// Use maximum filtering with point sampling for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MaximumMinMagPointMipLinear = unchecked(385),

        /// <summary>
        /// Use maximum filtering with point sampling for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MaximumMinPointMagLinearMipPoint = unchecked(388),

        /// <summary>
        /// Use maximum filtering with point sampling for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        MaximumMinPointMagMipLinear = unchecked(389),

        /// <summary>
        /// Use maximum filtering with linear interpolation for minification, point sampling for magnification, and point sampling for mip-levels.
        /// </summary>
        MaximumMinLinearMagMipPoint = unchecked(400),

        /// <summary>
        /// Use maximum filtering with linear interpolation for minification, point sampling and linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MaximumMinLinearMagPointMipLinear = unchecked(401),

        /// <summary>
        /// Use maximum filtering with linear interpolation for minification, linear interpolation for magnification, and point sampling for mip-levels.
        /// </summary>
        MaximumMinMagLinearMipPoint = unchecked(404),

        /// <summary>
        /// Use maximum filtering with linear interpolation for minification, linear interpolation for magnification, and linear interpolation for mip-levels.
        /// </summary>
        MaximumMinMagMipLinear = unchecked(405),

        /// <summary>
        /// Use maximum filtering with anisotropic filtering for minification, magnification, and mip-level sampling.
        /// </summary>
        MaximumAnisotropic = unchecked(469)
    }
}