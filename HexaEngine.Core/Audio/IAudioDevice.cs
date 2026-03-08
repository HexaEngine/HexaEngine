namespace HexaEngine.Core.Audio
{
    using HexaEngine.Core.IO;
    using System.IO;

    /// <summary>
    /// Represents an audio device that provides audio-related functionality and resources.
    /// </summary>
    public interface IAudioDevice : IDisposable, IAudioInputNode
    {
        /// <summary>
        /// Creates a new audio emitter for sound positioning and effects.
        /// </summary>
        /// <returns>An instance of the <see cref="IEmitter"/> interface representing the new audio emitter.</returns>
        IEmitter CreateEmitter();

        /// <summary>
        /// Creates a new audio listener for sound positioning and environmental effects.
        /// </summary>
        /// <returns>An instance of the <see cref="IListener"/> interface representing the new audio listener.</returns>
        IListener CreateListener();

        ISound CreateSound(IAudioStream audioStream);

        ISound CreateSound(in AssetPath path);

        /// <summary>
        /// Creates a submix voice for processing and mixing audio from multiple source voices.
        /// </summary>
        /// <param name="name">A name for the submix voice.</param>
        /// <returns>An instance of the <see cref="ISubmixVoice"/> interface representing the new submix voice.</returns>
        ISubmixVoice CreateSubmixVoice(string name);

        void Update();

        float Gain { get; set; }
    }
}