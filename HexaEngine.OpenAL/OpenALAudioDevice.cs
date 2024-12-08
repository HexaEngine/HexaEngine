namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using Hexa.NET.OpenAL;

    public unsafe class OpenALAudioDevice : IAudioDevice
    {
        public const int ALC_HRTF_SOFT = 0x1992;
        public const int AL_SOURCE_SPATIALIZE_SOFT = 0x1214;
        public const int ALC_TRUE = 1;
        public const int ALC_FALSE = 0;

        private readonly List<OpenALSourceVoice> sources = new();
        public readonly ALCdevice* Device;
        internal readonly OpenALAudioContext context;
        private bool disposedValue;
        private OpenALAudioContext? current;

        internal OpenALAudioDevice(ALCdevice* device)
        {
            Device = device;
            var pcontext = OpenAL.CreateContext(Device, null);
            CheckError(Device);
            OpenAL.DistanceModel(ALEnum.InverseDistanceClamped);
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
                if (value is not OpenALAudioContext context)
                {
                    return;
                }

                if (current == value)
                {
                    return;
                }

                if (value != null)
                {
                    OpenAL.MakeContextCurrent(context.Context);
                }
                else
                {
                    OpenAL.MakeContextCurrent(null);
                }

                CheckError(Device);
                current = context;
            }
        }

        public bool EnableHRTF(bool enable)
        {
            if (OpenAL.IsExtensionPresent(null, "ALC_SOFT_HRTF") != 0)
            {
                delegate*<ALCdevice*, int*, int> alcResetDeviceSOFT = (delegate*<ALCdevice*, int*, int>)OpenAL.GetProcAddress("alcResetDeviceSOFT");

                int* attributes = AllocT(3);
                attributes[0] = ALC_HRTF_SOFT;
                attributes[1] = enable ? ALC_TRUE : ALC_FALSE;
                attributes[2] = 0;

                var result = alcResetDeviceSOFT(Device, attributes);

                int hrtfenabled = 0;
                OpenAL.GetIntegerv(Device, ALC_HRTF_SOFT, 1, &hrtfenabled);
                Free(attributes);

                return hrtfenabled == 1;
            }
            return false;
        }

        public IAudioContext CreateContext()
        {
            var context = OpenAL.CreateContext(Device, null);
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
            uint source;
            OpenAL.GenSources(1, &source);
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
                OpenAL.CloseDevice(Device);
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}