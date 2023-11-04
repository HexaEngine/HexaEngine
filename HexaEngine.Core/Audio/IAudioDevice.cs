namespace HexaEngine.Core.Audio
{
    using System.IO;

    /// <summary>
    /// Represents an audio device that provides audio-related functionality and resources.
    /// </summary>
    public interface IAudioDevice : IDisposable
    {
        /// <summary>
        /// Gets or sets the current audio context associated with the device.
        /// </summary>
        IAudioContext? Current { get; set; }

        /// <summary>
        /// Gets the default audio context provided by the device.
        /// </summary>
        IAudioContext Default { get; }

        /// <summary>
        /// Creates a new audio context for managing audio operations.
        /// </summary>
        /// <returns>An instance of the <see cref="IAudioContext"/> interface representing the new audio context.</returns>
        IAudioContext CreateContext();

        /// <summary>
        /// Creates a new audio emitter for sound positioning and effects.
        /// </summary>
        /// <returns>An instance of the <see cref="IEmitter"/> interface representing the new audio emitter.</returns>
        IEmitter CreateEmitter();

        /// <summary>
        /// Creates a new audio listener for sound positioning and environmental effects.
        /// </summary>
        /// <param name="voice">The mastering voice to attach the listener to.</param>
        /// <returns>An instance of the <see cref="IListener"/> interface representing the new audio listener.</returns>
        IListener CreateListener(IMasteringVoice voice);

        /// <summary>
        /// Creates a mastering voice for controlling the final mixed audio output.
        /// </summary>
        /// <param name="name">A name for the mastering voice.</param>
        /// <returns>An instance of the <see cref="IMasteringVoice"/> interface representing the new mastering voice.</returns>
        IMasteringVoice CreateMasteringVoice(string name);

        /// <summary>
        /// Creates a new audio source voice for playing audio streams.
        /// </summary>
        /// <param name="audioStream">The audio stream to associate with the source voice.</param>
        /// <returns>An instance of the <see cref="ISourceVoice"/> interface representing the new source voice.</returns>
        ISourceVoice CreateSourceVoice(IAudioStream audioStream);

        /// <summary>
        /// Creates a submix voice for processing and mixing audio from multiple source voices.
        /// </summary>
        /// <param name="name">A name for the submix voice.</param>
        /// <param name="voice">The mastering voice to attach the submix voice to.</param>
        /// <returns>An instance of the <see cref="ISubmixVoice"/> interface representing the new submix voice.</returns>
        ISubmixVoice CreateSubmixVoice(string name, IMasteringVoice voice);

        /// <summary>
        /// Creates an audio stream from a <see cref="Stream"/> for playing wave audio data.
        /// </summary>
        /// <param name="stream">The stream containing the wave audio data.</param>
        /// <returns>An instance of the <see cref="IAudioStream"/> interface representing the new audio stream.</returns>
        IAudioStream CreateWaveAudioStream(Stream stream);

        /// <summary>
        /// Processes audio operations, updating the audio context and voice states.
        /// </summary>
        void ProcessAudio();
    }
}