namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Enum representing different types of constant buffers.
    /// </summary>
    public enum CBufferType
    {
        /// <summary>
        /// Standard constant buffer.
        /// </summary>
        CBuffer = 0,

        /// <summary>
        /// Typed constant buffer.
        /// </summary>
        TBuffer = 1,

        /// <summary>
        /// Interface pointers buffer.
        /// </summary>
        InterfacePointers = 2,

        /// <summary>
        /// Resource binding information buffer.
        /// </summary>
        ResourceBindInfo = 3,
    }
}