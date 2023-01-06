namespace HexaEngine.OpenAL
{
    using Silk.NET.OpenAL;

    public unsafe class AudioDevice : IDisposable
    {
        public const int ALC_HRTF_SOFT = 0x1992;
        public const int AL_SOURCE_SPATIALIZE_SOFT = 0x1214;
        public const int ALC_TRUE = 1;
        public const int ALC_FALSE = 0;

        private readonly List<SourceVoice> sources = new();
        public readonly Device* Device;
        internal readonly AudioContext context;
        private bool disposedValue;
        private AudioContext? current;

        internal AudioDevice(Device* device)
        {
            Device = device;
            var pcontext = alc.CreateContext(Device, null);
            CheckError(Device);
            al.DistanceModel(DistanceModel.InverseDistanceClamped);
            CheckError(Device);
            context = new(this, pcontext);
            Current = context;
        }

        public AudioContext Default => context;

        public AudioContext? Current
        {
            get => current;
            set
            {
                if (current == value) return;
                if (value != null)
                    alc.MakeContextCurrent(value.Context);
                else
                    alc.MakeContextCurrent(null);
                CheckError(Device);
                current = value;
            }
        }

        public bool EnableHRTF(bool enable)
        {
            if (alc.IsExtensionPresent("ALC_SOFT_HRTF"))
            {
                delegate*<Device*, int*, int> alcResetDeviceSOFT = (delegate*<Device*, int*, int>)al.GetProcAddress("alcResetDeviceSOFT");

                int* attributes = Alloc<int>(3);
                attributes[0] = ALC_HRTF_SOFT;
                attributes[1] = enable ? ALC_TRUE : ALC_FALSE;
                attributes[2] = 0;

                var result = alcResetDeviceSOFT(Device, attributes);

                int hrtfenabled = 0;
                delegate*<Device*, int, int, int*, void> alcGetIntegerv = (delegate*<Device*, int, int, int*, void>)al.GetProcAddress("alcGetIntegerv");
                alcGetIntegerv(Device, ALC_HRTF_SOFT, 1, &hrtfenabled);
                Free(attributes);

                return hrtfenabled == 1;
            }
            return false;
        }

        public AudioContext CreateContext()
        {
            var context = alc.CreateContext(Device, null);
            CheckError(Device);
            return new(this, context);
        }

        public MasteringVoice CreateMasteringVoice(string name)
        {
            return new(name);
        }

        public SubmixVoice CreateSubmixVoice(string name, MasteringVoice voice)
        {
            return new(name, voice);
        }

        public WaveAudioStream CreateWaveAudioStream(Stream stream)
        {
            return new(stream);
        }

        public SourceVoice CreateSourceVoice(AudioStream audioStream)
        {
            var source = al.GenSource();
            var sourceVoice = new SourceVoice(source, audioStream);
            sources.Add(sourceVoice);
            return sourceVoice;
        }

        public Listener CreateListener(MasteringVoice voice)
        {
            return new(voice);
        }

        public Emitter CreateEmitter()
        {
            return new();
        }

        public void ProcessAudio()
        {
            for (int i = 0; i < sources.Count; i++)
            {
                sources[i]?.Update();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                alc.CloseDevice(Device);
                disposedValue = true;
            }
        }

        ~AudioDevice()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}