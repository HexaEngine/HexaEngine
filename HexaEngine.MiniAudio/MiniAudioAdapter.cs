namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;

    public unsafe class MiniAudioAdapter : DisposableBase, IAudioAdapterEx
    {
        private MaContext* context;
        private MaDeviceId[] playbackDeviceIds = null!;
        private MaDeviceId[] captureDeviceIds = null!;
        private readonly List<AudioDeviceInfo> playbackDeviceInfos = [];
        private readonly List<AudioDeviceInfo> captureDeviceInfos = [];
        private MiniAudioVFSLayer? layer;
        private MaResourceManager* resourceManager;

        public MiniAudioAdapter()
        {
            context = AllocT<MaContext>();
            *context = default;
            MiniAudio.ContextInit(null, 0, null, context).CheckError();
            RefreshDevices();
        }

        public AudioBackend Backend => AudioBackend.MiniAudio;

        public int PlatformScore => 100;

        public IReadOnlyList<AudioDeviceInfo> PlaybackDevices => playbackDeviceInfos;

        public IReadOnlyList<AudioDeviceInfo> CaptureDevices => captureDeviceInfos;

        public void RefreshDevices()
        {
            playbackDeviceInfos.Clear();
            captureDeviceInfos.Clear();
            MaDeviceInfo* playbackDevices, captureDevices;
            uint playbackDeviceCount, captureDeviceCount;
            MiniAudio.ContextGetDevices(context, &playbackDevices, &playbackDeviceCount, &captureDevices, &captureDeviceCount).CheckError();

            playbackDeviceIds = new MaDeviceId[(int)playbackDeviceCount];
            captureDeviceIds = new MaDeviceId[(int)captureDeviceCount];
            for (uint i = 0; i < playbackDeviceCount; i++)
            {
                var device = &playbackDevices[i];
                var name = ToStringFromUTF8(&device->Name_0) ?? string.Empty;
                playbackDeviceInfos.Add(new AudioDeviceInfo
                {
                    Id = i + 1, // offset by one because 0 is reserved for default device
                    Name = name,
                    IsDefault = device->IsDefault != 0
                });
                playbackDeviceIds[i] = device->Id;
            }

            for (uint i = 0; i < captureDeviceCount; i++)
            {
                var device = &captureDevices[i];
                var name = ToStringFromUTF8(&device->Name_0) ?? string.Empty;
                captureDeviceInfos.Add(new AudioDeviceInfo
                {
                    Id = i + 1, // offset by one because 0 is reserved for default device
                    Name = name,
                    IsDefault = device->IsDefault != 0
                });
                captureDeviceIds[i] = device->Id;
            }
        }

        public IAudioDevice CreateAudioDevice(string? name)
        {
            throw new NotSupportedException();
        }

        public List<string> GetAvailableDevices()
        {
            throw new NotSupportedException();
        }

        public static void Init()
        {
            AudioAdapter.Adapters.Add(new MiniAudioAdapter());
        }

        public void InitInstance()
        {
            layer = new();
            MaResourceManagerConfig config = MiniAudio.ResourceManagerConfigInit();
            config.PVFS = layer.Callbacks;

            resourceManager = AllocT<MaResourceManager>();
            *resourceManager = default;
            MiniAudio.ResourceManagerInit(config, resourceManager).CheckError();
        }

        public IAudioDevice CreatePlaybackDevice(in AudioDeviceDesc desc)
        {
            MaEngineConfig engineConfig = MiniAudio.EngineConfigInit();
            engineConfig.PResourceManager = resourceManager;
            engineConfig.Channels = desc.Channels;
            engineConfig.SampleRate = desc.SampleRate;

            if (desc.Id != 0)
            {
                var devId = playbackDeviceIds[(int)desc.Id - 1];
                engineConfig.PPlaybackDeviceID = &devId;
            }

            MaEngine* engine = AllocT<MaEngine>();
            try
            {
                MiniAudio.EngineInit(engineConfig, engine).CheckError();
            }
            catch (Exception)
            {
                Free(engine);
                throw;
            }

            return new MiniAudioDevice(this, engine, desc);
        }

        protected override void DisposeCore()
        {
            if (resourceManager != null)
            {
                MiniAudio.ResourceManagerUninit(resourceManager);
                Free(resourceManager);
                resourceManager = null;
            }

            if (layer != null)
            {
                layer.Dispose();
                layer = null;
            }

            if (context != null)
            {
                MiniAudio.ContextUninit(context);
                Free(context);
                context = null;
            }
        }
    }
}