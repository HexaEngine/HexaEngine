namespace HexaEngine.Core.Audio
{
    using System;

    /// <summary>
    /// Represents a mastering voice used for controlling audio output and its properties.
    /// </summary>
    public interface IMasteringVoice
    {
        /// <summary>
        /// Gets or sets the gain (volume) of the mastering voice.
        /// </summary>
        float Gain { get; set; }

        /// <summary>
        /// Gets the name of the mastering voice.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Occurs when the gain (volume) of the mastering voice changes.
        /// </summary>
        event Action<float>? GainChanged;
    }
}