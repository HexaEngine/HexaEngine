namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Represents the types of shader inputs in HLSL.
    /// </summary>
    public enum ShaderInputType
    {
        /// <summary>
        /// Constant buffer.
        /// </summary>
        SitCBuffer = 0,

        /// <summary>
        /// Typed buffer.
        /// </summary>
        SitTBuffer = 1,

        /// <summary>
        /// Texture.
        /// </summary>
        SitTexture = 2,

        /// <summary>
        /// Sampler.
        /// </summary>
        SitSampler = 3,

        /// <summary>
        /// UAV with raw typed access.
        /// </summary>
        SitUavRwTyped = 4,

        /// <summary>
        /// Structured buffer.
        /// </summary>
        SitStructured = 5,

        /// <summary>
        /// UAV with raw structured access.
        /// </summary>
        SitUavRwStructured = 6,

        /// <summary>
        /// Byte address buffer.
        /// </summary>
        SitByteAddress = 7,

        /// <summary>
        /// UAV with raw byte address access.
        /// </summary>
        SitUavRwByteAddress = 8,

        /// <summary>
        /// UAV with append structured access.
        /// </summary>
        SitUavAppendStructured = 9,

        /// <summary>
        /// UAV with consume structured access.
        /// </summary>
        SitUavConsumeStructured = 10,

        /// <summary>
        /// UAV with raw structured access and counter.
        /// </summary>
        SitUavRwStructuredWithCounter = 11,

        /// <summary>
        /// Ray tracing acceleration structure.
        /// </summary>
        SitRtAccelerationStructure = 12,

        /// <summary>
        /// UAV with feedback texture access.
        /// </summary>
        SitUavFeedbackTexture = 13,
    }
}