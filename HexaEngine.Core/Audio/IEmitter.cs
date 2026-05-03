namespace HexaEngine.Core.Audio
{
    using System.Numerics;

    /// <summary>
    /// Represents an audio emitter used for controlling the properties of audio sources.
    /// </summary>
    public interface IEmitter : IDisposable
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
        
        /// <summary>
        /// Gets or sets the attenuation model used to determine how sound decreases in volume over distance.
        /// </summary>
        /// <remarks>Selecting an appropriate attenuation model can affect both the perceived audio
        /// quality and the performance of audio simulations. Different models may be suitable for different
        /// environments or use cases.</remarks>
        AttenuationModel AttenuationModel { get; set; }

        /// <summary>
        /// Gets or sets the directional attenuation factor for the audio emitter.
        /// </summary>
        float DirectionalAttenuationFactor { get; set; }
    }
}