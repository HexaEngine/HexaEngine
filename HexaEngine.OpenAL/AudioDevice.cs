namespace HexaEngine.OpenAL
{
    using Silk.NET.OpenAL;

    public unsafe class AudioDevice : IDisposable
    {
        public const int ALC_HRTF_SOFT = 0x1992;
        public const int AL_SOURCE_SPATIALIZE_SOFT = 0x1214;
        public const int ALC_TRUE = 1;

        private readonly List<SourceVoice> sources = new();
        public readonly Device* Device;
        internal readonly AudioContext context;
        private bool disposedValue;
        private AudioContext? current;

        internal AudioDevice(Device* device)
        {
            Device = device;
            int* attributes = Alloc<int>(4);
            attributes[0] = ALC_HRTF_SOFT;
            attributes[1] = ALC_TRUE;
            attributes[2] = 0;

            var pcontext = alc.CreateContext(Device, attributes);
            Free(attributes);
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

        public AudioStream CreateAudioStream(Stream stream)
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