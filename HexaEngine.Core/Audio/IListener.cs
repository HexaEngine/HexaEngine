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

        /// <summary>
        /// Gets or sets the inner angle of the listener's directional cone in radians.
        /// </summary>
        public float ConeInnerAngle { get; set; }

        /// <summary>
        /// Gets or sets the outer angle of the listener's directional cone in radians.
        /// </summary>
        public float ConeOuterAngle { get; set; }

        /// <summary>
        /// Gets or sets the gain applied to sounds outside the listener's directional cone, where 0.0 means no sound and 1.0 means full volume.
        /// </summary>
        public float ConeOuterGain { get; set; }
    }
}