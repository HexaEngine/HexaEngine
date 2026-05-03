namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using Hexa.NET.Utilities;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.IO;

    public unsafe class MiniAudioDevice : DisposableRefBase, IAudioDevice, IMiniAudioNode
    {
        private readonly MiniAudioAdapter adapter;
        private MaEngine* engine;
        private MiniAudioListener? activeListener;
        private readonly AudioDeviceDesc desc;
        private readonly List<MiniAudioSound> sounds = [];
        private readonly Lock syncObj = new();
        private readonly List<TaskSyncObject> syncObjects = [];

        public MiniAudioDevice(MiniAudioAdapter adapter, MaEngine* engine, AudioDeviceDesc desc)
        {
            this.adapter = adapter;
            this.engine = engine;
            this.desc = desc;
            engine->PProcessUserData = GCUtils.GCAlloc(this);
        }

        public AudioDeviceDesc Desc => desc;

        public void* Node => MiniAudio.EngineGetEndpoint(engine);

        public nint NativePointer => (nint)engine;

        internal MaEngine* Engine => engine;

        public IListener? ActiveListener
        {
            get => activeListener;
            set
            {
                activeListener?.SetActiveInternal(false);
                activeListener = (MiniAudioListener?)value;
                activeListener?.SetActiveInternal(true);
            }
        }

        public float Gain { get => MiniAudio.EngineGetVolume(engine); set => MiniAudio.EngineSetVolume(engine, value); }

        public IEmitter CreateEmitter()
        {
            return new MiniAudioEmitter();
        }

        public IListener CreateListener()
        {
            return new MiniAudioListener(this);
        }

        public ISound CreateSound(Stream stream, SoundFlags flags)
        {
            return null!;
        }

        public Task<ISound> CreateSoundAsync(Stream stream, SoundFlags flags)
        {
            return Task.FromResult<ISound>(null!);
        }

        public Task<ISound> CreateSoundAsync(in AssetPath path, SoundFlags flags)
        {
            MaSoundFlags maFlags = Helper.Convert(flags);
            maFlags |= MaSoundFlags.FlagAsync;
            var fence = AllocT<MaFence>();
            MiniAudio.FenceInit(fence).CheckError();
            MiniAudioSound sound;
            try
            {
                sound = CreateSoundFromFile(path.Raw, maFlags, fence);
            }
            catch (Exception)
            {
                Free(fence);
                throw;
            }

            TaskCompletionSource<ISound> tcs = new();

            lock (syncObj)
            {
                syncObjects.Add(new TaskSyncObject(fence, tcs, sound));
            }

            return tcs.Task;
        }
        
        public Task<ISound> CreateSoundAsync(in AssetRef assetRef, SoundFlags flags)
        {
            return CreateSoundAsync(assetRef.GetPath() ?? throw new FileNotFoundException($"Asset not found: {assetRef}"), flags);
        }

        public ISound CreateSound(in AssetPath path, SoundFlags flags)
        {
            MaSoundFlags maFlags = Helper.Convert(flags);
            return CreateSoundFromFile(path.Raw, maFlags, null);
        }

        private MiniAudioSound CreateSoundFromFile(string path, MaSoundFlags maFlags, MaFence* fence)
        {
            var pSound = AllocT<MaSound>();
            try
            {
                MiniAudio.SoundInitFromFile(engine, path, (uint)maFlags, null, fence, pSound).CheckError();
                MiniAudio.SoundSetSpatializationEnabled(pSound, 0);
            }
            catch (Exception)
            {
                Free(pSound);
                throw;
            }

            MiniAudioSound soundObj = new(pSound, this);
            lock (syncObj)
            {
                sounds.Add(soundObj);
            }
            return soundObj;
        }

        public ISound CreateSound(in AssetRef assetRef, SoundFlags flags)
        {
            return CreateSound(assetRef.GetPath() ?? throw new FileNotFoundException($"Asset not found: {assetRef}"), flags);
        }

        public ISubmixVoice CreateSubmixVoice(string name)
        {
            var config = MiniAudio.SoundGroupConfigInit2(engine);
            config.Flags |= (uint)(MaSoundFlags.FlagNoDefaultAttachment);

            MaSound group;
            MiniAudio.SoundGroupInitEx(engine, config, &group).CheckError();

            return new SubmixVoice(name, AllocT(group));
        }

        public void Update()
        {
            activeListener?.UpdateState();
            lock (syncObj)
            {
                foreach (var sound in sounds)
                {
                    sound.Update();
                }

                for (int i = syncObjects.Count - 1; i >= 0; i--)
                {
                    var syncObj = syncObjects[i];
                    if (syncObj.IsSignaled())
                    {
                        syncObj.Task.SetResult(syncObj.Result);
                        syncObjects.RemoveAt(i);
                    }
                }
            }
        }

        internal void DestroySound(MiniAudioSound sound)
        {
            lock (syncObj)
            {
                sounds.Remove(sound);
            }
        }

        protected override void DisposeCore()
        {
            if (engine != null)
            {
                MiniAudioSound[] snapshot;
                lock (syncObj)
                {
                    snapshot = [.. sounds];
                }

                foreach (var sound in snapshot)
                {
                    sound.Dispose();
                }
                var self = engine->PProcessUserData;
                MiniAudio.EngineUninit(engine);
                GCUtils.GCFree(self);
                Free(engine);
                engine = null;
            }
        }
    }
}