namespace HexaEngine.Core.Audio
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;

    /// <summary>
    /// Provides audio management functionalities such as initializing audio, creating audio streams, and managing audio devices.
    /// </summary>
    public static class AudioManager
    {
#nullable disable
        private static IAudioContext audioContext;
        private static IAudioDevice audioDevice;
        private static IMasteringVoice master;
        private static Thread streamThread;
#nullable enable
        private static bool running;

        /// <summary>
        /// Gets the audio device currently in use.
        /// </summary>
        public static IAudioDevice Device => audioDevice;

        /// <summary>
        /// Gets the audio context associated with the audio device.
        /// </summary>
        public static IAudioContext Context => audioContext;

        /// <summary>
        /// Gets the mastering voice used for audio output.
        /// </summary>
        public static IMasteringVoice Master => master;

        /// <summary>
        /// Initializes the audio manager with the specified audio device.
        /// </summary>
        /// <param name="device">The audio device to use for audio processing.</param>
        public static void Initialize(IAudioDevice device)
        {
            audioDevice = device;
            audioContext = audioDevice.Default;
            master = audioDevice.CreateMasteringVoice("Master");
            running = true;
            streamThread = new(ThreadVoid);
            streamThread.Start();
        }

        /// <summary>
        /// Creates an audio stream from the specified file path.
        /// </summary>
        /// <param name="path">The path to the audio file.</param>
        /// <returns>An audio stream created from the specified audio file.</returns>
        [Obsolete("Use AssetPath overload instead.")]
        public static IAudioStream CreateStream(string path)
        {
            return audioDevice.CreateWaveAudioStream(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Creates an audio stream from the specified file path.
        /// </summary>
        /// <param name="path">The path to the audio file.</param>
        /// <returns>An audio stream created from the specified audio file.</returns>
        public static IAudioStream CreateStream(AssetPath path)
        {
            return audioDevice.CreateWaveAudioStream(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Creates a source voice for playing audio from the provided audio stream.
        /// </summary>
        /// <param name="stream">The audio stream to play.</param>
        /// <returns>A source voice for playing audio from the given audio stream.</returns>
        public static ISourceVoice CreateSourceVoice(IAudioStream stream)
        {
            return audioDevice.CreateSourceVoice(stream);
        }

        /// <summary>
        /// Creates an audio listener for controlling audio output.
        /// </summary>
        /// <returns>An audio listener for controlling audio output.</returns>
        public static IListener CreateListener()
        {
            return audioDevice.CreateListener(master);
        }

        /// <summary>
        /// Creates an audio emitter for emitting audio in the 3D audio space.
        /// </summary>
        /// <returns>An audio emitter for emitting audio in 3D space.</returns>
        public static IEmitter CreateEmitter()
        {
            return audioDevice.CreateEmitter();
        }

        private static void ThreadVoid()
        {
            while (running)
            {
                audioDevice.ProcessAudio();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Releases and disposes of audio resources.
        /// </summary>
        public static void Release()
        {
            running = false;
            streamThread.Join();
            audioContext.Dispose();
            audioDevice.Dispose();
        }
    }
}