namespace HexaEngine.Audio
{
    using HexaEngine.IO;
    using HexaEngine.OpenAL;
    using System;

    public static class AudioManager
    {
        private static AudioContext audioContext;
        private static AudioDevice audioDevice;
        private static MasteringVoice master;
        private static Thread streamThread;
        private static bool running;

        public static AudioDevice Device => audioDevice;

        public static AudioContext Context => audioContext;

        public static MasteringVoice Master => master;

        public static void Initialize()
        {
            audioDevice = AudioSystem.CreateAudioDevice(null);
            audioContext = audioDevice.Default;
            master = audioDevice.CreateMasteringVoice("Master");
            running = true;
            streamThread = new(ThreadVoid);
            streamThread.Start();
        }

        public static WaveAudioStream CreateStream(string path)
        {
            return audioDevice.CreateWaveAudioStream(FileSystem.Open(Paths.CurrentSoundPath + path));
        }

        public static SourceVoice CreateSourceVoice(WaveAudioStream stream)
        {
            return audioDevice.CreateSourceVoice(stream);
        }

        public static Listener CreateListener()
        {
            return audioDevice.CreateListener(master);
        }

        public static Emitter CreateEmitter()
        {
            return new();
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