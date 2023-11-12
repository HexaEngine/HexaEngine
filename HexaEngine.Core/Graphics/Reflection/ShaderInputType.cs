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
        SitCbuffer = 0,

        /// <summary>
        /// Typed buffer.
        /// </summary>
        SitTbuffer = 1,

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
        SitUavRwtyped = 4,

        /// <summary>
        /// Structured buffer.
        /// </summary>
        SitStructured = 5,

        /// <summary>
        /// UAV with raw structured access.
        /// </summary>
        SitUavRwstructured = 6,

        /// <summary>
        /// Byte address buffer.
        /// </summary>
        SitByteaddress = 7,

        /// <summary>
        /// UAV with raw byte address access.
        /// </summary>
        SitUavRwbyteaddress = 8,

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
        SitUavRwstructuredWithCounter = 11,

        /// <summary>
        /// Ray tracing acceleration structure.
        /// </summary>
        SitRtaccelerationstructure = 12,

        /// <summary>
        /// UAV with feedback texture access.
        /// </summary>
        SitUavFeedbacktexture = 13,
    }
}