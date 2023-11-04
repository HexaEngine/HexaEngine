namespace HexaEngine.Core.Audio
{
    /// <summary>
    /// Represents an audio context that encapsulates platform-specific audio functionality.
    /// </summary>
    public interface IAudioContext : IDisposable
    {
        /// <summary>
        /// Gets the native pointer associated with the audio context.
        /// </summary>
        nint NativePointer { get; }
    }
}