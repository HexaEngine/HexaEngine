namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using Silk.NET.OpenAL;

    public unsafe class OpenALAudioDevice : IAudioDevice
    {
        public const int ALC_HRTF_SOFT = 0x1992;
        public const int AL_SOURCE_SPATIALIZE_SOFT = 0x1214;
        public const int ALC_TRUE = 1;
        public const int ALC_FALSE = 0;

        private readonly List<OpenALSourceVoice> sources = new();
        public readonly Device* Device;
        internal readonly OpenALAudioContext context;
        private bool disposedValue;
        private OpenALAudioContext? current;

        internal OpenALAudioDevice(Device* device)
        {
            Device = device;
            var pcontext = alc.CreateContext(Device, null);
            CheckError(Device);
            al.DistanceModel(DistanceModel.InverseDistanceClamped);
            CheckError(Device);
            context = new(this, pcontext);
            Current = context;
        }

        public IAudioContext Default => context;

        public IAudioContext? Current
        {
            get => current;
            set
            {
                if (value is not OpenALAudioContext context) return;
                if (current == value) return;
                if (value != null)
                    alc.MakeContextCurrent(context.Context);
                else
                    alc.MakeContextCurrent(null);
                CheckError(Device);
                current = context;
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

        public IAudioContext CreateContext()
        {
            var context = alc.CreateContext(Device, null);
            CheckError(Device);
            return new OpenALAudioContext(this, context);
        }

        public IMasteringVoice CreateMasteringVoice(string name)
        {
            return new OpenALMasteringVoice(name);
        }

        public ISubmixVoice CreateSubmixVoice(string name, IMasteringVoice voice)
        {
            return new OpenALSubmixVoice(name, voice);
        }

        public IAudioStream CreateWaveAudioStream(Stream stream)
        {
            return new OpenALWaveAudioStream(stream);
        }

        public ISourceVoice CreateSourceVoice(IAudioStream audioStream)
        {
            var source = al.GenSource();
            var sourceVoice = new OpenALSourceVoice(source, audioStream);
            sources.Add(sourceVoice);
            return sourceVoice;
        }

        public IListener CreateListener(IMasteringVoice voice)
        {
            return new OpenALListener(voice);
        }

        public IEmitter CreateEmitter()
        {
            return new OpenALEmitter();
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

        ~OpenALAudioDevice()
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