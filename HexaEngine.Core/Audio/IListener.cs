namespace HexaEngine.Core.Audio
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents an audio listener used for controlling the audio environment and listener properties.
    /// </summary>
    public interface IListener : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the audio listener is active or inactive.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the audio listener in 3D space.
        /// </summary>
        AudioOrientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the position of the audio listener in 3D space.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the velocity of the audio listener in 3D space.
        /// </summary>
        Vector3 Velocity { get; set; }
    }
}