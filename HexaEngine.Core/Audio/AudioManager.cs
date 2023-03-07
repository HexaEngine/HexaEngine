namespace HexaEngine.Core.Audio
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;

    public static class AudioManager
    {
#nullable disable
        private static IAudioContext audioContext;
        private static IAudioDevice audioDevice;
        private static IMasteringVoice master;
        private static Thread streamThread;
#nullable enable
        private static bool running;

        public static IAudioDevice Device => audioDevice;

        public static IAudioContext Context => audioContext;

        public static IMasteringVoice Master => master;

        public static void Initialize(IAudioDevice device)
        {
            audioDevice = device;
            audioContext = audioDevice.Default;
            master = audioDevice.CreateMasteringVoice("Master");
            running = true;
            streamThread = new(ThreadVoid);
            streamThread.Start();
        }

        public static IAudioStream CreateStream(string path)
        {
            return audioDevice.CreateWaveAudioStream(FileSystem.Open(Paths.CurrentSoundPath + path));
        }

        public static ISourceVoice CreateSourceVoice(IAudioStream stream)
        {
            return audioDevice.CreateSourceVoice(stream);
        }

        public static IListener CreateListener()
        {
            return audioDevice.CreateListener(master);
        }

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

        public static void Release()
        {
            running = false;
            streamThread.Join();
            audioContext.Dispose();
            audioDevice.Dispose();
        }
    }
}