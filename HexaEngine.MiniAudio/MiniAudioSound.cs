namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Audio;

    public unsafe sealed class MiniAudioSound : MiniAudioSoundBase, ISound
    {
        private MiniAudioEmitter? emitter;
        private IAudioInputNode? submix;
        private AudioSourceState state;
        private ulong emitterVersion;

        public MiniAudioSound(MaSound* sound, IAudioInputNode? submix) : base(sound)
        {
            this.submix = submix;
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

        public float Pan { get => MiniAudio.SoundGetPan(Sound); set => MiniAudio.SoundSetPan(Sound, value); }

        public PanMode PanMode { get => Helper.ConvertBack(MiniAudio.SoundGetPanMode(Sound)); set => MiniAudio.SoundSetPanMode(Sound, Helper.Convert(value)); }

        public AudioSourceState State { get => state; private set => OnStateChanged(value); }

        public IAudioInputNode? Submix
        {
            get => submix;
            set
            {
                if (submix == value) return;
                if (submix != null)
                {
                    MiniAudio.NodeDetachOutputBus(Sound, 0).CheckError();
                }

                if (value is IMiniAudioNode node)
                {
                    MiniAudio.NodeAttachOutputBus(Sound, 0, node.Node, 0).CheckError();
                }
                submix = value;
            }
        }

        public event EventHandler<AudioSourceState>? StateChanged;

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
            StateChanged?.Invoke(this, newState);
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
            MiniAudio.SoundSetPosition(Sound, emitter.Position.X, emitter.Position.Y, -emitter.Position.Z);
            MiniAudio.SoundSetDirection(Sound, emitter.Direction.X, emitter.Direction.Y, -emitter.Direction.Z);
            MiniAudio.SoundSetCone(Sound, emitter.ConeInnerAngle, emitter.ConeOuterAngle, emitter.ConeOuterGain);
            MiniAudio.SoundSetVelocity(Sound, emitter.Velocity.X, emitter.Velocity.Y, -emitter.Velocity.Z);
            MiniAudio.SoundSetMinDistance(Sound, emitter.ReferenceDistance);
            MiniAudio.SoundSetMaxDistance(Sound, emitter.MaxDistance);
            MiniAudio.SoundSetRolloff(Sound, emitter.RolloffFactor);
            MiniAudio.SoundSetMinGain(Sound, emitter.MinGain);
            MiniAudio.SoundSetMaxGain(Sound, emitter.MaxGain);
            MiniAudio.SoundSetDopplerFactor(Sound, emitter.DopplerEffectFactor);
            MiniAudio.SoundSetAttenuationModel(Sound, Helper.Convert(emitter.AttenuationModel));
            MiniAudio.SoundSetDirectionalAttenuationFactor(Sound, emitter.DirectionalAttenuationFactor);
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