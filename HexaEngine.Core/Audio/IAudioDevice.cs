namespace HexaEngine.Core.Audio
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO;

    [Flags]
    public enum SoundFlags
    {
        None = 0,

        /// <summary>
        /// Indicates that the audio data should be streamed from disk rather than loaded entirely into memory. This is useful for large audio files, such as music tracks or long sound effects, that may not fit comfortably in memory. When this flag is set, the audio data will be read and decoded in smaller chunks as needed during playback, rather than being fully loaded at once.
        /// </summary>
        Streaming = 1 << 0,

        /// <summary>
        /// Causes the audio data to be decoded immediately upon loading, rather than being streamed or decoded on demand. This can reduce latency when playing the sound for the first time, but may increase memory usage if the sound is large.
        /// </summary>
        Decode = 1 << 1,

        /// <summary>
        /// Indicates that the length of the audio data is unknown at the time of loading. This can be useful for certain types of audio sources, such as live streams or procedurally generated audio, where the total duration may not be known in advance. When this flag is set, the audio system will handle the sound as if it has an indefinite length, allowing it to play continuously until stopped.
        /// </summary>
        UnknownLength = 1 << 2,
    }

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

        ISound CreateSound(Stream stream, SoundFlags flags);

        ISound CreateSound(in AssetPath path, SoundFlags flags);

        ISound CreateSound(in AssetRef assetRef, SoundFlags flags);

        Task<ISound> CreateSoundAsync(Stream stream, SoundFlags flags);

        Task<ISound> CreateSoundAsync(in AssetPath path, SoundFlags flags);

        Task<ISound> CreateSoundAsync(in AssetRef assetRef, SoundFlags flags);

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