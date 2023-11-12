namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents the usage type of a resource.
    /// </summary>
    public enum Usage
    {
        /// <summary>
        /// Default usage type.
        /// </summary>
        Default = unchecked(0),

        /// <summary>
        /// Immutable usage type.
        /// </summary>
        Immutable = unchecked(1),

        /// <summary>
        /// Dynamic usage type.
        /// </summary>
        Dynamic = unchecked(2),

        /// <summary>
        /// Staging usage type.
        /// </summary>
        Staging = unchecked(3)
    }
}