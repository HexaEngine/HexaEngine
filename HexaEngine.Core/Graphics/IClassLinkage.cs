namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a linkage interface for managing shader class instances.
    /// </summary>
    public interface IClassLinkage : IDeviceChild
    {
        /// <summary>
        /// Creates a new class instance with the specified parameters.
        /// </summary>
        /// <param name="name">The name of the class instance.</param>
        /// <param name="cbOffset">The constant buffer offset for the class instance.</param>
        /// <param name="cvOffset">The constant variable offset for the class instance.</param>
        /// <param name="texOffset">The texture offset for the class instance.</param>
        /// <param name="samplerOffset">The sampler offset for the class instance.</param>
        /// <returns>The created class instance.</returns>
        IClassInstance CreateClassInstance(string name, uint cbOffset, uint cvOffset, uint texOffset, uint samplerOffset);

        /// <summary>
        /// Gets a class instance by name and index.
        /// </summary>
        /// <param name="name">The name of the class instance.</param>
        /// <param name="index">The index of the class instance.</param>
        /// <returns>The class instance found, or null if not found.</returns>
        IClassInstance GetClassInstance(string name, uint index);
    }
}