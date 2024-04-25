namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using System.Numerics;

    public class OpenALSourceVoice : ISourceVoice
    {
        public const int SourceSpatializeSoft = 0x1214;
        private readonly uint source;
        private readonly IAudioStream stream;
        private SourceState state;
        private bool disposedValue;
        private ISubmixVoice? submix;

        private float pitch = 1;
        private float gain = 1;

        internal OpenALSourceVoice(uint sourceVoice, IAudioStream audioStream)
        {
            source = sourceVoice;
            stream = audioStream;
            al.SetSourceProperty(source, SourceFloat.ReferenceDistance, 1);
            al.SetSourceProperty(source, SourceFloat.MaxDistance, float.PositiveInfinity);
            al.SetSourceProperty(source, SourceFloat.RolloffFactor, 1);
            al.SetSourceProperty(source, SourceFloat.Pitch, 1);
            al.SetSourceProperty(source, SourceFloat.Gain, 1);
            al.SetSourceProperty(source, SourceFloat.MinGain, 0);
            al.SetSourceProperty(source, SourceFloat.MaxGain, 1);
            al.SetSourceProperty(source, SourceFloat.ConeInnerAngle, 360);
            al.SetSourceProperty(source, SourceFloat.ConeOuterAngle, 360);
            al.SetSourceProperty(source, SourceFloat.ConeOuterGain, 0);
            al.SetSourceProperty(source, SourceVector3.Position, Vector3.Zero);
            al.SetSourceProperty(source, SourceVector3.Velocity, Vector3.Zero);
            al.SetSourceProperty(source, SourceVector3.Direction, Vector3.Zero);
            al.SetSourceProperty(source, SourceBoolean.Looping, false);
            al.SetSourceProperty(source, (SourceBoolean)SourceSpatializeSoft, true);
            audioStream.Initialize(source);
        }

        public ISubmixVoice? Submix
        {
            get => submix;
            set
            {
                if (submix == value)
                {
                    return;
                }

                if (submix != null)
                {
                    submix.GainChanged -= Submix_GainChanged;
                }
                if (value != null)
                {
                    value.GainChanged += Submix_GainChanged;
                }
                submix = value;
            }
        }

        public IAudioStream Buffer => stream;

        /// <summary>
        /// Specify the pitch to be applied, either at Source, or on mixer results, at Listener.
        /// Range: [0.5f - 2.0f] Default: 1.0f.
        /// </summary>
        public float Pitch
        {
            get => pitch;
            set
            {
                pitch = value;
                al.SetSourceProperty(source, SourceFloat.Pitch, value);
            }
        }

        /// <summary>
        /// Indicate the gain (volume amplification) applied. Type: float. Range: [0.0f -
        /// ? ] A value of 1.0 means un-attenuated/unchanged. Each division by 2 equals an
        /// attenuation of -6dB. Each multiplicaton with 2 equals an amplification of +6dB.
        /// A value of 0.0f is meaningless with respect to a logarithmic scale; it is interpreted
        /// as zero volume - the channel is effectively disabled.
        /// </summary>
        public float Gain
        {
            get => gain;
            set
            {
                gain = value;
                if (submix != null)
                {
                    al.SetSourceProperty(source, SourceFloat.Gain, value * submix.Gain);
                }
                else
                {
                    al.SetSourceProperty(source, SourceFloat.Gain, value);
                }
            }
        }

        public bool Looping
        {
            get => stream.Looping;
            set
            {
                stream.Looping = value;
            }
        }

        public AudioSourceState State => Convert(state);

        public event Action<AudioSourceState>? OnStateChanged;

        public event Action? OnPlay;

        public event Action? OnPause;

        public event Action? OnRewind;

        public event Action? OnStop;

        public IEmitter? Emitter { get; set; }

        public void Update()
        {
            if (Emitter != null)
            {
                al.SetSourceProperty(source, SourceFloat.ReferenceDistance, Emitter.ReferenceDistance);
                al.SetSourceProperty(source, SourceFloat.MaxDistance, Emitter.MaxDistance);
                al.SetSourceProperty(source, SourceFloat.RolloffFactor, Emitter.RolloffFactor);
                al.SetSourceProperty(source, SourceFloat.MinGain, Emitter.MinGain);
                al.SetSourceProperty(source, SourceFloat.MaxGain, Emitter.MaxGain);
                al.SetSourceProperty(source, SourceFloat.ConeInnerAngle, Emitter.ConeInnerAngle);
                al.SetSourceProperty(source, SourceFloat.ConeOuterAngle, Emitter.ConeOuterAngle);
                al.SetSourceProperty(source, SourceFloat.ConeOuterGain, Emitter.ConeOuterGain);
                al.SetSourceProperty(source, SourceVector3.Position, Emitter.Position);
                al.SetSourceProperty(source, SourceVector3.Velocity, Emitter.Velocity);
                al.SetSourceProperty(source, SourceVector3.Direction, Emitter.Direction);
            }

            al.GetSourceProperty(source, GetSourceInteger.SourceState, out int stateValue);
            var newState = (SourceState)stateValue;
            if (newState != state)
            {
                state = newState;
                OnStateChanged?.Invoke(Convert(state));
            }
            if (state == SourceState.Playing)
            {
                stream.Update(source);
            }
        }

        public void Play()
        {
            al.SourcePlay(source);
            OnPlay?.Invoke();
        }

        public void Stop()
        {
            al.SourceStop(source);
            stream.Reset();
            OnStop?.Invoke();
        }

        public void Pause()
        {
            al.SourcePause(source);
            OnPause?.Invoke();
        }

        public void Rewind()
        {
            al.SourceRewind(source);
            stream.Reset();
            OnRewind?.Invoke();
        }

        private void Submix_GainChanged(float submixGain)
        {
            al.SetSourceProperty(source, SourceFloat.Gain, gain * submixGain);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                al.DeleteBuffer(source);
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