namespace HexaEngine.MiniAudio
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.MiniAudio;
    using Hexa.NET.Utilities;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.IO;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe class MiniAudioDevice : DisposableRefBase, IAudioDevice, IMiniAudioNode
    {
        private readonly MiniAudioAdapter adapter;
        private MaEngine* engine;
        private MiniAudioListener? activeListener;
        private readonly AudioDeviceDesc desc;
        private readonly List<MiniAudioSound> sounds = [];
        private readonly Lock syncObj = new();

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

        public ISound CreateSound(IAudioStream audioStream)
        {
            return null!;
        }

        public ISound CreateSound(in AssetPath path)
        {
            MaSoundFlags flags = MaSoundFlags.FlagNoDefaultAttachment;
            MaSound sound;
            MiniAudio.SoundInitFromFile(engine, path.Raw, (uint)flags, null, null, &sound).CheckError();

            MiniAudioSound soundObj = new(AllocT(sound));
            lock (syncObj)
            {
                sounds.Add(soundObj);
            }
            return soundObj;
        }

        public ISubmixVoice CreateSubmixVoice(string name)
        {
            var config = MiniAudio.SoundGroupConfigInit2(engine);
            config.Flags |= (uint)MaSoundFlags.FlagNoDefaultAttachment;

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

    public unsafe sealed class MiniAudioListener : IListener
    {
        private Atomic<nuint> dirty;
        private AudioOrientation orientation;
        private Vector3 position;
        private Vector3 velocity;
        private float coneInnerAngle;
        private float coneOuterAngle;
        private float coneOuterGain;
        private readonly MiniAudioDevice device;

        public MiniAudioListener(MiniAudioDevice device)
        {
            this.device = device;
        }

        public bool IsActive { get => device.ActiveListener == this; set => device.ActiveListener = this; }

        public AudioOrientation Orientation { get => orientation; set => SetAndDirty(ref orientation, value); }

        public Vector3 Position { get => position; set => SetAndDirty(ref position, value); }

        public Vector3 Velocity { get => velocity; set => SetAndDirty(ref velocity, value); }

        public float ConeInnerAngle { get => coneInnerAngle; set => SetAndDirty(ref coneInnerAngle, value); }

        public float ConeOuterAngle { get => coneOuterAngle; set => SetAndDirty(ref coneOuterAngle, value); }

        public float ConeOuterGain { get => coneOuterGain; set => SetAndDirty(ref coneOuterGain, value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetAndDirty<T>(ref T field, in T value) where T : IEquatable<T>
        {
            if (field.Equals(value)) return;
            field = value;
            dirty.Store(1);
        }

        internal void SetActiveInternal(bool active)
        {
            if (active)
            {
                MiniAudio.EngineListenerSetEnabled(device.Engine, 0, 1);
                UpdateState();
            }
            else
            {
                MiniAudio.EngineListenerSetEnabled(device.Engine, 0, 0);
            }
        }

        internal void UpdateState()
        {
            if (!dirty.CompareExchange(0, 1))
            {
                return;
            }
            MiniAudio.EngineListenerSetPosition(device.Engine, 0, Position.X, Position.Y, Position.Z);
            MiniAudio.EngineListenerSetDirection(device.Engine, 0, Orientation.At.X, Orientation.At.Y, Orientation.At.Z);
            MiniAudio.EngineListenerSetWorldUp(device.Engine, 0, Orientation.Up.X, Orientation.Up.Y, Orientation.Up.Z);
            MiniAudio.EngineListenerSetVelocity(device.Engine, 0, Velocity.X, Velocity.Y, Velocity.Z);
            MiniAudio.EngineListenerSetCone(device.Engine, 0, ConeInnerAngle, ConeOuterAngle, ConeOuterGain);
        }

        public void Dispose()
        {
            if (IsActive)
            {
                IsActive = false;
            }
        }
    }

    public sealed class MiniAudioEmitter : IEmitter
    {
        internal ulong Version;
        private float coneInnerAngle;
        private float coneOuterAngle;
        private float coneOuterGain;
        private Vector3 direction;
        private float maxDistance;
        private float maxGain;
        private float minGain;
        private Vector3 position;
        private float referenceDistance;
        private float rolloffFactor;
        private Vector3 velocity;

        public float ConeInnerAngle { get => coneInnerAngle; set => SetAndVersion(ref coneInnerAngle, value); }

        public float ConeOuterAngle { get => coneOuterAngle; set => SetAndVersion(ref coneOuterAngle, value); }

        public float ConeOuterGain { get => coneOuterGain; set => SetAndVersion(ref coneOuterGain, value); }

        public Vector3 Direction { get => direction; set => SetAndVersion(ref direction, value); }

        public float MaxDistance { get => maxDistance; set => SetAndVersion(ref maxDistance, value); }

        public float MaxGain { get => maxGain; set => SetAndVersion(ref maxGain, value); }

        public float MinGain { get => minGain; set => SetAndVersion(ref minGain, value); }

        public Vector3 Position { get => position; set => SetAndVersion(ref position, value); }

        public float ReferenceDistance { get => referenceDistance; set => SetAndVersion(ref referenceDistance, value); }

        public float RolloffFactor { get => rolloffFactor; set => SetAndVersion(ref rolloffFactor, value); }

        public Vector3 Velocity { get => velocity; set => SetAndVersion(ref velocity, value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetAndVersion<T>(ref T field, in T value) where T : IEquatable<T>
        {
            if (field.Equals(value)) return;
            field = value;
            Interlocked.Increment(ref Version);
        }

        public void Dispose()
        {
        }
    }

    public unsafe sealed class MiniAudioSound : MiniAudioSoundBase, ISound
    {
        private MiniAudioEmitter? emitter;
        private ISubmixVoice? submix;
        private AudioSourceState state;
        private ulong emitterVersion;

        public MiniAudioSound(MaSound* sound) : base(sound)
        {
        }

        public IEmitter? Emitter
        {
            get => emitter;
            set
            {
                emitter = (MiniAudioEmitter?)value;
                if (emitter == null)
                {
                    MiniAudio.SoundSetSpatializationEnabled(Sound, 0);
                }
                else
                {
                    MiniAudio.SoundSetSpatializationEnabled(Sound, 1);
                    UpdateEmitterState(emitter);
                    emitterVersion = emitter.Version;
                }
            }
        }

        public float Gain { get => MiniAudio.SoundGetVolume(Sound); set => MiniAudio.SoundSetVolume(Sound, value); }

        public bool Looping { get => MiniAudio.SoundIsLooping(Sound) != 0; set => MiniAudio.SoundSetLooping(Sound, value ? 1u : 0u); }

        public float Pitch { get => MiniAudio.SoundGetPitch(Sound); set => MiniAudio.SoundSetPitch(Sound, value); }

        public AudioSourceState State { get => state; private set => OnStateChanged(value); }

        public ISubmixVoice? Submix
        {
            get => submix;
            set
            {
                if (submix == value) return;
                MiniAudio.NodeDetachOutputBus(Sound, 0);
                if (submix is IMiniAudioNode node)
                {
                    MiniAudio.NodeAttachOutputBus(Sound, 0, node.Node, 0);
                }
                submix = value;
            }
        }

        public event Action? Paused;

        public event Action? Playing;

        public event Action? Rewinded;

        public event Action<AudioSourceState>? StateChanged;

        public event Action? Stopped;

        public void Pause()
        {
            MiniAudio.SoundStop(Sound).CheckError();
            State = AudioSourceState.Paused;
        }

        public void Play()
        {
            MiniAudio.SoundStart(Sound).CheckError();
            State = AudioSourceState.Playing;
        }

        public void Rewind()
        {
            MiniAudio.SoundSeekToPcmFrame(Sound, 0).CheckError();
        }

        public void Stop()
        {
            MiniAudio.SoundStop(Sound).CheckError();
            MiniAudio.SoundSeekToPcmFrame(Sound, 0).CheckError();
            State = AudioSourceState.Stopped;
        }

        private void OnStateChanged(AudioSourceState newState)
        {
            if (state == newState)
            {
                return;
            }

            state = newState;
            StateChanged?.Invoke(newState);
            switch (newState)
            {
                case AudioSourceState.Playing:
                    Playing?.Invoke();
                    break;

                case AudioSourceState.Paused:
                    Paused?.Invoke();
                    break;

                case AudioSourceState.Stopped:
                    Stopped?.Invoke();
                    break;
            }
        }

        public void Update()
        {
            if (state != AudioSourceState.Playing)
            {
                return;
            }

            if (!Looping && MiniAudio.SoundAtEnd(Sound) != 0)
            {
                MiniAudio.SoundStop(Sound).CheckError();
                MiniAudio.SoundSeekToPcmFrame(Sound, 0).CheckError();
                State = AudioSourceState.Stopped;
            }

            if (emitter == null)
            {
                return;
            }

            var version = emitter.Version;
            if (version != emitterVersion)
            {
                UpdateEmitterState(emitter);
                emitterVersion = version;
            }
        }

        private void UpdateEmitterState(MiniAudioEmitter emitter)
        {
            MiniAudio.SoundSetPosition(Sound, emitter.Position.X, emitter.Position.Y, emitter.Position.Z);
            MiniAudio.SoundSetDirection(Sound, emitter.Direction.X, emitter.Direction.Y, emitter.Direction.Z);
            MiniAudio.SoundSetCone(Sound, emitter.ConeInnerAngle, emitter.ConeOuterAngle, emitter.ConeOuterGain);
            MiniAudio.SoundSetVelocity(Sound, emitter.Velocity.X, emitter.Velocity.Y, emitter.Velocity.Z);
            MiniAudio.SoundSetMinDistance(Sound, emitter.ReferenceDistance);
            MiniAudio.SoundSetMaxDistance(Sound, emitter.MaxDistance);
            MiniAudio.SoundSetRolloff(Sound, emitter.RolloffFactor);
            MiniAudio.SoundSetMinGain(Sound, emitter.MinGain);
            MiniAudio.SoundSetMaxGain(Sound, emitter.MaxGain);
        }

        protected override void DisposeCore()
        {
            var engine = MiniAudio.SoundGetEngine(Sound);
            var device = GCUtils.GetObject<MiniAudioDevice>(engine.PProcessUserData);
            device.DestroySound(this);
            base.DisposeCore();
        }
    }
}