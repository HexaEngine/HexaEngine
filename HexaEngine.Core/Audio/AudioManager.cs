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
        private static IAudioDevice audioDevice;
        private static Thread streamThread;
#nullable enable
        private static bool running;

        /// <summary>
        /// Gets the audio device currently in use.
        /// </summary>
        public static IAudioDevice Device => audioDevice;

        /// <summary>
        /// Initializes the audio manager with the specified audio device.
        /// </summary>
        /// <param name="device">The audio device to use for audio processing.</param>
        public static void Initialize(IAudioDevice device)
        {
            audioDevice = device;
            running = true;
            streamThread = new(ThreadVoid);
            streamThread.Start();
        }

        /// <summary>
        /// Creates a new sound source for audio playback using the specified asset path.
        /// </summary>
        /// <remarks>The audio device must be initialized before calling this method. An exception may be
        /// thrown if the asset path is invalid or if the audio device is not ready.</remarks>
        /// <param name="path">The asset path that identifies the location of the audio asset to be used for the sound source. The path
        /// must refer to a valid and accessible audio resource.</param>
        /// <returns>An instance of ISound that represents the created sound source, which can be used to control audio playback.</returns>
        public static ISound CreateSourceVoice(in AssetPath path)
        {
            return audioDevice.CreateSound(path);
        }

        /// <summary>
        /// Creates an audio listener for controlling audio output.
        /// </summary>
        /// <returns>An audio listener for controlling audio output.</returns>
        public static IListener CreateListener()
        {
            return audioDevice.CreateListener();
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
                audioDevice.Update();
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
            audioDevice.Dispose();
        }
    }
}