namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a sampler state object that defines how textures are sampled during shader execution.
    /// </summary>
    public interface ISamplerState : IDeviceChild
    {
        /// <summary>
        /// Gets the description of this sampler state object.
        /// </summary>
        SamplerStateDescription Description { get; }
    }
}