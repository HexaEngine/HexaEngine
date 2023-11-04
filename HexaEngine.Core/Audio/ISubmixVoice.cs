namespace HexaEngine.Core.Audio
{
    using System;

    /// <summary>
    /// Represents a submix voice used to apply audio effects to a group of audio sources.
    /// </summary>
    public interface ISubmixVoice
    {
        /// <summary>
        /// Gets or sets the gain (volume) of the submix voice.
        /// </summary>
        float Gain { get; set; }

        /// <summary>
        /// Gets the mastering voice to which this submix voice is connected.
        /// </summary>
        IMasteringVoice Master { get; }

        /// <summary>
        /// Gets or sets the name of the submix voice.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Occurs when the gain of the submix voice changes.
        /// </summary>
        event Action<float>? GainChanged;
    }
}