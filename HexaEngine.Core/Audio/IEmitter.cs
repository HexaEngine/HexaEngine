namespace HexaEngine.Core.Audio
{
    using System.Numerics;

    /// <summary>
    /// Represents an audio emitter used for controlling the properties of audio sources.
    /// </summary>
    public interface IEmitter
    {
        /// <summary>
        /// Gets or sets the inner angle of the cone in radians for sound directionality.
        /// </summary>
        float ConeInnerAngle { get; set; }

        /// <summary>
        /// Gets or sets the outer angle of the cone in radians for sound directionality.
        /// </summary>
        float ConeOuterAngle { get; set; }

        /// <summary>
        /// Gets or sets the gain outside of the outer cone for sound directionality.
        /// </summary>
        float ConeOuterGain { get; set; }

        /// <summary>
        /// Gets or sets the direction of the audio emitter.
        /// </summary>
        Vector3 Direction { get; set; }

        /// <summary>
        /// Gets or sets the maximum distance at which the audio is audible.
        /// </summary>
        float MaxDistance { get; set; }

        /// <summary>
        /// Gets or sets the maximum gain for the audio emitter.
        /// </summary>
        float MaxGain { get; set; }

        /// <summary>
        /// Gets or sets the minimum gain for the audio emitter.
        /// </summary>
        float MinGain { get; set; }

        /// <summary>
        /// Gets or sets the position of the audio emitter.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the reference distance for the audio emitter.
        /// </summary>
        float ReferenceDistance { get; set; }

        /// <summary>
        /// Gets or sets the rolloff factor for attenuation of the audio emitter.
        /// </summary>
        float RolloffFactor { get; set; }

        /// <summary>
        /// Gets or sets the velocity of the audio emitter.
        /// </summary>
        Vector3 Velocity { get; set; }
    }
}